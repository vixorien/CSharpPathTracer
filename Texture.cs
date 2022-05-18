using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using System.Numerics;

namespace CSharpPathTracer
{
	/// <summary>
	/// Texture filter mode
	/// </summary>
	public enum TextureFilter
	{
		Point,
		Linear
	}

	/// <summary>
	/// Address mode for UV's outside 0-1
	/// </summary>
	public enum TextureAddressMode
	{
		Clamp,
		Wrap
	}

	/// <summary>
	/// Represents a 2D grid of pixels that can be sampled
	/// </summary>
	class Texture
	{
		private Vector4[,] pixels;

		/// <summary>
		/// Gets the width of the texture
		/// </summary>
		public int Width { get; private set; }

		/// <summary>
		/// Gets the height of the texture
		/// </summary>
		public int Height { get; private set; }

		/// <summary>
		/// Gets the color at the given pixel coords
		/// </summary>
		/// <param name="x">The X coord</param>
		/// <param name="y">The Y coord</param>
		/// <returns>A single pixel color</returns>
		public Vector4 this[int x, int y]
		{
			get { return pixels[y, x]; }
		}

		/// <summary>
		/// Loads a new texture from a file
		/// </summary>
		/// <param name="filepath">The file to load</param>
		/// <param name="gammaUncorrect">Should the colors be converted back to linear color space?</param>
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

			// Speeds up the processing a bit!
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

		/// <summary>
		/// Creates a new texture given a 2D array of pixel colors
		/// </summary>
		/// <param name="colors">
		/// 2D array of RGBA pixel colors where 
		/// each component is between 0.0 and 1.0
		/// </param>
		public Texture(Vector4[,] colors)
		{
			// Clone the colors array so we have our own data
			pixels = (Vector4[,])colors.Clone();
			Height = pixels.GetLength(0);
			Width = pixels.GetLength(1);
		}

		/// <summary>
		/// Samples the given texture at the specified coordinates
		/// </summary>
		/// <param name="uv">The UV coords for sampling</param>
		/// <param name="addressMode">The address mode for UV's outside 0-1</param>
		/// <param name="filter">The filter mode</param>
		/// <returns>The 4-component color from the texture</returns>
		public Vector4 Sample(Vector2 uv, TextureAddressMode addressMode = TextureAddressMode.Wrap, TextureFilter filter = TextureFilter.Point)
		{
			// Adjust uv's as necessary
			switch (addressMode)
			{
				case TextureAddressMode.Wrap:
					// Truncate the UV
					uv -= new Vector2(MathF.Truncate(uv.X), MathF.Truncate(uv.Y));

					// Adjust by 1 if we're negative
					if (uv.X < 0.0f) uv.X += 1.0f;
					if (uv.Y < 0.0f) uv.Y += 1.0f;
					break;

				default:
				case TextureAddressMode.Clamp:
					uv = Vector2.Clamp(uv, Vector2.Zero, Vector2.One);
					break;

			}

			// Handle filtering
			switch (filter)
			{
				// Linear filtering
				// Two horizontal and one vertical interpolation
				//
				//  o <-- 1 --> o
				//        ^
				//        |
				//        3
				//        |
				//        v
				//  o <-- 2 --> o
				case TextureFilter.Linear:

					// Calculate the two integer values
					float scaledU = uv.X * (Width - 1);
					float scaledV = uv.Y * (Height - 1);
					int xLow = (int)MathF.Floor(scaledU);
					int yLow = (int)MathF.Floor(scaledV);
					int xHigh = xLow == Width - 1 ? 0 : xLow + 1;
					int yHigh = yLow == Height - 1 ? 0 : yLow + 1;

					// Get the 4 pixels
					Vector4 tl = this[xLow, yLow];
					Vector4 bl = this[xLow, yHigh];
					Vector4 tr = this[xHigh, yLow];
					Vector4 br = this[xHigh, yHigh];

					// Interpolate left/right
					float interpU = scaledU - xLow;
					Vector4 top = Vector4.Lerp(tl, tr, interpU);
					Vector4 bot = Vector4.Lerp(bl, br, interpU);

					// Final interpolation
					float interpV = scaledV - yLow;
					return Vector4.Lerp(top, bot, interpV);

				default:
				case TextureFilter.Point:
					// Convert uv to pixel location
					int x = (int)(uv.X * (Width - 1));
					int y = (int)(uv.Y * (Height - 1));
					return this[x, y];
			}
		}

		/// <summary>
		/// Creates a checkboard texture using black and white as its two colors
		/// </summary>
		/// <param name="width">The overall pixel width of the texture</param>
		/// <param name="height">The overall pixel height of the texture</param>
		/// <param name="numCheckersX">Number of checkers (squares of opposite colors) along X</param>
		/// <param name="numCheckersY">Number of checkers (squares of opposite colors) along Y</param>
		/// <returns>A new checkerboard texture</returns>
		public static Texture CreateCheckerboard(int width, int height, int numCheckersX, int numCheckersY)
		{
			return CreateCheckerboard(width, height, numCheckersX, numCheckersY, new Vector4(0, 0, 0, 1), new Vector4(1, 1, 1, 1));
		}

		/// <summary>
		/// Creates a checkboard texture
		/// </summary>
		/// <param name="width">The overall pixel width of the texture</param>
		/// <param name="height">The overall pixel height of the texture</param>
		/// <param name="numCheckersX">Number of checkers (squares of opposite colors) along X</param>
		/// <param name="numCheckersY">Number of checkers (squares of opposite colors) along Y</param>
		/// <param name="color0">The first color (used in the top-left checker)</param>
		/// <param name="color1">The second color</param>
		/// <returns>A new checkerboard texture</returns>
		public static Texture CreateCheckerboard(int width, int height, int numCheckersX, int numCheckersY, Vector4 color0, Vector4 color1)
		{
			// Verify parameters
			if (width <= 0 || height <= 0 || numCheckersX <= 0 || numCheckersY <= 0)
				throw new ArgumentException("Cannot create checkerboard texture.  One or more parameters are less than or equal to zero.");

			// Create the array of pixel colors
			Vector4[,] checker = new Vector4[height, width];

			// How many pixels of each color?
			int checkerWidth = width / numCheckersX;
			int checkerHeight = height / numCheckersY;

			// Loop through each pixel
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					// Assume its color 0
					Vector4 c = color0;

					// Flip-flop between color 0 and 1 over the width
					if ((x / checkerWidth) % 2 == 1) 
						c = color1;

					// Potentially flip-flop again based on y value
					if ((y / checkerHeight) % 2 == 1)
						c = c == color0 ? color1 : color0;

					checker[y, x] = c;
				}
			}

			return new Texture(checker);
		}
	}
}
