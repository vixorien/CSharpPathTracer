using Microsoft.Xna.Framework;

namespace CSharpPathTracer
{
	interface IBoundable
	{
		BoundingBox AABB { get; }
	}
}
