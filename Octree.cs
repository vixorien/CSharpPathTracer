using System.Collections.Generic;

namespace CSharpPathTracer
{
	/// <summary>
	/// A tree of AABBs
	/// </summary>
	/// <typeparam name="T">The type of object to store in the octree</typeparam>
	class Octree<T> : IBoundable, IRayIntersectable
		where T : IBoundable, IRayIntersectable
	{
		/// <summary>
		/// The amount of objects at which an oct attempts to divide
		/// </summary>
		private const int DivideAt = 2;

		// Should overlapping objects be allowed in an oct node?
		private bool allowOverlaps;

		// The objects stored in this node
		private List<T> objects;

		// The child nodes
		private Octree<T>[] children;

		/// <summary>
		/// Gets whether or not this oct has been divided
		/// </summary>
		public bool Divided { get { return children != null; } }

		/// <summary>
		/// Gets the overall AABB of this oct
		/// </summary>
		public AABB AABB { get; private set; }

		/// <summary>
		/// Gets the shrunken AABB of this oct, if it exists
		/// </summary>
		public AABB? ShrunkAABB { get; private set; }

		/// <summary>
		/// Creates a new Octree node
		/// </summary>
		/// <param name="bounds">The bounds of this new node</param>
		/// <param name="allowOverlaps">Are overlapping objects allowed?</param>
		public Octree(AABB bounds, bool allowOverlaps = true)
		{
			this.allowOverlaps = allowOverlaps;

			AABB = bounds;
			ShrunkAABB = null;

			objects = new List<T>();
		}

		/// <summary>
		/// Recursively performs a ray intersection on this octree node
		/// </summary>
		/// <param name="ray">The ray for the intersection test</param>
		/// <param name="hits">The hit info</param>
		/// <returns>True if an intersection occurs, false otherwise</returns>
		public bool RayIntersection(Ray ray, out RayHit hit)
		{
			// Which AABB?
			AABB boxToCheck = ShrunkAABB.HasValue ? ShrunkAABB.Value : AABB;

			// Does the ray hit this oct?
			if (boxToCheck.Intersects(ray))
			{
				// Closest hit could be from this quad or children
				RayHit closestHit = RayHit.Infinity;
				bool anyHit = false;

				// Check all geometry here first
				foreach (T geo in objects)
				{
					// Is there and intersection and is it the closest?
					if (geo.RayIntersection(ray, out hit) &&
						hit.Distance < closestHit.Distance)
					{
						closestHit = hit;
						anyHit = true;
					}
				}

				// Now recursively check children if necessary
				if (Divided)
				{
					foreach (Octree<T> child in children)
					{
						if (child != null &&
							child.RayIntersection(ray, out hit) &&
							hit.Distance < closestHit.Distance)
						{
							closestHit = hit;
							anyHit = true;
						}
					}
				}

				// Was anything hit?
				if (anyHit)
				{
					hit = closestHit;
					return true;
				}
			}

			// Nothing hit
			hit = RayHit.None;
			return false;
		}

		/// <summary>
		/// Adds an object to this node
		/// </summary>
		/// <param name="obj">The object to add</param>
		/// <returns>True if the object is successfully added, false otherwise</returns>
		public bool AddObject(T obj)
		{
			// Are overlapping objects allowed?
			if (allowOverlaps)
			{
				// We're allowing overlaps, so ensure the object COULD fit
				// if it were within the oct's AABB
				if (AABB.Contains(obj.AABB) == AABBContainment.NoOverlap ||
					!AABB.CouldFit(obj.AABB))
					return false;
			}
			else
			{
				// No overlaps, so the object must entirely be contained
				if (AABB.Contains(obj.AABB) != AABBContainment.Contains)
					return false;
			}

			// Do we need to add here or a child?
			if (Divided)
			{
				// Loop and check children
				foreach (Octree<T> oct in children)
				{
					// Attempt to add, and if it worked, return success
					if (oct != null && oct.AddObject(obj))
						return true;
				}
			}

			// Was not added to a child, and we know it actually
			// fits here, so just add here
			objects.Add(obj);

			// Do we need to divide?
			if (!Divided && objects.Count >= DivideAt)
				Divide();

			// We've definitely added
			return true;
		}

		/// <summary>
		/// Shrinks AABBs and prunes empty nodes
		/// </summary>
		public void ShrinkAndPrune()
		{
			// Note: Should we reset here?  Or prevent this from happening twice?
			//       Maybe finalize the oct so nothing else can be added?

			// Recursively shrink child nodes first
			if (Divided)
			{
				// Track how many child octs have been pruned
				int pruneCount = 0;
				int validChildCount = 0;
				Octree<T> lastChildFound = null;
				for (int i = 0; i < children.Length; i++)
				{
					// Grab this child
					Octree<T> child = children[i];
					if (child == null)
						continue;

					// Is this child a leaf node with no objects?
					if (!child.Divided && child.objects.Count == 0)
					{
						// No children; prune!
						children[i] = null;
						pruneCount++;
						continue;
					}

					// Attempt to shrink and prune the child
					child.ShrinkAndPrune();

					// Add the child's shrunken AABB to this one, if necessary
					if (child.ShrunkAABB.HasValue)
					{
						if (ShrunkAABB.HasValue)
							ShrunkAABB = AABB.Combine(ShrunkAABB.Value, child.ShrunkAABB.Value);
						else
							ShrunkAABB = child.ShrunkAABB.Value;
					}

					// Remember the last child
					lastChildFound = child;
					validChildCount++;
				}

				// Has this entire node been pruned?
				if (pruneCount == 8)
				{
					// Just un-divide!
					this.children = null;
				}
				else if (validChildCount == 1 && objects.Count == 0)
				{
					// We only have a single child, but that
					// child may have more.  So lets just use
					// that child's data instead of our own
					this.children = lastChildFound.children;
					this.objects = lastChildFound.objects;
					this.ShrunkAABB = lastChildFound.ShrunkAABB.Value;
				}
			}

			// Any objects here?
			foreach (T obj in objects)
			{
				// Is there a value?
				if (ShrunkAABB.HasValue)
					ShrunkAABB = AABB.Combine(ShrunkAABB.Value, obj.AABB);
				else
					ShrunkAABB = obj.AABB;
			}
		}

		/// <summary>
		/// Divides the octree into 8 children
		/// </summary>
		private void Divide()
		{
			// Necessary?
			if (Divided)
				return;

			// Time to divide - create the 8 children
			children = new Octree<T>[8];
			children[0] = new Octree<T>(AABB.LeftHalf().BottomHalf().FrontHalf(), allowOverlaps);    // 0: -X, -Y, -Z
			children[1] = new Octree<T>(AABB.LeftHalf().BottomHalf().BackHalf(), allowOverlaps);     // 1: -X, -Y, +Z
			children[2] = new Octree<T>(AABB.LeftHalf().TopHalf().FrontHalf(), allowOverlaps);       // 2: -X, +Y, -Z
			children[3] = new Octree<T>(AABB.LeftHalf().TopHalf().BackHalf(), allowOverlaps);        // 3: -X, +Y, +Z
			children[4] = new Octree<T>(AABB.RightHalf().BottomHalf().FrontHalf(), allowOverlaps);   // 4: +X, -Y, -Z
			children[5] = new Octree<T>(AABB.RightHalf().BottomHalf().BackHalf(), allowOverlaps);    // 5: +X, -Y, +Z
			children[6] = new Octree<T>(AABB.RightHalf().TopHalf().FrontHalf(), allowOverlaps);      // 6: +X, +Y, -Z
			children[7] = new Octree<T>(AABB.RightHalf().TopHalf().BackHalf(), allowOverlaps);       // 7: +X, +Y, +Z

			// Attempt to move each piece of geometry to a child
			for (int i = 0; i < objects.Count; i++)
			{
				// Try this geometry in a child
				bool added = false;
				foreach (Octree<T> oct in children)
				{
					if (oct.AddObject(objects[i]))
					{
						added = true;
						objects.RemoveAt(i);
						break;
					}
				}

				// If the geometry was added, the list has
				// shifted to check the same one again
				if (added)
					i--;
			}

		}
	}
}
