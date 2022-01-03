using System.Numerics;

using BoundingBox = Microsoft.Xna.Framework.BoundingBox;

namespace CSharpPathTracer
{
	/// <summary>
	/// Base class for all geometry
	/// </summary>
	abstract class Geometry : IBoundable, IRayIntersectable
	{
		// Fields
		protected AABB aabb;

		/// <summary>
		/// Gets the local (non-transformed) AABB bounding box of this geometry
		/// </summary>
		public AABB AABB => aabb;

		/// <summary>
		/// Sets up the base-level geometry
		/// </summary>
		public Geometry()
		{
			// Set up AABB such that the first added data will be valid
			aabb = new AABB(
				new Vector3(float.PositiveInfinity),  // Min starts at max
				new Vector3(float.NegativeInfinity)); // Max starts at min
		}

		/// <summary>
		/// Performs a ray intersection on this geometry
		/// </summary>
		/// <param name="ray">The ray for the intersection test</param>
		/// <param name="hits">The hit info</param>
		/// <returns>True if an intersection occurs, false otherwise</returns>
		public abstract bool RayIntersection(Ray ray, out RayHit hit);
	}
}
