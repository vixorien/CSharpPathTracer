using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SIMD = System.Numerics;


namespace CSharpPathTracer
{
	struct RaytracingResults
	{
		public int Width { get; set; }
		public int Height { get; set; }
		public SIMD.Vector4[] Pixels { get; set; }
		public RaytracingStats Stats { get; set; }
		public bool Success { get; set; }

		public RaytracingResults(int width, int height, SIMD.Vector4[] pixels, RaytracingStats stats)
		{
			Width = width;
			Height = height;
			Pixels = pixels;
			Stats = stats;
			Success = true;
		}
	}

	class RaytracerMonoGame : Raytracer
	{
		// Results
		private SIMD.Vector4[] pixels; // Must be 1D for later MonoGame texture SetData call
		//private SIMD.Vector4[] currentScanline;

		/// <summary>
		/// Creates a new raytracer for monogame
		/// </summary>
		public RaytracerMonoGame() { }


		public RaytracingResults RaytraceScene(RaytracingParameters rtParams)
		{
			// Verify size
			int fullWidth = rtParams.Width;
			int fullHeight = rtParams.Height;
			int finalWidth = fullWidth / rtParams.ResolutionReduction;
			int finalHeight = fullHeight / rtParams.ResolutionReduction;
			if (fullWidth <= 0 || fullHeight <= 0 ||
				finalWidth <= 0 || finalHeight <= 0)
				return new RaytracingResults();

			// Need to update the pixel array?
			int totalPixels = finalWidth * finalHeight;
			if (pixels == null || pixels.Length != totalPixels)
			{
				pixels = new SIMD.Vector4[totalPixels];
			}

			// Reset stats for this trace
			stats.Reset(rtParams.MaxRecursionDepth);

			// Loop through scanlines
			for (int y = 0; y < finalHeight; y++)
			{
				// Parallel loop for the pixels on the current scanline
				Parallel.For(0, finalWidth, x =>
				{
					// Handle multiple samples per pixel
					SIMD.Vector3 totalColor = SIMD.Vector3.Zero;
					for (int s = 0; s < rtParams.SamplesPerPixel; s++)
					{
						float adjustedX = x + ThreadSafeRandom.Instance.NextFloat(-0.5f, 0.5f);
						float adjustedY = y + ThreadSafeRandom.Instance.NextFloat(-0.5f, 0.5f);
						
						// Get this ray and add to the total raytraced color
						Ray ray = rtParams.Camera.GetRayThroughPixel(adjustedX, adjustedY, finalWidth, finalHeight);
						totalColor += TraceRay(ray, rtParams.Scene, rtParams.MaxRecursionDepth);
					}

					// Average the color and set the resolution block
					if (rtParams.SamplesPerPixel > 1)
						totalColor /= rtParams.SamplesPerPixel;

					// Replace color in array
					pixels[Index(x,y,finalWidth)] = new SIMD.Vector4(
						MathF.Pow(totalColor.X, GammaCorrectionPower),
						MathF.Pow(totalColor.Y, GammaCorrectionPower),
						MathF.Pow(totalColor.Z, GammaCorrectionPower), 
						1);
				});
			}

			// Report the final details
			return new RaytracingResults(
				finalWidth,
				finalHeight,
				pixels,
				stats);
		}

		public RaytracingResults RaytraceScanline(RaytracingParameters rtParams, int scanlineIndex)
		{
			// Verify size
			int fullWidth = rtParams.Width;
			int fullHeight = rtParams.Height;
			int finalWidth = fullWidth / rtParams.ResolutionReduction;
			int finalHeight = fullHeight / rtParams.ResolutionReduction;
			if (fullWidth <= 0 || fullHeight <= 0 ||
				finalWidth <= 0 || finalHeight <= 0)
				return new RaytracingResults();

			// Reset stats for this trace
			stats.Reset(rtParams.MaxRecursionDepth);

			// Create the scanline vector
			SIMD.Vector4[] scanline = new SIMD.Vector4[finalWidth];

			// Parallel loop for the pixels on the current scanline
			Parallel.For(0, finalWidth, x =>
			{
				// Handle multiple samples per pixel
				SIMD.Vector3 totalColor = SIMD.Vector3.Zero;
				for (int s = 0; s < rtParams.SamplesPerPixel; s++)
				{
					float adjustedX = x + ThreadSafeRandom.Instance.NextFloat(-0.5f, 0.5f);
					float adjustedY = scanlineIndex + ThreadSafeRandom.Instance.NextFloat(-0.5f, 0.5f);

					// Get this ray and add to the total raytraced color
					Ray ray = rtParams.Camera.GetRayThroughPixel(adjustedX, adjustedY, finalWidth, finalHeight);
					totalColor += TraceRay(ray, rtParams.Scene, rtParams.MaxRecursionDepth);
				}

				// Average the color and set the resolution block
				if (rtParams.SamplesPerPixel > 1)
					totalColor /= rtParams.SamplesPerPixel;

				// Replace color in array
				scanline[x] = new SIMD.Vector4(
					MathF.Pow(totalColor.X, GammaCorrectionPower),
					MathF.Pow(totalColor.Y, GammaCorrectionPower),
					MathF.Pow(totalColor.Z, GammaCorrectionPower),
					1);
			});

			// Report the final details
			return new RaytracingResults(
				finalWidth,
				1,
				scanline,
				stats);
		}

		private int Index(int x, int y, int width)
		{
			return y * width + x;
		}
	}
}
