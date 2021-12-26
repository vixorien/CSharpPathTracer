﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace CSharpPathTracer
{
	class Octree<T> : IBoundable, IRayIntersectable
		where T : IBoundable, IRayIntersectable
	{
		private const int DivideAt = 3;

		private List<T> objects;
		private Octree<T>[] children;
		
		public bool Divided { get { return children != null; } }
		public BoundingBox AABB { get; private set; }

		public Octree(BoundingBox bounds)
		{
			AABB = bounds;
			objects = new List<T>();
		}

		public bool RayIntersection(Ray ray, out RayHit hit)
		{
			// Does the ray hit this oct?
			if (AABB.Intersects(ray).HasValue)
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
						if (child.RayIntersection(ray, out hit) &&
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

		public bool AddObject(T obj)
		{
			// Does the geometry fit inside this node?
			if (AABB.Contains(obj.AABB) != ContainmentType.Contains)
				return false;

			// Do we need to add here or a child?
			if (Divided)
			{
				// Loop and check children
				foreach (Octree<T> oct in children)
				{
					// Attempt to add, and if it worked, return success
					if(oct.AddObject(obj))
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

		private void Divide()
		{
			// Necessary?
			if (Divided) 
				return;

			// Time to divide - create the 8 children
			// 0: -X, -Y, -Z
			// 1: -X, -Y, +Z
			// 2: -X, +Y, -Z
			// 3: -X, +Y, +Z
			// 4: +X, -Y, -Z
			// 5: +X, -Y, +Z
			// 6: +X, +Y, -Z
			// 7: +X, +Y, +Z
			children = new Octree<T>[8];
			children[0] = new Octree<T>(AABB.LeftHalf().BottomHalf().FrontHalf());
			children[1] = new Octree<T>(AABB.LeftHalf().BottomHalf().BackHalf());
			children[2] = new Octree<T>(AABB.LeftHalf().TopHalf().FrontHalf());
			children[3] = new Octree<T>(AABB.LeftHalf().TopHalf().BackHalf());
			children[4] = new Octree<T>(AABB.RightHalf().BottomHalf().FrontHalf());
			children[5] = new Octree<T>(AABB.RightHalf().BottomHalf().BackHalf());
			children[6] = new Octree<T>(AABB.RightHalf().TopHalf().FrontHalf());
			children[7] = new Octree<T>(AABB.RightHalf().TopHalf().BackHalf());

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