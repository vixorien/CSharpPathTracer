
namespace CSharpPathTracer
{
	interface IRayIntersectable
	{
		bool RayIntersection(Ray ray, out RayHit hit);
	}
}
