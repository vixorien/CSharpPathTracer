using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System.Windows.Forms;
using System.Drawing;

namespace CSharpRaytracing
{
	class Raytracer
	{
		public event Action RaytraceComplete;

		public delegate void PixelDelegate(int x, int y);
		public event PixelDelegate RaytracePixelComplete;

		public delegate void ScanlineDelegate(int y);
		public event ScanlineDelegate RaytraceScanlineComplete;

		private Random rng;
		private List<Sphere> scene;
		private Environment environment;

		private const float GammaCorrectionPower = 1.0f / 2.2f;

		public Raytracer(Environment environment)
		{
			// Save params
			this.environment = environment;

			// Other setup
			rng = new Random();
			scene = new List<Sphere>();

			// Materials ===
			Material grayMatte = new Material(System.Drawing.Color.Gray.ToVector3(), false);
			Material greenMatte = new Material(new Vector3(0.2f, 1.0f, 0.2f), false);
			Material mirror = new Material(new Vector3(1, 1, 1), true);
			Material gold = new Material(new Vector3(1.000f, 0.766f, 0.336f), true);

			// Set up scene ===

			// Large sphere below
			scene.Add(new Sphere(new Vector3(0, -1000, 0), 1000, grayMatte));

			// Small spheres in front of camera
			scene.Add(new Sphere(new Vector3(-5, 2.0f, 0), 2.0f, greenMatte));
			scene.Add(new Sphere(new Vector3(0, 4.0f, 0), 2.0f, mirror));
			scene.Add(new Sphere(new Vector3(5, 2.0f, 0), 2.0f, gold));

			// Random floating spheres
			for (int i = 0; i < 25; i++)
			{
				scene.Add(new Sphere(
					new Vector3(rng.NextFloat(-10, 10), rng.NextFloat(0, 5), rng.NextFloat(-10, 10)),
					rng.NextFloat(0.5f, 3.0f),
					new Material(rng.NextColor(), rng.NextBool())));
			}
		}

		public void RaytraceScene(Bitmap output, Camera camera, ProgressBar progress, int samplesPerPixel, int maxRecursionDepth)
		{
			int width = output.Width;
			int height = output.Height;

			// Set up progress bar
			progress.Minimum = 0;
			progress.Maximum = width * height;
			progress.Value = 0;

			// Loop through pixels
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					// Multiple samples per pixel
					Vector3 totalColor = Vector3.Zero;
					for (int s = 0; s < samplesPerPixel; s++)
					{
						float adjustedX = x + rng.NextFloat(-0.5f, 0.5f);
						float adjustedY = y + rng.NextFloat(-0.5f, 0.5f);

						// Get this ray and add to the total raytraced color
						Ray ray = camera.GetRayThroughPixel(adjustedX, adjustedY, width, height);
						totalColor += TraceRay(ray, maxRecursionDepth);
					}
					SetColor(output, x, y, totalColor / samplesPerPixel);
					
					// Update form
					progress.IncrementNoAnimation(1);
					RaytracePixelComplete?.Invoke(x, y);
					Application.DoEvents();
				}

				// Update after an entire line
				RaytraceScanlineComplete?.Invoke(y);
			}

			// All done
			RaytraceComplete?.Invoke();
		}

		private Vector3 TraceRay(Ray ray, int depth)
		{
			// Have we gone too far?
			if (depth <= 0) return Vector3.Zero;

			// Get closest hit along this ray
			SphereIntersection hit;
			if (GetClosestSceneHit(ray, out hit))
			{
				Ray newRay;

				// We found a hit; which kind of material?
				if (hit.Sphere.Material.Metal)
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
				return hit.Sphere.Material.Color * TraceRay(newRay, depth - 1);
			}
			else
			{
				// No hit so use environment gradient
				return environment.GetColorFromDirection(ray.Direction);
			}
		}


		private bool GetClosestSceneHit(Ray ray, out SphereIntersection hit)
		{
			// No hits yet
			bool anyHit = false;
			hit = new SphereIntersection(Vector3.Zero, Vector3.Zero, float.PositiveInfinity, Sphere.Null);

			// Loop through scene and check all spheres
			foreach (Sphere sphere in scene)
			{
				SphereIntersection[] currentHits;
				if (ray.Intersect(sphere, out currentHits))
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
