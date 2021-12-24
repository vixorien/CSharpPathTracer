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
			Ray tRay = ray;
			tRay.Origin = Vector3.Transform(ray.Origin, Transform.WorldMatrix);
			tRay.Direction = Vector3.TransformNormal(ray.Direction, Transform.WorldMatrix);

			// Perform the intersection using the transformed ray
			if (Geometry.RayIntersection(tRay, out hits))
			{
				// Transform the results back into world space
				Matrix worldInv = Matrix.Invert(Transform.WorldMatrix);
				for (int i = 0; i < hits.Length; i++)
				{
					hits[i].Position = Vector3.Transform(hits[i].Position, worldInv);
					hits[i].Normal = Vector3.TransformNormal(hits[i].Normal, worldInv);
				}
				
				// Success
				return true;
			}

			// Nothing hit
			return false;
		}
	}
}
