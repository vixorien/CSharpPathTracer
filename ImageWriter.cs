using System;
using System.IO;
using System.Numerics;

namespace CSharpPathTracer
{
	/// <summary>
	/// Writes out a very basic image file
	/// </summary>
	static class ImageWriter
	{
		/// <summary>
		/// Writes the PPM image file given a 2D array of pixel colors
		/// </summary>
		/// <param name="file">The file to write</param>
		/// <param name="pixels">The 2D array of colors</param>
		public static void WritePPMImageFile(string file, Vector3[,] pixels)
		{
			using (StreamWriter output = new StreamWriter(file))
			{
				// Write header:
				// - File type (P3)
				// - width
				// - height
				// - maximum value per channel
				output.WriteLine($"P3 {pixels.GetLength(0)} {pixels.GetLength(1)} 255");

				// Write one pixel at a time as integer channel values
				for(int y = 0; y < pixels.GetLength(1); y++)
				{
					for (int x = 0; x < pixels.GetLength(0); x++)
					{
						output.WriteLine(GetColorString(pixels[x,y]));
					}
				}
			}
		}

		/// <summary>
		/// Creates a string from a color
		/// </summary>
		/// <param name="color">The color to convert</param>
		/// <returns>The string representation of the color</returns>
		private static string GetColorString(Vector3 color)
		{
			// Perform the 0-1 clamp and convert to 0-255
			color = Vector3.Clamp(color, Vector3.Zero, Vector3.One) * 255;

			int r = (int)(color.X);
			int g = (int)(color.Y);
			int b = (int)(color.Z);

			return $"{r} {g} {b}";
		}
	}
}
