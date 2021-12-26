using Microsoft.Xna.Framework;

namespace CSharpPathTracer
{
	class Entity : IBoundable, IRayIntersectable
	{
		public Transform Transform { get; private set; }
		public Geometry Geometry { get; set; }
		public Material Material { get; set; }

		public BoundingBox AABB { get { return Geometry.AABB.GetTransformed(Transform); } }

		public Entity(Geometry geometry, Material material)
		{
			Transform = new Transform();
			Geometry = geometry;
			Material = material;
		}

		public bool RayIntersection(Ray ray, out RayHit hit)
		{
			// Transform the ray into the entity's space
			Matrix worldInv = Matrix.Invert(Transform.WorldMatrix);
			Ray transformedRay = ray.GetTransformed(worldInv);

			// Perform the intersection using the transformed ray
			if (Geometry.RayIntersection(transformedRay, out hit))
			{
				// Handle distance first
				Vector3 rayToHit = hit.Position - transformedRay.Origin;
				hit.Distance = Vector3.TransformNormal(rayToHit, Transform.WorldMatrix).Length();

				// Transform params
				hit.Position = Vector3.Transform(hit.Position, Transform.WorldMatrix);
				hit.Normal = Vector3.TransformNormal(hit.Normal, Transform.WorldMatrix).Normalized();

				// Save this entity
				hit.HitObject = this;
				
				// Success
				return true;
			}

			// Nothing hit
			return false;
		}
	}
}
