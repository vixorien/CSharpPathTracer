
namespace CSharpPathTracer
{
	/// <summary>
	/// Represents an object that can be intersected by a ray
	/// </summary>
	interface IRayIntersectable
	{
		/// <summary>
		/// Performs a ray intersection with the object
		/// </summary>
		/// <param name="ray">The ray to check</param>
		/// <param name="hit">Hit information (if a hit occurs)</param>
		/// <returns>True if a hit occurs, false otherwise</returns>
		bool RayIntersection(Ray ray, out RayHit hit);
	}
}
