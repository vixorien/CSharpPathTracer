

namespace CSharpPathTracer
{
	/// <summary>
	/// Base class for all geometry
	/// </summary>
	abstract class Geometry
	{
		public Geometry()
		{
		}

		public abstract bool RayIntersection(Ray ray, out RayHit[] hits);
	}
}
