using System.Collections.Generic;

namespace CSharpPathTracer
{
	class Scene
	{
		private List<Entity> entities;

		/// <summary>
		/// Gets or sets the name of the scene
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the scene's environment
		/// </summary>
		public Environment Environment { get; set; }

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
		public Scene(string name, Environment env)
		{
			Name = name;
			Environment = env;
			entities = new List<Entity>();
		}

		/// <summary>
		/// Adds an entity to the scene
		/// </summary>
		/// <param name="entity">The entity to add</param>
		public void Add(Entity entity)
		{
			if (entity == null)
				return;

			entities.Add(entity);
		}

		/// <summary>
		/// Gets the closest entity along the given ray, if any
		/// </summary>
		/// <param name="ray">The ray to check</param>
		/// <param name="hit">The hit information</param>
		/// <returns>True if a hit is encountered, false otherwise</returns>
		public bool ClosestHit(Ray ray, out RayHit hit)
		{
			// No hits yet
			bool anyHit = false;
			hit = RayHit.Infinity;

			// Loop through scene and check all spheres
			foreach (Entity e in entities)
			{
				RayHit[] currentHits;
				if (e.RayIntersection(ray, out currentHits))
				{
					// We have a hit; was it closest?
					if (currentHits[0].Distance < hit.Distance)
					{
						hit = currentHits[0];
						anyHit = true;
					}
				}
			}

			return anyHit;
		}

	}
}
