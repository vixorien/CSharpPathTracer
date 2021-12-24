using Microsoft.Xna.Framework;

namespace CSharpPathTracer
{
	class Entity
	{
		public Transform Transform { get; private set; }
		public Geometry Geometry { get; set; }
		public Material Material { get; set; }

		public Entity(Geometry geometry, Material material)
		{
			Transform = new Transform();
			Geometry = geometry;
			Material = material;
		}

		public bool RayIntersection(Ray ray, out RayHit[] hits)
		{
			// Transform the ray into the entity's space
			Matrix worldInv = Matrix.Invert(Transform.WorldMatrix);
			Ray tRay = ray;
			tRay.Origin = Vector3.Transform(ray.Origin, worldInv);
			tRay.Direction = Vector3.TransformNormal(ray.Direction, worldInv);

			// Perform the intersection using the transformed ray
			if (Geometry.RayIntersection(tRay, out hits))
			{
				// Transform the results back into world space
				
				for (int i = 0; i < hits.Length; i++)
				{
					hits[i].Position = Vector3.Transform(hits[i].Position, Transform.WorldMatrix);
					hits[i].Normal = Vector3.TransformNormal(hits[i].Normal, Transform.WorldMatrix);
					hits[i].Entity = this;
				}
				
				// Success
				return true;
			}

			// Nothing hit
			return false;
		}
	}
}
