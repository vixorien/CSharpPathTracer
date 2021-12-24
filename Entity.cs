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
			ray.Origin = Vector3.Transform(ray.Origin, worldInv);
			ray.Direction = Vector3.TransformNormal(ray.Direction, worldInv).Normalized();

			// Perform the intersection using the transformed ray
			if (Geometry.RayIntersection(ray, out hits))
			{
				// Transform the results back into world space
				for (int i = 0; i < hits.Length; i++)
				{
					// Handle distance first
					Vector3 rayToHit = hits[i].Position - ray.Origin;
					hits[i].Distance = Vector3.TransformNormal(rayToHit, Transform.WorldMatrix).Length();

					// Transform params
					hits[i].Position = Vector3.Transform(hits[i].Position, Transform.WorldMatrix);
					hits[i].Normal = Vector3.TransformNormal(hits[i].Normal, Transform.WorldMatrix).Normalized();

					// Save this entity
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
