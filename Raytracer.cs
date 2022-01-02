using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;


namespace CSharpPathTracer
{
	/// <summary>
	/// Parameters to begin a new raytrace
	/// </summary>
	class RaytracingParameters
	{
		public Scene Scene { get; set; }
		public Camera Camera { get; set; }
		public int SamplesPerPixel { get; set; }
		public int ResolutionReduction { get; set; }
		public int MaxRecursionDepth { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }

		public RaytracingParameters(Scene scene, Camera camera, int width, int height, int samplesPerPixel, int resolutionReduction, int maxRecursionDepth)
		{
			Scene = scene;
			Width = width;
			Height = height;
			Camera = camera;
			SamplesPerPixel = samplesPerPixel;
			ResolutionReduction = resolutionReduction;
			MaxRecursionDepth = maxRecursionDepth;
		}
	}

	/// <summary>
	/// Progress reported after a single scanline is finished
	/// </summary>
	class RaytracingProgress
	{
		public int ScanlineIndex { get; private set; }
		public int ScanlineDuplicateCount { get; private set; }
		public byte[] Scanline { get; private set; }
		public double CompletionPercent { get; private set; }
		public RaytracingStats Stats { get; private set; }

		public RaytracingProgress(int scanlineIndex, int duplicateCount, byte[] scanline, double completionPercent, RaytracingStats stats)
		{
			ScanlineIndex = scanlineIndex;
			ScanlineDuplicateCount = duplicateCount;
			Scanline = scanline;
			CompletionPercent = completionPercent;
			Stats = stats;
		}
	}

	/// <summary>
	/// Details on a single raytrace
	/// </summary>
	struct RaytracingStats
	{
		public ulong TotalRays { get; set; }
		public int DeepestRecursion { get; set; }
		public int MaxRecursion { get; set; }

		public void Reset(int maxRecursion)
		{
			TotalRays = 0;
			DeepestRecursion = 0;
			MaxRecursion = maxRecursion;
		}
	}

	/// <summary>
	/// Handles raytracing a scene
	/// </summary>
	class Raytracer
	{
		private RaytracingStats stats;
		private byte[][] pixels;

		public const int ChannelsPerPixel = 3;

		private const float GammaCorrectionPower = 1.0f / 2.2f;

		public Raytracer()
		{
		}

		public void RaytraceScene(object sender, DoWorkEventArgs e)
		{
			RaytracingParameters rtParams = e.Argument as RaytracingParameters;
			if (rtParams == null || rtParams.Width == 0 || rtParams.Height == 0)
				return;

			BackgroundWorker worker = sender as BackgroundWorker;
			if (worker == null)
				return;

			// Get ready
			stats.Reset(rtParams.MaxRecursionDepth);

			int width = rtParams.Width;
			int height = rtParams.Height;

			// Do we need to set up a new array of results?
			if (pixels == null || pixels.Length != height || pixels[0].Length != width * ChannelsPerPixel)
			{
				// Set up the results as separate scanline
				// arrays of colors to easily faciliate returning
				// a single scanline of colors at a time
				pixels = new byte[height][];
				for (int h = 0; h < height; h++)
				{
					pixels[h] = new byte[width * ChannelsPerPixel];
				}
			}

			// Figure out the resolution, and adjust by half so we
			// calculate the "center" of the large pixel
			int res = rtParams.ResolutionReduction;
			int half = res / 2;

			// Loop through scanlines
			for (int y = half; y < height; y += res)
			{
				// Loop through pixels on a scanline (parallel so it's async)
				Parallel.For(0, (int)Math.Ceiling((double)width / res), xIteration =>
				{
					// The actual coordinate to use (adjusted by half due to resolution reduction)
					int x = xIteration * res + half;

					// Multiple samples per pixel
					Vector3 totalColor = Vector3.Zero;
					for (int s = 0; s < rtParams.SamplesPerPixel; s++)
					{
						float adjustedX = x + ThreadSafeRandom.Instance.NextFloat(-0.5f, 0.5f);
						float adjustedY = y + ThreadSafeRandom.Instance.NextFloat(-0.5f, 0.5f);

						// Get this ray and add to the total raytraced color
						Ray ray = rtParams.Camera.GetRayThroughPixel(adjustedX, adjustedY, width, height);
						totalColor += TraceRay(ray, rtParams.Scene, rtParams.MaxRecursionDepth);
					}

					// Average the color and set the resolution block
					totalColor /= rtParams.SamplesPerPixel;

					// Prepare to set the color in the array of bytes
					Vector3 colorAsBytes = ColorAsBytes(ref totalColor, true);
					for (int blockX = x - half; blockX < x + res - half && blockX < width; blockX++)
						SetByteColor(pixels, blockX, y, ref colorAsBytes);
				});

				// Check for cancellation
				if (worker.CancellationPending)
				{
					e.Cancel = true;
					return;
				}

				// Report each scanline that was completed
				RaytracingProgress progress = new RaytracingProgress(y - half, res, pixels[y], (double)y / height * 100, stats);
				worker.ReportProgress((int)progress.CompletionPercent, progress);
			}



		}


		private Vector3 TraceRay(Ray ray, Scene scene, int depth)
		{
			// Check depth for stats
			if (stats.MaxRecursion - depth > stats.DeepestRecursion)
				stats.DeepestRecursion = stats.MaxRecursion - depth;

			// Have we gone too far?
			if (depth <= 0) return Vector3.Zero;

			// Using the ray; update stats
			stats.TotalRays++;

			// Get closest hit along this ray
			RayHit hit;
			if (scene.RayIntersection(ray, out hit))
			{
				// Grab the entity from the hit
				Entity hitEntity = hit.HitObject as Entity;

				// How is this ray bouncing?
				Ray newRay = hitEntity.Material.GetNextBounce(ray, hit);

				// Take into account the hit color and trace the next ray
				return hitEntity.Material.GetColorAtUV(hit.UV) * TraceRay(newRay, scene, depth - 1);
			}
			else
			{
				// No hit so use environment
				return scene.Environment.GetColorFromDirection(ray.Direction);
			}
		}


		private Vector3 ColorAsBytes(ref Vector3 color, bool gammaCorrect = true)
		{
			if (gammaCorrect)
			{
				color.X = MathF.Pow(Math.Clamp(color.X, 0, 1), GammaCorrectionPower);
				color.Y = MathF.Pow(Math.Clamp(color.Y, 0, 1), GammaCorrectionPower);
				color.Z = MathF.Pow(Math.Clamp(color.Z, 0, 1), GammaCorrectionPower);
			}

			// Adjust to the byte range
			return color * 255;
		}

		private void SetByteColor(byte[][] results, int x, int y, ref Vector3 colorInBytes)
		{
			// Note: bitmap data is BGR, not RGB!
			int pixelStart = x * ChannelsPerPixel;
			results[y][pixelStart + 0] = (byte)colorInBytes.Z;
			results[y][pixelStart + 1] = (byte)colorInBytes.Y;
			results[y][pixelStart + 2] = (byte)colorInBytes.X;
		}

	}
}
