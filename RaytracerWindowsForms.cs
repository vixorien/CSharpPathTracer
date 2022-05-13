using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Numerics;

namespace CSharpPathTracer
{
	class RaytracerWindowsForms : Raytracer
	{
		// Results
		private byte[][] pixels;
		private Vector3[][] progressiveColors;

		/// <summary>
		/// Numbers of channels per pixel for our results
		/// </summary>
		public const int ChannelsPerPixel = 3;

		/// <summary>
		/// Creates a new raytracer for windows forms
		/// </summary>
		public RaytracerWindowsForms() { }

		/// <summary>
		/// Raytraces a scene launched by a BackgroundWorker
		/// </summary>
		/// <param name="sender">The object that launched the raytrace</param>
		/// <param name="e">Work parameters</param>
		public void RaytraceSceneBackgroundWorker(object sender, DoWorkEventArgs e)
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
						if (raysPerPixel > 1)
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
