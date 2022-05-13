using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Numerics;


namespace CSharpPathTracer
{
	/// <summary>
	/// Parameters to begin a new raytrace
	/// </summary>
	class RaytracingParameters
	{
		public Scene Scene { get; set; }
		public Camera Camera { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
		public int SamplesPerPixel { get; set; }
		public int ResolutionReduction { get; set; }
		public int MaxRecursionDepth { get; set; }
		public bool Progressive { get; set; }

		public RaytracingParameters(Scene scene, Camera camera, int width, int height, int samplesPerPixel, int resolutionReduction, int maxRecursionDepth, bool progressive)
		{
			Scene = scene;
			Width = width;
			Height = height;
			Camera = camera;
			SamplesPerPixel = samplesPerPixel;
			ResolutionReduction = resolutionReduction;
			MaxRecursionDepth = maxRecursionDepth;
			Progressive = progressive;
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
		protected RaytracingStats stats;

		/// <summary>
		/// Gamma correction value so we don't have to re-divide
		/// </summary>
		public const float GammaCorrectionPower = 1.0f / 2.2f;

		/// <summary>
		/// Creates a new raytracer
		/// </summary>
		public Raytracer() { }

		/// <summary>
		/// Recursively traces a single ray in the given scene
		/// </summary>
		/// <param name="ray">The ray to trace</param>
		/// <param name="scene">The scene to use</param>
		/// <param name="depth">Current recursion depth</param>
		/// <returns>Resulting color of the trace</returns>
		protected Vector3 TraceRay(Ray ray, Scene scene, int depth)
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
				return 
					hitEntity.Material.GetEmissionAtUV(hit.UV) + 
					hitEntity.Material.GetColorAtUV(hit.UV) * TraceRay(newRay, scene, depth - 1);
			}
			else
			{
				// No hit so use environment
				return scene.Environment.GetColorFromDirection(ray.Direction);
			}
		}

	}
}
