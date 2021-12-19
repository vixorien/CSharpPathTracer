using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CSharpRaytracing
{
	struct Sphere
	{
		public static Sphere Null { get { return new Sphere(Vector3.Zero, 0, null); } }

		public Vector3 Center { get; set; }
		public float Radius { get; set; }

		public Material Material { get; set; }

		public Sphere(Vector3 center, float radius, Material mat)
		{
			Center = center;
			Radius = radius;
			Material = mat;
		}
	}

	struct SphereIntersection
	{
		public Vector3 Position { get; }
		public Vector3 Normal { get; }
		public float Distance { get; }
		public Sphere Sphere { get; }

		public SphereIntersection(Vector3 position, Vector3 normal, float distance, Sphere sphere)
		{
			Position = position;
			Normal = normal;
			Distance = distance;
			Sphere = sphere;
		}
	}

	struct Ray
	{
		public Vector3 Origin { get; set; }
		public Vector3 Direction { get; set; }
		public float TMin { get; set; }
		public float TMax { get; set; }

		public Ray(Vector3 origin, Vector3 direction) 
			: this(origin, direction, 0.0f, 1000.0f)
		{ }

		public Ray(Vector3 origin, Vector3 direction, float tmin, float tmax)
		{
			Origin = origin;
			Direction = direction;
			Direction.Normalize();
			TMin = tmin;
			TMax = tmax;
		}

		public bool Intersect(Sphere sphere, out SphereIntersection[] hits)
		{
			// How far along ray to closest point to sphere center
			Vector3 originToCenter = sphere.Center - this.Origin;
			float tCenter = Vector3.Dot(originToCenter, this.Direction);

			// If tCenter is negative, we point away from sphere
			if (tCenter < 0)
			{
				// No intersection points
				hits = new SphereIntersection[0];
				return false;
			}

			// Distance from closest point to sphere's center
			float d = MathF.Sqrt(originToCenter.LengthSquared() - tCenter * tCenter);
			
			// If distance is greater than radius, we don't hit the sphere
			if (d > sphere.Radius)
			{
				// No intersection points
				hits = new SphereIntersection[0];
				return false;
			}

			// Offset from tCenter to an intersection point
			float offset = MathF.Sqrt(sphere.Radius * sphere.Radius - d * d);

			// Distances to the two hit points
			float t1 = tCenter - offset;
			float t2 = tCenter + offset;

			// Points of intersection
			Vector3 p1 = this.Origin + this.Direction * t1;
			Vector3 p2 = this.Origin + this.Direction * t2;

			// Normals
			Vector3 n1 = p1 - sphere.Center; n1.Normalize();
			Vector3 n2 = p2 - sphere.Center; n2.Normalize();

			// Set up return values
			hits = new SphereIntersection[2];
			hits[0] = new SphereIntersection(p1, n1, t1, sphere);
			hits[1] = new SphereIntersection(p2, n2, t2, sphere);
			return true;
		}
	}
}
