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

	class Raytracer
	{
		private RaytracingStats stats;

		private const float GammaCorrectionPower = 1.0f / 2.2f;

		public Raytracer()
		{
		}

		public void RaytraceScene(object sender, DoWorkEventArgs e)
		{
			RaytracingParameters rtParams = e.Argument as RaytracingParameters;
			if (rtParams == null)
				return;

			BackgroundWorker worker = sender as BackgroundWorker;
			if (worker == null)
				return;

			// Get ready
			stats.Reset(rtParams.MaxRecursionDepth);

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
				Parallel.For(0, (int)Math.Ceiling((double)width / res), xIteration =>
				{
					// The actual coordinate to use
					int x = xIteration * res;
					
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
					Vector3 colorAsBytes = ColorAsBytes(totalColor, true);
					for (int blockX = x; blockX < x + res && blockX < width; blockX++)
						SetByteColor(results, blockX, y, channelsPerPixel, colorAsBytes);
				});

				// Check for cancellation
				if (worker.CancellationPending)
				{
					e.Cancel = true;
					return;
				}

				// Report each scanline that was completed
				RaytracingProgress progress = new RaytracingProgress(y, res, results[y], (double)y / height * 100, stats);
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
				Ray newRay;
				Entity hitEntity = hit.HitObject as Entity;

				// We found a hit; which kind of material?
				if (hitEntity.Material.Transparent)
				{
					// Grab the index of refraction, which needs to be
					// inverted if we're OUTSIDE the object
					float indexOfRefraction = hitEntity.Material.IndexOfRefraction;
					if (hit.Side == HitSide.Outside)
						indexOfRefraction = 1.0f / indexOfRefraction;

					// Random chance for reflection instead of refraction based on Fresnel
					float NdotV = Vector3.Dot(-1 * ray.Direction, hit.Normal);
					bool reflectFresnel = FresnelSchlick(NdotV, indexOfRefraction) > ThreadSafeRandom.Instance.NextFloat();

					// Test for refraction
					Vector3 newDir;
					if (reflectFresnel || !Refract(ray.Direction, hit.Normal, indexOfRefraction, out newDir))
						newDir = Vector3.Reflect(ray.Direction, hit.Normal);

					// Create the new ray based on either reflection or refraction
					newRay = new Ray(hit.Position, newDir, ray.TMin, ray.TMax);
				}
				else if (hitEntity.Material.Metal)
				{
					// Metal - perfect reflection
					Vector3 reflection = Vector3.Reflect(ray.Direction, hit.Normal);

					// Adjust based on roughness
					reflection += ThreadSafeRandom.Instance.NextVector3() * hitEntity.Material.GetRoughnessAtUV(hit.UV);

					newRay = new Ray(hit.Position, reflection, ray.TMin, ray.TMax);
				}
				else
				{
					// Non-metal, so diffuse!
					newRay = new Ray(hit.Position, ThreadSafeRandom.Instance.NextVectorInHemisphere(hit.Normal), ray.TMin, ray.TMax);	
				}

				// Take into account the hit color and trace the next ray
				return hitEntity.Material.GetColorAtUV(hit.UV) * TraceRay(newRay, scene, depth - 1);
			}
			else
			{
				// No hit so use environment gradient
				return scene.Environment.GetColorFromDirection(ray.Direction);
			}
		}

	
		private bool Refract(Vector3 incident, Vector3 normal, float indexOfRefraction, out Vector3 refraction)
		{
			float cos = Vector3.Dot(-1 * incident, normal);
			float sin = MathF.Sqrt(1.0f - cos * cos);
			if (indexOfRefraction * sin > 1.0f)
			{
				refraction = Vector3.Zero;
				return false;
			}

			Vector3 rPerp = indexOfRefraction * (incident + cos * normal);
			Vector3 rParallel = -MathF.Sqrt(MathF.Abs(1.0f - rPerp.LengthSquared())) * normal;
			refraction = (rPerp + rParallel).Normalized();
			return true;
		}

		private double FresnelSchlick(float NdotV, float indexOfRefraction)
		{
			float r0 = MathF.Pow((1.0f - indexOfRefraction) / (1.0f + indexOfRefraction), 2.0f);
			return r0 + (1.0f - r0) * MathF.Pow(1 - NdotV, 5.0f);
		}

		private Vector3 ColorAsBytes(Vector3 color, bool gammaCorrect = true)
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

		private void SetByteColor(byte[][] results, int x, int y, int channelsPerPixel, Vector3 colorInBytes)
		{
			// Note: bitmap data is BGR, not RGB!
			int pixelStart = x * channelsPerPixel;
			results[y][pixelStart + 0] = (byte)colorInBytes.Z;
			results[y][pixelStart + 1] = (byte)colorInBytes.Y;
			results[y][pixelStart + 2] = (byte)colorInBytes.X;
		}

	}
}
