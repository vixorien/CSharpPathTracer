﻿
using System.Numerics;

namespace CSharpPathTracer
{
	/// <summary>
	/// A ray in 3D space
	/// </summary>
	struct Ray
	{
		private Vector3 direction;
		private Vector3 invDirection; // For fast ray-box intersections

		public Vector3 Origin { get; set; }
		public Vector3 Direction {
			get => direction;
			set
			{
				direction = value;
				invDirection = Vector3.One / value;
			}
		}
		public Vector3 InvDirection { get => invDirection; }
		public float TMin { get; set; }
		public float TMax { get; set; }
		public bool Specular { get; set; }

		public Ray(Vector3 origin, Vector3 direction, bool specular = false)
			: this(origin, direction, 0.0f, 1000.0f, specular)
		{ }

		public Ray(Vector3 origin, Vector3 direction, float tmin, float tmax, bool specular = false)
		{
			Origin = origin;
			this.direction = direction;
			invDirection = Vector3.One / direction;
			TMin = tmin;
			TMax = tmax;
			Specular = specular;
		}

		public Ray GetTransformed(Matrix4x4 transMatrix)
		{
			Ray tRay = new Ray();
			tRay.Origin = Vector3.Transform(Origin, transMatrix);
			tRay.Direction = Vector3.Normalize(Vector3.TransformNormal(Direction, transMatrix));
			tRay.TMin = Vector3.TransformNormal(Direction * TMin, transMatrix).Length();
			tRay.TMax = Vector3.TransformNormal(Direction * TMax, transMatrix).Length();
			tRay.Specular = this.Specular;
			return tRay;
		}
	}

	/// <summary>
	/// Which side of an object was hit by a ray?
	/// </summary>
	public enum HitSide { Inside, Outside }

	/// <summary>
	/// The result of a ray hitting geometry
	/// </summary>
	struct RayHit
	{
		public static RayHit None { get; } = new RayHit();
		public static RayHit Infinity { get; } = new RayHit(Vector3.Zero, Vector3.Zero, Vector3.Zero, Vector2.Zero, float.PositiveInfinity, HitSide.Outside, null);

		public Vector3 Position { get; set; }
		public Vector3 Normal { get; set; }
		public Vector3 Tangent { get; set; }
		public Vector2 UV { get; set; }
		public float Distance { get; set; }
		public HitSide Side { get; set; }
		public IRayIntersectable HitObject { get; set; }

		public RayHit(Vector3 position, Vector3 normal, Vector3 tangent, Vector2 uv, float distance, HitSide side, IRayIntersectable hitObj)
		{
			Position = position;
			Normal = normal;
			Tangent = tangent;
			UV = uv;
			Distance = distance;
			Side = side;
			HitObject = hitObj;
		}
	}

}
