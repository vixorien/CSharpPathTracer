using System;
using System.Numerics;
using System.Windows.Forms;

namespace CSharpPathTracer
{
	public static class SIMDExtensionMethods
	{
		public static Matrix4x4 Invert(this Matrix4x4 matrix)
		{
			Matrix4x4 result = matrix;
			Matrix4x4.Invert(matrix, out result);
			return result;
		}

		public static Vector3 Right(this Matrix4x4 m)
		{
			return new Vector3(m.M11, m.M12, m.M13);
		}

		public static Vector3 Up(this Matrix4x4 m)
		{
			return new Vector3(m.M21, m.M22, m.M23);
		}

		public static Vector3 Forward(this Matrix4x4 m)
		{
			return new Vector3(-m.M31, -m.M32, -m.M33);
		}

		public static Vector3 ToVector3(this Vector4 v)
		{
			return new Vector3(v.X, v.Y, v.Z);
		}

		public static System.Drawing.Color ToSystemColor(this Vector3 color)
		{
			return System.Drawing.Color.FromArgb(
				(int)(Math.Clamp(color.X, 0, 1) * 255),
				(int)(Math.Clamp(color.Y, 0, 1) * 255),
				(int)(Math.Clamp(color.Z, 0, 1) * 255));
		}

		public static Vector3 ToVector3(this System.Drawing.Color color)
		{
			return new Vector3((float)color.R / 255, (float)color.G / 255, (float)color.B / 255);
		}
	}

	
	public static class RandomExtensionMethods
	{

		public static float NextFloat(this Random rng)
		{
			return (float)rng.NextDouble();
		}

		public static float NextFloat(this Random rng, float max)
		{
			return (float)rng.NextDouble() * max;
		}

		public static float NextFloat(this Random rng, float min, float max)
		{
			return min + (float)rng.NextDouble() * (max - min);
		}

		public static Vector3 NextVector3(this Random rng)
		{
			Vector3 randomVec = new Vector3(
				(float)rng.NextDouble() * 2 - 1,
				(float)rng.NextDouble() * 2 - 1,
				(float)rng.NextDouble() * 2 - 1);
			return Vector3.Normalize(randomVec);
		}

		public static Vector3 NextColor(this Random rng)
		{
			Vector3 randomColor = new Vector3(
				(float)rng.NextDouble(),
				(float)rng.NextDouble(),
				(float)rng.NextDouble());
			return randomColor;
		}

		public static bool NextBool(this Random rng)
		{
			return rng.Next(2) == 0;
		}

		public static Vector3 NextVectorInHemisphere(this Random rng, Vector3 normal)
		{
			// Calculate a random vector
			Vector3 randomVec = rng.NextVector3();

			// Are we in the wrong hemisphere?
			float dot = Vector3.Dot(randomVec, normal);
			if (dot < 0.0f)
				randomVec *= -1;

			return randomVec;
		}
	}

	public static class ProgressBarExtensionMethods
	{

		public static void IncrementNoAnimation(this ProgressBar bar, int amount)
		{
			// Verify the bar still exists
			if (bar == null)
				return;

			// Are we at max?
			if (bar.Value == bar.Maximum)
				return;

			// Are we about to hit max?
			if (bar.Value >= bar.Maximum - amount)
			{
				// Go to max, decrement 1 then increment again
				bar.Value = bar.Maximum;
				bar.Value--; // Would Increment(-1) have the same effect?
				bar.Increment(1);
				return;
			}

			// Otherwise, go up an extra and back down
			bar.Increment(amount + 1);
			bar.Value--;
		}

		public static void StopMarquee(this ProgressBar bar)
		{
			// Verify the bar still exists
			if (bar == null)
				return;

			// Slightly ridiculous way to stop the scrolling marquee when
			// the progress bar is not moving
			int oldMin = bar.Minimum;
			bar.Minimum = bar.Value;
			bar.Value = bar.Minimum;
			bar.Minimum = oldMin;
		}
	}
}
