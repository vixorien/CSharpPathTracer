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
		private RaytracingStats stats;
		private byte[][] pixels;
		private Vector3[][] progressiveColors;

		/// <summary>
		/// Numbers of channels per pixel for our results
		/// </summary>
		public const int ChannelsPerPixel = 3;

		/// <summary>
		/// Gamma correction value so we don't have to re-divide
		/// </summary>
		private const float GammaCorrectionPower = 1.0f / 2.2f;

		/// <summary>
		/// Creates a new raytracer
		/// </summary>
		public Raytracer() { }

		/// <summary>
		/// Raytraces a scene - this is assumed to be launched by a BackgroundWorker
		/// </summary>
		/// <param name="sender">The object that launched the raytrace</param>
		/// <param name="e">Work parameters</param>
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
				progressiveColors = new Vector3[height][];
				for (int h = 0; h < height; h++)
				{
					pixels[h] = new byte[width * ChannelsPerPixel];
					progressiveColors[h] = new Vector3[width];
				}
			}

			// Reset progressive colors
			for (int h = 0; h < height; h++)
				for (int w = 0; w < width; w++)
					progressiveColors[h][w] = new Vector3();

			// Figure out the resolution, and adjust by half so we
			// calculate the "center" of the large pixel
			int res = rtParams.ResolutionReduction;
			int half = res / 2;

			// Values we'll be using for rays
			int progressiveSteps = rtParams.Progressive ? rtParams.SamplesPerPixel : 1;
			int raysPerPixel = rtParams.Progressive ? 1 : rtParams.SamplesPerPixel;

			// Progressive loop (as necessary)
			for (int p = 0; p < progressiveSteps; p++)
			{
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
						for (int s = 0; s < raysPerPixel; s++)
						{
							float adjustedX = x + ThreadSafeRandom.Instance.NextFloat(-0.5f, 0.5f);
							float adjustedY = y + ThreadSafeRandom.Instance.NextFloat(-0.5f, 0.5f);

							// Get this ray and add to the total raytraced color
							Ray ray = rtParams.Camera.GetRayThroughPixel(adjustedX, adjustedY, width, height);
								totalColor += TraceRay(ray, rtParams.Scene, rtParams.MaxRecursionDepth);
						}

						// Average the color and set the resolution block
						if(raysPerPixel > 1)
							totalColor /= raysPerPixel;

						// What's the best value to use for X?
						int bestX = Math.Min(xIteration, width);

						// Add to the progressive color, average and convert to bytes
						progressiveColors[y][bestX] += totalColor;
						Vector3 avgResult = progressiveColors[y][bestX] / (p + 1);
						Vector3 colorAsBytes = ColorAsBytes(ref avgResult, true);

						// Set the block of pixels necessary for the reduced pixel count
						for (int blockX = x - half; blockX < x + res - half && blockX < width; blockX++)
						{
							SetByteColor(pixels, blockX, y, ref colorAsBytes);
						}
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
					percentComplete += ((double)y / height) * onePassPercentage;
					
					RaytracingProgress progress = new RaytracingProgress(
						y - half, 
						res, 
						pixels[y],
						percentComplete, 
						stats);
					worker.ReportProgress((int)progress.CompletionPercent, progress);
				}
			}

		}

		/// <summary>
		/// Recursively traces a single ray in the given scene
		/// </summary>
		/// <param name="ray">The ray to trace</param>
		/// <param name="scene">The scene to use</param>
		/// <param name="depth">Current recursion depth</param>
		/// <returns>Resulting color of the trace</returns>
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

		/// <summary>
		/// Converts a color from the 0-1 range to the 0-255 range
		/// </summary>
		/// <param name="color">The color to convert</param>
		/// <param name="gammaCorrect">Should the color be gamma corrected?</param>
		/// <returns>The resulting 0-255 color</returns>
		private Vector3 ColorAsBytes(ref Vector3 color, bool gammaCorrect = true)
		{
			// Perform a clamp between 0 and 1 first
			Vector3 result = Vector3.Clamp(color, Vector3.Zero, Vector3.One);

			// Raise to a power if we're gamma correcting
			if (gammaCorrect)
			{
				result.X = MathF.Pow(result.X, GammaCorrectionPower);
				result.Y = MathF.Pow(result.Y, GammaCorrectionPower);
				result.Z = MathF.Pow(result.Z, GammaCorrectionPower);
			}

			// Adjust to the byte range
			return result * 255;
		}

		/// <summary>
		/// Sets a 0-255 color as bytes in the given array
		/// </summary>
		/// <param name="results">Where to store the bytes</param>
		/// <param name="x">The pixel's x coord</param>
		/// <param name="y">The pixel's y coord</param>
		/// <param name="colorInBytes">The 0-255 color</param>
		private void SetByteColor(byte[][] results, int x, int y, ref Vector3 colorInBytes)
		{
			// Note: bitmap data is BGR, not RGB!
			int pixelStart = x * ChannelsPerPixel;
			results[y][pixelStart + 0] = (byte)colorInBytes.Z; // B
			results[y][pixelStart + 1] = (byte)colorInBytes.Y; // G
			results[y][pixelStart + 2] = (byte)colorInBytes.X; // R
		}

	}
}
