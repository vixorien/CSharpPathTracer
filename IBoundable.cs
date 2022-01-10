

namespace CSharpPathTracer
{
	/// <summary>
	/// Represents an object that has axis-aligned bounds
	/// </summary>
	interface IBoundable
	{
		/// <summary>
		/// Gets the object's AABB
		/// </summary>
		AABB AABB { get; }
	}
}
