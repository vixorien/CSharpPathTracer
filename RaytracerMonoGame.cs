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

	/// <summary>
	/// Progress reported after a single scanline is finished in a monogame raytrace
	/// </summary>
	class RaytracingProgressMonoGame
	{
		public int ScanlineIndex { get; private set; }
		public int ScanlineWidth { get; private set; }
		public SIMD.Vector4[] Pixels { get; private set; }
		public double CompletionPercent { get; private set; }
		public RaytracingStats Stats { get; private set; }

		public RaytracingProgressMonoGame(int scanlineIndex, int scanlineWidth, SIMD.Vector4[] pixels, double completionPercent, RaytracingStats stats)
		{
			ScanlineIndex = scanlineIndex;
			ScanlineWidth = scanlineWidth;
			Pixels = pixels;
			CompletionPercent = completionPercent;
			Stats = stats;
		}
	}

	class RaytracerMonoGame : Raytracer
	{
		// Results
		private SIMD.Vector4[] pixels; // Must be 1D for later MonoGame texture SetData call
		private SIMD.Vector4[] progressivePixels;

		/// <summary>
		/// Creates a new raytracer for monogame
		/// </summary>
		public RaytracerMonoGame() { }

		/// <summary>
		/// Helper for converting from 2D indices to a 1D index
		/// </summary>
		/// <param name="x">X index</param>
		/// <param name="y">Y index</param>
		/// <param name="width">Width of the array</param>
		/// <returns>A 1D index</returns>
		private int Index(int x, int y, int width)
		{
			return y * width + x;
		}

		/// <summary>
		/// Raytraces a scene for a background worker object
		/// </summary>
		public void RaytraceSceneBackgroundWorker(object sender, DoWorkEventArgs e)
		{
			// Make sure a background worker invoked this method
			BackgroundWorker worker = sender as BackgroundWorker;
			if (worker == null)
				return;

			// Ensure we have proper parameters
			RaytracingParameters rtParams = e.Argument as RaytracingParameters;
			if (rtParams == null)
				return;

			// Verify size
			int fullWidth = rtParams.Width;
			int fullHeight = rtParams.Height;
			int finalWidth = fullWidth / rtParams.ResolutionReduction;
			int finalHeight = fullHeight / rtParams.ResolutionReduction;
			if (fullWidth <= 0 || fullHeight <= 0 ||
				finalWidth <= 0 || finalHeight <= 0)
				return;

			// Get ready
			stats.Reset(rtParams.MaxRecursionDepth);

			// Do we need to set up a new array of results?
			int totalPixels = finalWidth * finalHeight;
			if (pixels == null || pixels.Length != totalPixels || progressivePixels.Length != totalPixels)
			{
				pixels = new SIMD.Vector4[totalPixels];
				progressivePixels = new SIMD.Vector4[totalPixels];
			}

			// Reset arrays
			Array.Clear(pixels, 0, pixels.Length);
			Array.Clear(progressivePixels, 0, progressivePixels.Length);

			// Values we'll be using for rays
			int progressiveSteps = rtParams.Progressive ? rtParams.SamplesPerPixel : 1;
			int raysPerPixel = rtParams.Progressive ? 1 : rtParams.SamplesPerPixel;

			// Progressive loop (as necessary)
			for (int p = 0; p < progressiveSteps; p++)
			{
				// Loop through scanlines
				for (int y = 0; y < finalHeight; y++)
				{
					// Loop through pixels on a scanline (parallel so it's async)
					Parallel.For(0, finalWidth, x =>
					{
						// Multiple samples per pixel
						SIMD.Vector3 totalColor = SIMD.Vector3.Zero;
						for (int s = 0; s < raysPerPixel; s++)
						{
							float adjustedX = x + ThreadSafeRandom.Instance.NextFloat(-0.5f, 0.5f);
							float adjustedY = y + ThreadSafeRandom.Instance.NextFloat(-0.5f, 0.5f);

							// Get this ray and add to the total raytraced color
							Ray ray = rtParams.Camera.GetRayThroughPixel(adjustedX, adjustedY, finalWidth, finalHeight);
							totalColor += TraceRay(ray, rtParams.Scene, rtParams.MaxRecursionDepth);
						}

						// Average the color and set the resolution block
						if (raysPerPixel > 1)
							totalColor /= raysPerPixel;

						// Add to the progressive color and average
						int pixelIndex = Index(x, y, finalWidth);
						progressivePixels[pixelIndex] += new SIMD.Vector4(totalColor, 1);
						SIMD.Vector4 average = progressivePixels[pixelIndex] / (p + 1);

						// Gamma correct the averaged color as we store
						pixels[pixelIndex] = new SIMD.Vector4(
							MathF.Pow(average.X, GammaCorrectionPower),
							MathF.Pow(average.Y, GammaCorrectionPower),
							MathF.Pow(average.Z, GammaCorrectionPower),
							1);
					});

					// Check for cancellation
					if (worker.CancellationPending)
					{
						e.Cancel = true;
						return;
					}

					// Report each scanline that was completed
					double onePassPercentage = 1.0 / progressiveSteps;
					double percentComplete = (double)p / progressiveSteps;
					percentComplete += ((double)y / finalHeight) * onePassPercentage;

					RaytracingProgressMonoGame progress = new RaytracingProgressMonoGame(
						y,
						finalWidth,
						pixels,
						percentComplete,
						stats);
					worker.ReportProgress((int)progress.CompletionPercent, progress);
				}
			}
		}
	}
}
