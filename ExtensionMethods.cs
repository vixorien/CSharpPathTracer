using System;
using System.Numerics;
using System.Windows.Forms;

namespace CSharpPathTracer
{
	/// <summary>
	/// Extension methods for the System.Numerics classes
	/// </summary>
	public static class SIMDExtensionMethods
	{
		/// <summary>
		/// Gives the results of a matrix inversion
		/// </summary>
		/// <param name="matrix">The matrix itself</param>
		/// <returns>The inverted matrix</returns>
		public static Matrix4x4 Invert(this Matrix4x4 matrix)
		{
			Matrix4x4 result = matrix;
			Matrix4x4.Invert(matrix, out result);
			return result;
		}

		/// <summary>
		/// Gets the right vector from the given matrix
		/// </summary>
		/// <param name="m">The matrix itself</param>
		/// <returns>The right vector from the matrix</returns>
		public static Vector3 Right(this Matrix4x4 m)
		{
			return new Vector3(m.M11, m.M12, m.M13);
		}

		/// <summary>
		/// Gets the up vector from the given matrix
		/// </summary>
		/// <param name="m">The matrix itself</param>
		/// <returns>The up vector from the matrix</returns>
		public static Vector3 Up(this Matrix4x4 m)
		{
			return new Vector3(m.M21, m.M22, m.M23);
		}

		/// <summary>
		/// Gets the forward vector from the given matrix
		/// </summary>
		/// <param name="m">The matrix itself</param>
		/// <returns>The forward vector from the matrix</returns>
		public static Vector3 Forward(this Matrix4x4 m)
		{
			return new Vector3(-m.M31, -m.M32, -m.M33);
		}

		/// <summary>
		/// Converts the vector4 to a vector3 by dropping the W component
		/// </summary>
		/// <param name="v">The vector4 itself</param>
		/// <returns>A vector3 using the vector4's XYZ components</returns>
		public static Vector3 ToVector3(this Vector4 v)
		{
			return new Vector3(v.X, v.Y, v.Z);
		}

		/// <summary>
		/// Converts a color to a Vector3 with normalized (0-1) components
		/// </summary>
		/// <param name="color">The color itself</param>
		/// <returns>A vector3 with components in the 0-1 range</returns>
		public static Vector3 ToVector3(this System.Drawing.Color color)
		{
			return new Vector3((float)color.R / 255, (float)color.G / 255, (float)color.B / 255);
		}
	}

	/// <summary>
	/// Extension methods for the random class
	/// </summary>
	public static class RandomExtensionMethods
	{
		/// <summary>
		/// Generates a random float value between 0-1
		/// </summary>
		/// <param name="rng">The random object itself</param>
		/// <returns>Random float between 0-1</returns>
		public static float NextFloat(this Random rng)
		{
			return (float)rng.NextDouble();
		}

		/// <summary>
		/// Generates a random float value between 0 - max (exclusive)
		/// </summary>
		/// <param name="rng">The random object itself</param>
		/// <param name="max">The maximum value of the random number (exclusive)</param>
		/// <returns>Random float between 0-max</returns>
		public static float NextFloat(this Random rng, float max)
		{
			return (float)rng.NextDouble() * max;
		}

		/// <summary>
		/// Generates a random float value between min (inclusive) - max (exclusive)
		/// </summary>
		/// <param name="rng">The random object itself</param>
		/// <param name="min">The minimum value of the random number (inclusive)</param>
		/// <param name="max">The maximum value of the random number (exclusive)</param>
		/// <returns>Random float between min (inclusive) - max (exclusive)</returns>
		public static float NextFloat(this Random rng, float min, float max)
		{
			return min + (float)rng.NextDouble() * (max - min);
		}

		/// <summary>
		/// Generates a normalized 3D vector with random components
		/// </summary>
		/// <param name="rng">The random object itself</param>
		/// <returns>A normalized vector with randomized components</returns>
		public static Vector3 NextVector3(this Random rng)
		{
			Vector3 randomVec = new Vector3(
				(float)rng.NextDouble() * 2 - 1,
				(float)rng.NextDouble() * 2 - 1,
				(float)rng.NextDouble() * 2 - 1);
			return Vector3.Normalize(randomVec);
		}

		/// <summary>
		/// Generates a vector with random 0-1 values for its components
		/// </summary>
		/// <param name="rng">The random object itself</param>
		/// <returns>A vector with randomized components</returns>
		public static Vector3 NextColor(this Random rng)
		{
			Vector3 randomColor = new Vector3(
				(float)rng.NextDouble(),
				(float)rng.NextDouble(),
				(float)rng.NextDouble());
			return randomColor;
		}

		/// <summary>
		/// Generates random boolean (true or false)
		/// </summary>
		/// <param name="rng">The random object itself</param>
		/// <returns>True or false</returns>
		public static bool NextBool(this Random rng)
		{
			return rng.Next(2) == 0;
		}

		/// <summary>
		/// Generates a normalized 3D vector with random components in the hemisphere
		/// centered on the given normal
		/// </summary>
		/// <param name="rng">The random object itself</param>
		/// <returns>A normalized vector with randomized components in a hemisphere</returns>
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

		/// <summary>
		/// Generates a 3D vector with random x & y components within a 
		/// circle of the given radius, and a z component of zero
		/// </summary>
		/// <param name="rng">The random object itself</param>
		/// <param name="radius">The radius of the circle</param>
		/// <returns>A vector with length less than the given radius</returns>
		public static Vector3 NextVectorInCircle(this Random rng, float radius)
		{
			// Start with random values in x and y
			Vector3 randomVec = new Vector3(
				rng.NextFloat(-1.0f, 1.0f),
				rng.NextFloat(-1.0f, 1.0f),
				0.0f);

			// Normalize and adjust radius randomly
			return Vector3.Normalize(randomVec) * rng.NextFloat(radius);
		}

	}

	/// <summary>
	/// Extension methods for the built-in C# progress bar
	/// </summary>
	public static class ProgressBarExtensionMethods
	{

		/// <summary>
		/// Increments the progress bar without animating it
		/// </summary>
		/// <param name="bar">The bar itself</param>
		/// <param name="amount">The new amount</param>
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

		/// <summary>
		/// Sets the progress bar's value as a percentage of completion
		/// </summary>
		/// <param name="bar">This bar</param>
		/// <param name="percent">The percentage from 0 - 1</param>
		public static void SetPercentage(this ProgressBar bar, double percent)
		{
			if (bar == null)
				return;

			// Calculate the new maximum value and verify bounds
			int newValue = bar.Minimum + (int)((bar.Maximum - bar.Minimum) * percent);
			newValue = Math.Clamp(newValue, bar.Minimum, bar.Maximum);

			// Set the value now that we know it's value, then decrease
			// and increase to stop the damn animation
			bar.Value = newValue;
			bar.Increment(-1);
			bar.Increment(1);
		}

		/// <summary>
		/// Attempts to stop the scrolling marquee animation
		/// </summary>
		/// <param name="bar">The bar itself</param>
		public static void StopMarquee(this ProgressBar bar)
		{
			// Verify the bar still exists
			if (bar == null)
				return;

			// Slightly ridiculous way to stop the scrolling marquee when
			// the progress bar is not moving (doesn't always work, ARGH)
			int oldMin = bar.Minimum;
			bar.Minimum = bar.Value;
			bar.Value = bar.Minimum;
			bar.Minimum = oldMin;
		}
	}
}
