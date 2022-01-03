using System;
using System.IO;
using System.Numerics;

namespace CSharpPathTracer
{
	static class ImageWriter
	{
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
