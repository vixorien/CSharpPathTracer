using System;
using System.Diagnostics;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using System.Windows.Forms;
using System.Drawing;

namespace CSharpRaytracing
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
		public Bitmap RenderTarget { get; set; }
		public Camera Camera { get; set; }
		public int SamplesPerPixel { get; set; }
		public int MaxRecursionDepth { get; set; }

		public RaytracingParameters(Bitmap renderTarget, Camera camera, int samplesPerPixel, int maxRecursionDepth)
		{
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
		private List<Geometry> scene;
		private Environment environment;

		private RaytracingStats stats;

		private const float GammaCorrectionPower = 1.0f / 2.2f;

		public Raytracer(Environment environment)
		{
			// Save params
			this.environment = environment;

			// Other setup
			rng = new Random();
			scene = new List<Geometry>();

			// Materials ===
			Material grayMatte = new Material(System.Drawing.Color.Gray.ToVector3(), false);
			Material greenMatte = new Material(new Vector3(0.2f, 1.0f, 0.2f), false);
			Material blueMatte = new Material(new Vector3(0.2f, 0.2f, 1.0f), false);
			Material mirror = new Material(new Vector3(1, 1, 1), true);
			Material gold = new Material(new Vector3(1.000f, 0.766f, 0.336f), true);

			// Set up scene ===

			// Try out a cube
			Mesh cubeMesh = new Mesh("Content/Models/cube.obj", blueMatte);
			scene.Add(cubeMesh);

			// Large sphere below
			scene.Add(new Sphere(new Vector3(0, -1000, 0), 1000, grayMatte));

			// Small spheres in front of camera
			scene.Add(new Sphere(new Vector3(-5, 2.0f, 0), 2.0f, greenMatte));
			scene.Add(new Sphere(new Vector3(0, 4.0f, 0), 2.0f, mirror));
			scene.Add(new Sphere(new Vector3(5, 2.0f, 0), 2.0f, gold));

			// Random floating spheres
			//for (int i = 0; i < 25; i++)
			//{
			//	scene.Add(new Sphere(
			//		new Vector3(rng.NextFloat(-10, 10), rng.NextFloat(0, 5), rng.NextFloat(-10, 10)),
			//		rng.NextFloat(0.25f, 1.0f),
			//		new Material(rng.NextColor(), rng.NextBool())));
			//}
		}

		public void RaytraceScene(object threadParam) //Bitmap output, Camera camera, int samplesPerPixel, int maxRecursionDepth
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
						totalColor += TraceRay(ray, rtParams.MaxRecursionDepth);
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

		private Vector3 TraceRay(Ray ray, int depth)
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
			if (GetClosestSceneHit(ray, out hit))
			{
				Ray newRay;

				// We found a hit; which kind of material?
				if (hit.Geometry.Material.Metal)
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
				return hit.Geometry.Material.Color * TraceRay(newRay, depth - 1);
			}
			else
			{
				// No hit so use environment gradient
				return environment.GetColorFromDirection(ray.Direction);
			}
		}


		private bool GetClosestSceneHit(Ray ray, out RayHit hit)
		{
			// No hits yet
			bool anyHit = false;
			hit = RayHit.Infinity;

			// Loop through scene and check all spheres
			foreach (Geometry geo in scene)
			{
				RayHit[] currentHits;
				if(geo.RayIntersection(ray, out currentHits))
				{
					// We have a hit; was it closest?
					if (currentHits[0].Distance < hit.Distance)
					{
						hit = currentHits[0];
						anyHit = true;
					}
				}
			}

			return anyHit;
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
