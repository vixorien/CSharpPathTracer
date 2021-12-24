using System;
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
	}

	/// <summary>
	/// The result of a ray hitting geometry
	/// </summary>
	struct RayHit
	{
		public static RayHit None { get; } = new RayHit();
		public static RayHit Infinity { get; } = new RayHit(Vector3.Zero, Vector3.Zero, float.PositiveInfinity, null);

		public Vector3 Position { get; set; }
		public Vector3 Normal { get; set; }
		public float Distance { get; set; }
		public Entity Entity { get; set; }

		public RayHit(Vector3 position, Vector3 normal, float distance, Entity entity)
		{
			Position = position;
			Normal = normal;
			Distance = distance;
			Entity = entity;
		}
	}

}
