

namespace CSharpPathTracer
{
	/// <summary>
	/// Base class for all geometry
	/// </summary>
	abstract class Geometry
	{
		public Material Material { get; set; }

		public Geometry(Material material)
		{
			Material = material;
		}

		public abstract bool RayIntersection(Ray ray, out RayHit[] hits);
	}
}
