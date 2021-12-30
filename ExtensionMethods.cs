using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Windows.Forms;

namespace CSharpPathTracer
{
	public static class BoundingBoxExtensionMethods
	{
		public static BoundingBox GetTransformed(this BoundingBox aabb, Transform transform)
		{
			Vector3[] corners = aabb.GetCorners();

			// Create bounding box around just the first corner
			Vector3 transformedCorner = Vector3.Transform(corners[0], transform.WorldMatrix);
			BoundingBox transformedAABB = new BoundingBox(transformedCorner, transformedCorner);

			// Loop and encompass all transformed corners
			for (int i = 1; i < corners.Length; i++)
			{
				transformedCorner = Vector3.Transform(corners[i], transform.WorldMatrix);
				transformedAABB.Encompass(transformedCorner);
			}

			return transformedAABB;
		}

		public static void Encompass(this ref BoundingBox aabb, Vector3 point)
		{
			// Test min
			if (point.X < aabb.Min.X) aabb.Min.X = point.X;
			if (point.Y < aabb.Min.Y) aabb.Min.Y = point.Y;
			if (point.Z < aabb.Min.Z) aabb.Min.Z = point.Z;

			// Test max
			if (point.X > aabb.Max.X) aabb.Max.X = point.X;
			if (point.Y > aabb.Max.Y) aabb.Max.Y = point.Y;
			if (point.Z > aabb.Max.Z) aabb.Max.Z = point.Z;
		}

		public static void Encompass(this ref BoundingBox aabb, BoundingBox other)
		{
			aabb.Encompass(other.Min);
			aabb.Encompass(other.Max);
		}

		public static BoundingBox LeftHalf(this BoundingBox aabb)
		{
			return new BoundingBox(aabb.Min, new Vector3(MathHelper.Lerp(aabb.Min.X, aabb.Max.X, 0.5f), aabb.Max.Y, aabb.Max.Z));
		}

		public static BoundingBox RightHalf(this BoundingBox aabb)
		{
			return new BoundingBox(new Vector3(MathHelper.Lerp(aabb.Min.X, aabb.Max.X, 0.5f), aabb.Min.Y, aabb.Min.Z), aabb.Max);
		}

		public static BoundingBox BottomHalf(this BoundingBox aabb)
		{
			return new BoundingBox(aabb.Min, new Vector3(aabb.Max.X, MathHelper.Lerp(aabb.Min.Y, aabb.Max.Y, 0.5f), aabb.Max.Z));
		}

		public static BoundingBox TopHalf(this BoundingBox aabb)
		{
			return new BoundingBox(new Vector3(aabb.Min.X, MathHelper.Lerp(aabb.Min.Y, aabb.Max.Y, 0.5f), aabb.Min.Z), aabb.Max);
		}

		public static BoundingBox FrontHalf(this BoundingBox aabb)
		{
			return new BoundingBox(aabb.Min, new Vector3(aabb.Max.X, aabb.Max.Y, MathHelper.Lerp(aabb.Min.Z, aabb.Max.Z, 0.5f)));
		}

		public static BoundingBox BackHalf(this BoundingBox aabb)
		{
			return new BoundingBox(new Vector3(aabb.Min.X, aabb.Min.Y, MathHelper.Lerp(aabb.Min.Z, aabb.Max.Z, 0.5f)), aabb.Max);
		}
	}

	public static class VectorExtensionMethods
	{
		public static Vector3 Abs(this Vector3 v)
		{
			return new Vector3(
				MathF.Abs(v.X),
				MathF.Abs(v.Y),
				MathF.Abs(v.Z));
		}

		public static Vector3 Normalized(this Vector3 v)
		{
			v.Normalize();
			return v;
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

		public static Vector3 ToVector3(this Vector4 vec4)
		{
			return new Vector3(vec4.X, vec4.Y, vec4.Z);
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
			randomVec.Normalize();
			return randomVec;
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
