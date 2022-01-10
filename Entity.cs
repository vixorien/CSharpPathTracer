using System.Numerics;

namespace CSharpPathTracer
{
	/// <summary>
	/// An entity in a 3D scene
	/// </summary>
	class Entity : IBoundable, IRayIntersectable
	{
		/// <summary>
		/// Gets the entity's transform
		/// </summary>
		public Transform Transform { get; private set; }

		/// <summary>
		/// Gets or sets the entity's geometry
		/// </summary>
		public Geometry Geometry { get; set; }

		/// <summary>
		/// Gets or sets the entity's material
		/// </summary>
		public Material Material { get; set; }

		/// <summary>
		/// Gets an AABB that encompasses the transformed entity
		/// </summary>
		public AABB AABB { get { return Geometry.AABB.GetTransformed(Transform); } }

		/// <summary>
		/// Creates a new entity
		/// </summary>
		/// <param name="geometry">The entity's geometry</param>
		/// <param name="material">The entity's material</param>
		public Entity(Geometry geometry, Material material)
		{
			Transform = new Transform();
			Geometry = geometry;
			Material = material;
		}

		/// <summary>
		/// Performs a ray intersection with the entity's geometry
		/// </summary>
		/// <param name="ray">The ray to check</param>
		/// <param name="hit">Hit information (if a hit occurs)</param>
		/// <returns>True if a hit occurs, false otherwise</returns>
		public bool RayIntersection(Ray ray, out RayHit hit)
		{
			// Transform the ray into the entity's space
			Matrix4x4 worldInv = Transform.WorldMatrix.Invert();
			Ray transformedRay = ray.GetTransformed(worldInv);

			// Perform the intersection using the transformed ray
			if (Geometry.RayIntersection(transformedRay, out hit))
			{
				// Handle distance first
				Vector3 rayToHit = hit.Position - transformedRay.Origin;
				hit.Distance = Vector3.TransformNormal(rayToHit, Transform.WorldMatrix).Length();

				// Transform params
				hit.Position = Vector3.Transform(hit.Position, Transform.WorldMatrix);
				hit.Normal = Vector3.Normalize(Vector3.TransformNormal(hit.Normal, Transform.WorldMatrix));

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
