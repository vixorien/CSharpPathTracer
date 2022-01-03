
using System.Numerics;

using MathHelper = Microsoft.Xna.Framework.MathHelper;

namespace CSharpPathTracer
{
	public enum AABBContainment
	{
		NoOverlap,
		Contains,
		Intersects
	}

	struct AABB
	{
		private Vector3 min;
		private Vector3 max;

		public Vector3 Min { get => min; set => min = value; }
		public Vector3 Max { get => max; set => max = value; }

		public float Width { get => max.X - min.X; }
		public float Height { get => max.Y - min.Y; }
		public float Depth { get => max.Z - min.Z; }

		public AABB(Vector3 min, Vector3 max)
		{
			this.min = min;
			this.max = max;
		}

		public void Encompass(Vector3 point)
		{
			// Test min
			if (point.X < Min.X) min.X = point.X;
			if (point.Y < Min.Y) min.Y = point.Y;
			if (point.Z < Min.Z) min.Z = point.Z;

			// Test max
			if (point.X > Max.X) max.X = point.X;
			if (point.Y > Max.Y) max.Y = point.Y;
			if (point.Z > Max.Z) max.Z = point.Z;
		}

		public void Encompass(AABB other)
		{
			Encompass(other.min);
			Encompass(other.max);
		}

		public AABB LeftHalf()
		{
			return new AABB(min, new Vector3(MathHelper.Lerp(min.X, max.X, 0.5f), max.Y, max.Z));
		}

		public AABB RightHalf()
		{
			return new AABB(new Vector3(MathHelper.Lerp(min.X, max.X, 0.5f), min.Y, min.Z), max);
		}

		public AABB BottomHalf()
		{
			return new AABB(min, new Vector3(max.X, MathHelper.Lerp(min.Y, max.Y, 0.5f), max.Z));
		}

		public AABB TopHalf()
		{
			return new AABB(new Vector3(min.X, MathHelper.Lerp(min.Y, max.Y, 0.5f), min.Z), max);
		}

		public AABB FrontHalf()
		{
			return new AABB(min, new Vector3(max.X, max.Y, MathHelper.Lerp(min.Z, max.Z, 0.5f)));
		}

		public AABB BackHalf()
		{
			return new AABB(new Vector3(min.X, min.Y, MathHelper.Lerp(min.Z, max.Z, 0.5f)), max);
		}

		public bool CouldFit(AABB other)
		{
			return
				this.Width >= other.Width &&
				this.Height >= other.Height &&
				this.Depth >= other.Depth;
		}

		public Vector3 GetCorner(int cornerIndex)
		{
			switch (cornerIndex)
			{
				case 0: return new Vector3(min.X, min.Y, min.Z);
				case 1: return new Vector3(min.X, min.Y, max.Z);
				case 2: return new Vector3(min.X, max.Y, min.Z);
				case 3: return new Vector3(min.X, max.Y, max.Z);
				case 4: return new Vector3(max.X, min.Y, min.Z);
				case 5: return new Vector3(max.X, min.Y, max.Z);
				case 6: return new Vector3(max.X, max.Y, min.Z);
				case 7: return new Vector3(max.X, max.Y, max.Z);
				default: throw new System.IndexOutOfRangeException($"Corner index {cornerIndex} is invalid - valid indices are 0-7");
			}
		}

		public AABB GetTransformed(Transform transform)
		{
			// Get the transformed min, which is GetCorner[0]
			Vector3 transformedCorner = Vector3.Transform(min, transform.WorldMatrix);

			// Create the new transformed AABB with the first point
			AABB transformedAABB = new AABB(transformedCorner, transformedCorner);

			// Loop and handle all other corners
			for (int i = 1; i < 8; i++)
			{
				transformedCorner = Vector3.Transform(GetCorner(i), transform.WorldMatrix);
				transformedAABB.Encompass(transformedCorner);
			}

			return transformedAABB;
		}

		public AABBContainment Contains(AABB other)
		{
			// First check if there is space between them
			if (other.max.X < min.X ||
				other.max.Y < min.Y ||
				other.max.Z < min.Z ||
				other.min.X > max.X ||
				other.min.Y > max.Y ||
				other.min.Z > max.Z)
			{
				// There is space, so they cannot overlap
				return AABBContainment.NoOverlap;
			}

			// Next check to see if we contain the entirety
			// of the other AABB
			if (other.min.X >= min.X &&
				other.min.Y >= min.Y &&
				other.min.Z >= min.Z &&
				other.max.X <= max.X &&
				other.max.Y <= max.Y &&
				other.max.Z <= max.Z)
			{
				// The other AABB is completely within this one
				return AABBContainment.Contains;
			}

			// The only other option is a partial intersection
			return AABBContainment.Intersects;
		}

		public static AABB Combine(AABB b1, AABB b2)
		{
			b1.Encompass(b2);
			return b1;
		}


		public bool Intersects(Ray ray, out float distance)
		{
			// Reference: https://tavianator.com/2015/ray_box_nan.html
			// Note: This method is SLOOOOW in C# due to Min()/Max()
			//       having pretty horrible performance here (apparently?)
			{
				//// Handle X
				//float t1 = (min.X - ray.Origin.X) * ray.InvDirection.X;
				//float t2 = (max.X - ray.Origin.X) * ray.InvDirection.X;
				//tMin = MathF.Min(t1, t2);
				//float tMax = MathF.Max(t1, t2);

				//// Handle Y
				//t1 = (min.Y - ray.Origin.Y) * ray.InvDirection.Y;
				//t2 = (max.Y - ray.Origin.Y) * ray.InvDirection.Y;
				//tMin = MathF.Max(tMin, MathF.Min(MathF.Min(t1, t2), tMax));
				//tMax = MathF.Min(tMax, MathF.Max(MathF.Max(t1, t2), tMin));

				//// Handle Z
				//t1 = (min.Z - ray.Origin.Z) * ray.InvDirection.Z;
				//t2 = (max.Z - ray.Origin.Z) * ray.InvDirection.Z;
				//tMin = MathF.Max(tMin, MathF.Min(MathF.Min(t1, t2), tMax));
				//tMax = MathF.Min(tMax, MathF.Max(MathF.Max(t1, t2), tMin));

				//// Determine if we actually hit
				//// Note: Using >= here to handle infinitely thin boxes
				////       See comment by Aleksei at above link!
				//return tMax >= MathF.Max(tMin, 0.0f);
			}


			// Directly from MonoGame, with a few tweaks to remove nullables
			// Decent, but maybe faster if we vectorize and skip some branches?
			{
				//	const float Epsilon = 1e-6f;

				//	distance = -1;
				//	float tMin = 0;
				//	float tMax = 0;

				//	if (Math.Abs(ray.Direction.X) < Epsilon)
				//	{
				//		if (ray.Origin.X < min.X || ray.Origin.X > max.X)
				//			return false;
				//	}
				//	else
				//	{
				//		tMin = (min.X - ray.Origin.X) / ray.Direction.X;
				//		tMax = (max.X - ray.Origin.X) / ray.Direction.X;

				//		if (tMin > tMax)
				//		{
				//			var temp = tMin;
				//			tMin = tMax;
				//			tMax = temp;
				//		}
				//	}

				//	if (Math.Abs(ray.Direction.Y) < Epsilon)
				//	{
				//		if (ray.Origin.Y < min.Y || ray.Origin.Y > max.Y)
				//			return false;
				//	}
				//	else
				//	{
				//		var tMinY = (min.Y - ray.Origin.Y) / ray.Direction.Y;
				//		var tMaxY = (max.Y - ray.Origin.Y) / ray.Direction.Y;

				//		if (tMinY > tMaxY)
				//		{
				//			var temp = tMinY;
				//			tMinY = tMaxY;
				//			tMaxY = temp;
				//		}

				//		if (tMin > tMaxY || tMinY > tMax)
				//			return false;

				//		if (tMinY > tMin) tMin = tMinY;
				//		if (tMaxY < tMax) tMax = tMaxY;
				//	}

				//	if (Math.Abs(ray.Direction.Z) < Epsilon)
				//	{
				//		if (ray.Origin.Z < min.Z || ray.Origin.Z > max.Z)
				//			return false;
				//	}
				//	else
				//	{
				//		var tMinZ = (min.Z - ray.Origin.Z) / ray.Direction.Z;
				//		var tMaxZ = (max.Z - ray.Origin.Z) / ray.Direction.Z;

				//		if (tMinZ > tMaxZ)
				//		{
				//			var temp = tMinZ;
				//			tMinZ = tMaxZ;
				//			tMaxZ = temp;
				//		}

				//		if (tMin > tMaxZ || tMinZ > tMax)
				//			return false;

				//		if (tMinZ > tMin) tMin = tMinZ;
				//		if (tMaxZ < tMax) tMax = tMaxZ;
				//	}

				//	// having a positive tMax and a negative tMin means the ray is inside the box
				//	// we expect the intesection distance to be 0 in that case
				//	if (tMin < 0 && tMax > 0)
				//	{
				//		distance = 0;
				//		return true;
				//	}

				//	// a negative tMin means that the intersection point is behind the ray's origin
				//	// we discard these as not hitting the AABB
				//	if (tMin < 0) return false;

				//	distance = tMin;
				//	return true;
			}


			// Reference: https://www.scratchapixel.com/lessons/3d-basic-rendering/minimal-ray-tracer-rendering-simple-shapes/ray-box-intersection
			// Let's try to vectorize this one - we have a winner!
			{
				distance = 0;

				// Vectorize the main math
				Vector3 tMinVec = (min - ray.Origin) * ray.InvDirection;
				Vector3 tMaxVec = (max - ray.Origin) * ray.InvDirection;

				// Check the values and swap if necessary
				if (tMinVec.X > tMaxVec.X) { float t = tMinVec.X; tMinVec.X = tMaxVec.X; tMaxVec.X = t; }
				if (tMinVec.Y > tMaxVec.Y) { float t = tMinVec.Y; tMinVec.Y = tMaxVec.Y; tMaxVec.Y = t; }
				if (tMinVec.Z > tMaxVec.Z) { float t = tMinVec.Z; tMinVec.Z = tMaxVec.Z; tMaxVec.Z = t; }

				// Start with X results
				float tMin = tMinVec.X;
				float tMax = tMaxVec.X;

				// Check X against Y
				if (tMin > tMaxVec.Y || tMinVec.Y > tMax) return false;
				if (tMinVec.Y > tMin) tMin = tMinVec.Y;
				if (tMaxVec.Y < tMax) tMax = tMaxVec.Y;

				// Check Z against previous results
				if (tMin > tMaxVec.Z || tMinVec.Z > tMax) return false;
				if (tMinVec.Z > tMin) tMin = tMinVec.Z;
				//if (tMaxVec.Z < tMax) tMax = tMaxVec.Z; // Not currently needed

				distance = tMin;
				return true;
			}
		}


		public bool Intersects(Ray ray)
		{
			float ignore;
			return Intersects(ray, out ignore);
		}


	}
}
