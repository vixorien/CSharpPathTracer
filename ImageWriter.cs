using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CSharpRaytracing
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
			int r = (int)(Math.Clamp(color.X, 0, 1) * 255);
			int g = (int)(Math.Clamp(color.Y, 0, 1) * 255);
			int b = (int)(Math.Clamp(color.Z, 0, 1) * 255);

			return $"{r} {g} {b}";
		}
	}
}
