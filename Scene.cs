using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace CSharpPathTracer
{
	class Scene:  IBoundable, IRayIntersectable
	{
		private List<Entity> entities;
		private Octree<Entity> octree;

		/// <summary>
		/// Gets or sets the name of the scene
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the scene's environment
		/// </summary>
		public Environment Environment { get; set; }

		/// <summary>
		/// Gets the bounds of the scene
		/// </summary>
		public BoundingBox AABB { get; private set; }

		/// <summary>
		/// Gets the count of entities in the scene
		/// </summary>
		public int EntityCount { get { return entities.Count; } }

		/// <summary>
		/// Gets the entity in the scene at the specified index
		/// </summary>
		/// <param name="index">The index of the entity</param>
		/// <returns>An entity in the scene</returns>
		public Entity this[int index]
		{
			get { return entities[index]; }
		}

		/// <summary>
		/// Creates a new scene
		/// </summary>
		/// <param name="name">The name of the scene</param>
		/// <param name="env">The scene's environment</param>
		/// <param name="bounds">The bounds of the scene for octree creation</param>
		public Scene(string name, Environment env, BoundingBox bounds)
		{
			Name = name;
			Environment = env;
			entities = new List<Entity>();
			octree = new Octree<Entity>(bounds);
		}

		/// <summary>
		/// Adds an entity to the scene
		/// </summary>
		/// <param name="entity">The entity to add</param>
		public void Add(Entity entity)
		{
			if (entity == null)
				return;

			// Add to our list and the octree
			entities.Add(entity);
			octree.AddObject(entity);
		}

		/// <summary>
		/// Gets the closest entity along the given ray, if any
		/// </summary>
		/// <param name="ray">The ray to check</param>
		/// <param name="hit">The hit information</param>
		/// <returns>True if a hit is encountered, false otherwise</returns>
		//public bool ClosestHit(Ray ray, out RayHit hit)
		//{
		//	// No hits yet
		//	bool anyHit = false;
		//	hit = RayHit.Infinity;

		//	// Loop through scene and check all entities
		//	foreach (Entity e in entities)
		//	{
		//		RayHit currentHit;
		//		if (e.RayIntersection(ray, out currentHit))
		//		{
		//			// We have a hit; was it closest?
		//			if (currentHit.Distance < hit.Distance)
		//			{
		//				hit = currentHit;
		//				anyHit = true;
		//			}
		//		}
		//	}

		//	return anyHit;
		//}


		public bool RayIntersection(Ray ray, out RayHit hit)
		{
			// Just use the octree
			return octree.RayIntersection(ray, out hit);
		}
	}
}
