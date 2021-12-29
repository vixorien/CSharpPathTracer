using System;
using Microsoft.Xna.Framework;

namespace CSharpPathTracer
{
	/// <summary>
	/// A perfect sphere represented by a center point and a radius
	/// </summary>
	class Sphere : Geometry
	{
		public static Sphere Default { get { return new Sphere(); } }

		public Vector3 Center { get; set; }
		public float Radius { get; set; }

		/// <summary>
		/// Creates a new default sphere centered at the origin with a radius of 1
		/// </summary>
		public Sphere() 
			: this(Vector3.Zero, 1.0f)
		{
		}

		public Sphere(Vector3 center, float radius) : base()
		{
			// Save sphere details
			Center = center;
			Radius = radius;

			// Set up AABB
			aabb.Min = center - new Vector3(radius);
			aabb.Max = center + new Vector3(radius);
		}


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
			Vector3 n1 = p1 - Center; 
			n1.Normalize();
			if (side == HitSide.Inside) 
				n1 *= -1.0f;

			// Calculate a UV based on normal
			Vector2 xz = new Vector2(n1.X, n1.Z); xz.Normalize();
			Vector2 uv = new Vector2(
				-MathF.Acos(Vector2.Dot(Vector2.UnitX, xz)) / MathF.PI,
				-n1.Y * 0.5f + 0.5f);

			// Set up return values
			hit = new RayHit(p1, n1, uv, hitDistance, side, null);
			return true;
		}
	}
}
