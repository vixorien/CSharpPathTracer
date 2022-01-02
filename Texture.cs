using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using System.Numerics;

namespace CSharpPathTracer
{
	class Texture
	{
		public enum Filter
		{
			Point,
			Linear
		}

		public enum AddressMode
		{
			Clamp,
			Wrap
		}

		private Vector4[,] pixels;

		public int Width { get; private set; }
		public int Height { get; private set; }

		public Vector4 this[int x, int y]
		{
			get { return pixels[y, x]; }
		}

		public Texture(string filepath, bool gammaUncorrect = true)
		{
			byte[] pixelBytes;
			int bytesPerPixel = -1;
			int byteStride = -1;
			PixelFormat format = PixelFormat.Undefined;

			using (Bitmap bitmap = new Bitmap(filepath))
			{
				// Save the size
				Width = bitmap.Width;
				Height = bitmap.Height;
				format = bitmap.PixelFormat;

				// Lock the bitmap to copy data out
				BitmapData data = bitmap.LockBits(
					new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
					System.Drawing.Imaging.ImageLockMode.ReadOnly,
					bitmap.PixelFormat);

				// Now that we have access to the data, prepare an
				// array of bytes to hold the info
				byteStride = data.Stride;
				bytesPerPixel = data.Stride / data.Width;
				pixelBytes = new byte[data.Stride * data.Height];

				// Copy the pixels to our local array
				Marshal.Copy(data.Scan0, pixelBytes, 0, pixelBytes.Length);

				// Unlock now that we're done
				bitmap.UnlockBits(data);
			}

			// At this point the bitmap has been disposed, so we can convert
			// the byte array to our 2D array of pixels
			pixels = new Vector4[Height, Width]; // Row, Col

			Parallel.For(0, Height, h =>
			{	
				for (int w = 0; w < Width; w++)
				{
					// Grab a pixel worth of data and convert
					int byteIndex = h * byteStride + w * bytesPerPixel;

					// Set color components (and convert to float) - Note that the order is BGRA
					pixels[h, w].Z = pixelBytes[byteIndex] / 255.0f;
					if (bytesPerPixel >= 2) pixels[h, w].Y = pixelBytes[byteIndex + 1] / 255.0f;
					if (bytesPerPixel >= 3) pixels[h, w].X = pixelBytes[byteIndex + 2] / 255.0f;
					if (bytesPerPixel >= 4) pixels[h, w].W = pixelBytes[byteIndex + 3] / 255.0f;

					if (gammaUncorrect)
					{
						pixels[h, w].X = MathF.Pow(pixels[h, w].X, 2.2f);
						pixels[h, w].Y = MathF.Pow(pixels[h, w].Y, 2.2f);
						pixels[h, w].Z = MathF.Pow(pixels[h, w].Z, 2.2f);
						pixels[h, w].W = MathF.Pow(pixels[h, w].W, 2.2f);
					}
				}
			});
		}

		public Vector4 Sample(Vector2 uv, AddressMode addressMode = AddressMode.Wrap)//, Filter filter = Filter.Point)
		{
			// Adjust uv's as necessary
			switch (addressMode)
			{
				case AddressMode.Clamp:
					uv.X = Math.Clamp(uv.X, 0.0f, 1.0f);
					uv.Y = Math.Clamp(uv.Y, 0.0f, 1.0f);
					break;

				case AddressMode.Wrap:
					uv.X = uv.X - MathF.Truncate(uv.X);
					if (uv.X < 0.0f) uv.X += 1.0f;

					uv.Y = uv.Y - MathF.Truncate(uv.Y);
					if (uv.Y < 0.0f) uv.Y += 1.0f;
					break;
			}

			// Convert uv to pixel location
			// TODO: Handle filtering
			int x = (int)(uv.X * (Width - 1));
			int y = (int)(uv.Y * (Height - 1));

			// Return the requested pixel
			return this[x, y];
		}

	}
}
