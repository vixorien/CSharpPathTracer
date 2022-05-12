using System;
using System.Numerics;

namespace CSharpPathTracer
{
	/// <summary>
	/// A perfect sphere represented by a center point and a radius
	/// </summary>
	class Sphere : Geometry
	{
		/// <summary>
		/// Creates a new, default sphere with a radius of 1 at (0,0,0)
		/// </summary>
		public static Sphere Default { get { return new Sphere(); } }

		/// <summary>
		/// Gets or sets the center of the sphere
		/// </summary>
		public Vector3 Center { get; set; }

		/// <summary>
		/// Gets or sets the sphere's radius
		/// </summary>
		public float Radius { get; set; }

		/// <summary>
		/// Creates a new default sphere centered at the origin with a radius of 1
		/// </summary>
		public Sphere() 
			: this(Vector3.Zero, 1.0f)
		{
		}

		/// <summary>
		/// Creates a new sphere
		/// </summary>
		/// <param name="center">Sphere's center</param>
		/// <param name="radius">Sphere's radius</param>
		public Sphere(Vector3 center, float radius) : base()
		{
			// Save sphere details
			Center = center;
			Radius = radius;

			// Set up AABB
			aabb.Min = center - new Vector3(radius);
			aabb.Max = center + new Vector3(radius);
		}

		/// <summary>
		/// Performs a ray intersection on this sphere
		/// </summary>
		/// <param name="ray">The ray for the intersection test</param>
		/// <param name="hits">The hit info</param>
		/// <returns>True if an intersection occurs, false otherwise</returns>
		public override bool RayIntersection(Ray ray, out RayHit hit)
		{
			// How far along ray to closest point to sphere center
			Vector3 originToCenter = Center - ray.Origin;
			float tCenter = Vector3.Dot(originToCenter, ray.Direction);

			// If tCenter is negative, we point away from sphere
			if (tCenter < 0)
			{
				// No intersection points
				hit = RayHit.None;
				return false;
			}

			// Distance from closest point to sphere's center
			float d = MathF.Sqrt(originToCenter.LengthSquared() - tCenter * tCenter);

			// If distance is greater than radius, we don't hit the sphere
			if (d > Radius)
			{
				// No intersection points
				hit = RayHit.None;
				return false;
			}

			// Offset from tCenter to an intersection point
			float offset = MathF.Sqrt(Radius * Radius - d * d);

			// Distance to the hit point
			float hitDistance = tCenter - offset; // And tCenter + offset

			// Valid hit?
			HitSide side = HitSide.Outside;
			if (hitDistance < ray.TMin || hitDistance > ray.TMax)
			{
				// If t1 is negative (or just behind TMin), we need
				// to try the second intersection point instead and
				// re-check for the same issue
				hitDistance = tCenter + offset;
				if (hitDistance < ray.TMin || hitDistance > ray.TMax)
				{
					// Outside the valid ray range
					hit = RayHit.None;
					return false;
				}

				// Second point is valid - use that!
				// Denote that we're actually inside
				side = HitSide.Inside;
			}

			// Points of intersection
			Vector3 p1 = ray.Origin + ray.Direction * hitDistance;
			
			// Normals
			Vector3 normal = Vector3.Normalize(p1 - Center);
			if (side == HitSide.Inside)
			{
				normal *= -1.0f;
			}

			// Calculate a UV based on normal
			Vector2 xz = Vector2.Normalize(new Vector2(normal.X, normal.Z));
			Vector2 uv = new Vector2(
				-MathF.Atan2(xz.Y, xz.X) / MathF.PI + 1,
				MathF.Acos(Vector3.Dot(Vector3.UnitY, normal)) / MathF.PI);

			// Calculate tangent from x/z vector
			// - To rotate by 90 degrees: [x,y] -> [-y, x]
			// - TODO: Verify this is the correct rotation direction
			Vector3 tangent = new Vector3(-xz.Y, xz.X, 0.0f);
			if (side == HitSide.Inside)
			{
				tangent *= -1.0f;
			}

			// Set up return values
			hit = new RayHit(p1, normal, tangent, uv, hitDistance, side, null);
			return true;
		}
	}
}
