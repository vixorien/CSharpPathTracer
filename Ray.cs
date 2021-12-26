﻿using System;
using Microsoft.Xna.Framework;

namespace CSharpPathTracer
{
	/// <summary>
	/// A ray in 3D space
	/// </summary>
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
			TMin = tmin;
			TMax = tmax;
		}

		public Ray GetTransformed(Matrix transMatrix)
		{
			Ray tRay = new Ray();
			tRay.Origin = Vector3.Transform(Origin, transMatrix);
			tRay.Direction = Vector3.TransformNormal(Direction, transMatrix).Normalized();
			tRay.TMin = Vector3.TransformNormal(Direction * TMin, transMatrix).Length();
			tRay.TMax = Vector3.TransformNormal(Direction * TMax, transMatrix).Length();
			return tRay;
		}

		public static implicit operator Microsoft.Xna.Framework.Ray(Ray ray)
		{
			return new Microsoft.Xna.Framework.Ray(ray.Origin + ray.Direction * ray.TMin, ray.Direction);
		}
	}

	/// <summary>
	/// The result of a ray hitting geometry
	/// </summary>
	struct RayHit
	{
		public static RayHit None { get; } = new RayHit();
		public static RayHit Infinity { get; } = new RayHit(Vector3.Zero, Vector3.Zero, Vector2.Zero, float.PositiveInfinity, null);

		public Vector3 Position { get; set; }
		public Vector3 Normal { get; set; }
		public Vector2 UV { get; set; }
		public float Distance { get; set; }
		public IRayIntersectable HitObject { get; set; }

		public RayHit(Vector3 position, Vector3 normal, Vector2 uv, float distance, IRayIntersectable hitObj)
		{
			Position = position;
			Normal = normal;
			UV = uv;
			Distance = distance;
			HitObject = hitObj;
		}
	}

}
