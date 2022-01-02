
using BoundingBox = Microsoft.Xna.Framework.BoundingBox;

namespace CSharpPathTracer
{
	interface IBoundable
	{
		BoundingBox AABB { get; }
	}
}
