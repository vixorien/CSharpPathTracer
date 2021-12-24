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

		public Sphere() : base()
		{
			Center = Vector3.Zero;
			Radius = 1.0f;
		}

		public Sphere(Vector3 center, float radius) : base()
		{
			Center = center;
			Radius = radius;
		}

		public override bool RayIntersection(Ray ray, out RayHit[] hits)
		{
			// How far along ray to closest point to sphere center
			Vector3 originToCenter = Center - ray.Origin;
			float tCenter = Vector3.Dot(originToCenter, ray.Direction);

			// If tCenter is negative, we point away from sphere
			if (tCenter < 0)
			{
				// No intersection points
				hits = new RayHit[0];
				return false;
			}

			// Distance from closest point to sphere's center
			float d = MathF.Sqrt(originToCenter.LengthSquared() - tCenter * tCenter);

			// If distance is greater than radius, we don't hit the sphere
			if (d > Radius)
			{
				// No intersection points
				hits = new RayHit[0];
				return false;
			}

			// Offset from tCenter to an intersection point
			float offset = MathF.Sqrt(Radius * Radius - d * d);

			// Distances to the two hit points
			float t1 = tCenter - offset;
			float t2 = tCenter + offset;

			// Points of intersection
			Vector3 p1 = ray.Origin + ray.Direction * t1;
			Vector3 p2 = ray.Origin + ray.Direction * t2;

			// Normals
			Vector3 n1 = p1 - Center; n1.Normalize();
			Vector3 n2 = p2 - Center; n2.Normalize();

			// Set up return values
			hits = new RayHit[2];
			hits[0] = new RayHit(p1, n1, t1, null);
			hits[1] = new RayHit(p2, n2, t2, null);
			return true;
		}
	}
}
