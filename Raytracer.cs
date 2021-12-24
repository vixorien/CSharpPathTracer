using System;
using System.Diagnostics;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using System.Windows.Forms;
using System.Drawing;

namespace CSharpPathTracer
{
	struct RaytracingStats
	{
		public ulong TotalRays { get; set; }
		public int DeepestRecursion { get; set; }
		public int MaxRecursion { get; set; }
		public TimeSpan TotalTime { get; set; }

		public void Reset()
		{
			TotalRays = 0;
			DeepestRecursion = 0;
			MaxRecursion = 0;
			TotalTime = TimeSpan.Zero;
		}
	}

	class RaytracingParameters
	{
		public Scene Scene { get; set; }
		public Bitmap RenderTarget { get; set; }
		public Camera Camera { get; set; }
		public int SamplesPerPixel { get; set; }
		public int MaxRecursionDepth { get; set; }

		public RaytracingParameters(Scene scene, Bitmap renderTarget, Camera camera, int samplesPerPixel, int maxRecursionDepth)
		{
			Scene = scene;
			RenderTarget = renderTarget;
			Camera = camera;
			SamplesPerPixel = samplesPerPixel;
			MaxRecursionDepth = maxRecursionDepth;
		}
	}

	class Raytracer
	{
		public delegate void CompleteDelegate(RaytracingStats stats);
		public event CompleteDelegate RaytraceComplete;

		public delegate void PixelDelegate(int x, int y);
		public event PixelDelegate RaytracePixelComplete;

		public delegate void ScanlineDelegate(int y, RaytracingStats stats);
		public event ScanlineDelegate RaytraceScanlineComplete;

		private Random rng;
		
		private RaytracingStats stats;

		private const float GammaCorrectionPower = 1.0f / 2.2f;

		public Raytracer()
		{
			// Other setup
			rng = new Random();
		}

		public void RaytraceScene(object threadParam)
		{
			RaytracingParameters rtParams = threadParam as RaytracingParameters;
			if (rtParams == null)
				return;

			int width = rtParams.RenderTarget.Width;
			int height = rtParams.RenderTarget.Height;

			// Reset stats
			stats.Reset();
			stats.MaxRecursion = rtParams.MaxRecursionDepth;
			Stopwatch sw = new Stopwatch();
			sw.Start();
			
			// Loop through pixels
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					// Multiple samples per pixel
					Vector3 totalColor = Vector3.Zero;
					for (int s = 0; s < rtParams.SamplesPerPixel; s++)
					{
						float adjustedX = x + rng.NextFloat(-0.5f, 0.5f);
						float adjustedY = y + rng.NextFloat(-0.5f, 0.5f);

						// Get this ray and add to the total raytraced color
						Ray ray = rtParams.Camera.GetRayThroughPixel(adjustedX, adjustedY, width, height);
						totalColor += TraceRay(ray, rtParams.Scene, rtParams.MaxRecursionDepth);
					}
					SetColor(rtParams.RenderTarget, x, y, totalColor / rtParams.SamplesPerPixel);
					
					// Notify that this pixel is complete
					RaytracePixelComplete?.Invoke(x, y);
				}

				// Update after an entire line
				stats.TotalTime = sw.Elapsed;
				RaytraceScanlineComplete?.Invoke(y, stats);
				Application.DoEvents();
			}

			sw.Stop();
			stats.TotalTime = sw.Elapsed;

			// All done
			RaytraceComplete?.Invoke(stats);
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
			if (scene.ClosestHit(ray, out hit))
			{
				Ray newRay;

				// We found a hit; which kind of material?
				if (hit.Entity.Material.Metal)
				{
					// Metal - perfect reflection
					Vector3 reflection = Vector3.Reflect(ray.Direction, hit.Normal);
					newRay = new Ray(hit.Position, reflection);
				}
				else
				{
					// Non-metal, so diffuse!
					newRay = new Ray(hit.Position, rng.NextVectorInHemisphere(hit.Normal));
				}

				// Take into account the hit color and trace the next ray
				return hit.Entity.Material.Color * TraceRay(newRay, scene, depth - 1);
			}
			else
			{
				// No hit so use environment gradient
				return scene.Environment.GetColorFromDirection(ray.Direction);
			}
		}


		private void SetColor(Bitmap bitmap, int x, int y, Vector3 color, bool gammaCorrect = true)
		{
			if (gammaCorrect)
			{
				color = new Vector3(
					MathF.Pow(color.X, GammaCorrectionPower),
					MathF.Pow(color.Y, GammaCorrectionPower),
					MathF.Pow(color.Z, GammaCorrectionPower));
			}

			bitmap.SetPixel(x, y, color.ToSystemColor());
		}

		private void SetColor(Bitmap bitmap, int x, int y, System.Drawing.Color color, bool gammaCorrect = true)
		{
			SetColor(bitmap, x, y, color.ToVector3(), gammaCorrect);
		}

	}
}
