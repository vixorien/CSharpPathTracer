using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
//using System.Drawing;

namespace CSharpPathTracer
{
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

	class RaytracingProgress
	{
		public int ScanlineIndex { get; private set; }
		public byte[] Scanline { get; private set; }
		public double CompletionPercent { get; private set; }
		public RaytracingStats Stats { get; private set; }

		public RaytracingProgress(int scanlineIndex, byte[] scanline, double completionPercent, RaytracingStats stats)
		{
			ScanlineIndex = scanlineIndex;
			Scanline = scanline;
			CompletionPercent = completionPercent;
			Stats = stats;
		}
	}

	class Raytracer
	{
		private Random rng;

		private bool raytracingInProgress;
		private RaytracingStats stats;

		private const float GammaCorrectionPower = 1.0f / 2.2f;

		public Raytracer()
		{
			raytracingInProgress = false;
			rng = new Random();
		}

		public void RaytraceScene(object sender, DoWorkEventArgs e)
		{
			if (raytracingInProgress)
				return;

			RaytracingParameters rtParams = e.Argument as RaytracingParameters;
			if (rtParams == null)
				return;

			BackgroundWorker worker = sender as BackgroundWorker;
			if (worker == null)
				return;

			// Get ready
			stats.Reset(rtParams.MaxRecursionDepth);
			raytracingInProgress = true;

			int width = rtParams.Width;
			int height = rtParams.Height;

			// Set up the results as separate scanline
			// arrays of colors to easily faciliate returning
			// a single scanline of colors at a time
			const int channelsPerPixel = 3;
			byte[][] results = new byte[height][];
			for (int h = 0; h < height; h++)
			{
				results[h] = new byte[width * channelsPerPixel];
			}

			int res = rtParams.ResolutionReduction;

			// Loop through pixels
			for (int y = 0; y < height; y += res)
			{
				// TODO: Fix random w/ parallel for: https://devblogs.microsoft.com/pfxteam/getting-random-numbers-in-a-thread-safe-way/
				//Parallel.For(0, width, x =>

				for (int x = 0; x < width; x += res)
				{
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
					for (int blockY = y; blockY < y + res && blockY < height; blockY++)
						for (int blockX = x; blockX < x + res && blockX < width; blockX++)
							FinalizeColor(results, blockX, blockY, channelsPerPixel, totalColor, true);
				}
				//});

				// Check for cancellation
				if (worker.CancellationPending)
				{
					e.Cancel = true;
					raytracingInProgress = false;
					return;
				}

				// Report each scanline that was completed
				// TODO: Make this one report with a scanline count!
				for (int blockY = y; blockY < y + res && blockY < height; blockY++)
				{
					RaytracingProgress progress = new RaytracingProgress(blockY, results[blockY], (double)blockY / height * 100, stats);
					worker.ReportProgress((int)progress.CompletionPercent, progress);
				}
			}

			// Finished
			raytracingInProgress = false;
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
				Ray newRay;
				Entity hitEntity = hit.HitObject as Entity;

				// We found a hit; which kind of material?
				if (hitEntity.Material.Metal)
				{
					// Metal - perfect reflection
					Vector3 reflection = Vector3.Reflect(ray.Direction, hit.Normal);
					newRay = new Ray(hit.Position, reflection, ray.TMin, ray.TMax);
				}
				else
				{
					Random localRNG = ThreadSafeRandom.Instance;
					// Non-metal, so diffuse!
					newRay = new Ray(hit.Position, localRNG.NextVectorInHemisphere(hit.Normal), ray.TMin, ray.TMax);
				}

				// Take into account the hit color and trace the next ray
				return hitEntity.Material.Color * TraceRay(newRay, scene, depth - 1);
			}
			else
			{
				// No hit so use environment gradient
				return scene.Environment.GetColorFromDirection(ray.Direction);
			}
		}

		private void FinalizeColor(byte[][] results, int x, int y, int channelsPerPixel, Vector3 color, bool gammaCorrect = true)
		{
			if (gammaCorrect)
			{
				color.X = MathF.Pow(Math.Clamp(color.X, 0, 1), GammaCorrectionPower);
				color.Y = MathF.Pow(Math.Clamp(color.Y, 0, 1), GammaCorrectionPower);
				color.Z = MathF.Pow(Math.Clamp(color.Z, 0, 1), GammaCorrectionPower);
			}

			// Note: bitmap data is BGR, not RGB!
			int pixelStart = x * channelsPerPixel;
			results[y][pixelStart + 0] = (byte)(color.Z * 255); 
			results[y][pixelStart + 1] = (byte)(color.Y * 255);
			results[y][pixelStart + 2] = (byte)(color.X * 255);
		}

	}
}
