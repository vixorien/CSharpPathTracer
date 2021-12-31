using System;
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
		/// Finalizes the octree by shrinking all
		/// internal AABBs to closely match their objects
		/// </summary>
		public void FinalizeOctree()
		{
			octree.ShrinkAndPrune();
		}

		/// <summary>
		/// Determines the closest object, if any, hit
		/// by the specified ray
		/// </summary>
		/// <param name="ray">The ray to check</param>
		/// <param name="hit">Information about the hit</param>
		/// <returns>True if a hit occurs, false otherwise</returns>
		public bool RayIntersection(Ray ray, out RayHit hit)
		{
			// Just use the octree
			return octree.RayIntersection(ray, out hit);
		}

		/// <summary>
		/// Generates a set of scenes for testing
		/// </summary>
		/// <returns>A list of scenes</returns>
		public static List<Scene> GenerateScenes()
		{
			List<Scene> scenes = new List<Scene>();

			// Overall scene bounds
			BoundingBox sceneBounds = new BoundingBox(
				new Vector3(-10000),
				new Vector3(10000));

			// === Textures ===
			Texture crateTexture = new Texture("Content/Textures/crate.png");
			Texture tilesTexture = new Texture("Content/Textures/tiles.png");
			Texture tilesTextureNoGamma = new Texture("Content/Textures/tiles.png", false);

			Texture skyRight = new Texture("Content/Skies/Clouds Blue/right.png");
			Texture skyLeft = new Texture("Content/Skies/Clouds Blue/left.png");
			Texture skyUp = new Texture("Content/Skies/Clouds Blue/up.png");
			Texture skyDown = new Texture("Content/Skies/Clouds Blue/down.png");
			Texture skyBack = new Texture("Content/Skies/Clouds Blue/back.png");
			Texture skyFront = new Texture("Content/Skies/Clouds Blue/front.png");

			// === Environments ===
			Environment environment = new EnvironmentGradient(
				System.Drawing.Color.CornflowerBlue.ToVector3(),
				System.Drawing.Color.White.ToVector3(),
				System.Drawing.Color.White.ToVector3());

			Environment skybox = new EnvironmentSkybox(
				skyRight,
				skyLeft,
				skyUp,
				skyDown,
				skyBack,
				skyFront);


			// === Materials ===
			Material crate = new DiffuseMaterial(Vector3.One, crateTexture);
			Material tiles = new DiffuseMaterial(new Vector3(0.2f, 1.0f, 0.2f), tilesTexture);
			Material grayMatte = new DiffuseMaterial(System.Drawing.Color.LightGray.ToVector3(), null, null, 0.5f);
			Material greenMatte = new DiffuseMaterial(new Vector3(0.2f, 1.0f, 0.2f));
			Material blueMatte = new DiffuseMaterial(new Vector3(0.2f, 0.2f, 1.0f), tilesTexture);

			Material metalTiles = new MetalMaterial(new Vector3(1.000f, 0.766f, 0.336f), null, tilesTextureNoGamma);
			Material mirror = new MetalMaterial(new Vector3(1, 1, 1));
			Material gold = new MetalMaterial(new Vector3(1.000f, 0.766f, 0.336f));

			Material transparent = new TransparentMaterial(new Vector3(1, 1, 1), 1.5f);
			Material transparentRough = new TransparentMaterial(Vector3.One, 1.5f, null, tilesTextureNoGamma);

			// === Meshes ===
			Mesh cubeMesh = new Mesh("Content/Models/cube.obj");
			Mesh helixMesh = new Mesh("Content/Models/helix.obj");
			Mesh sphereMesh = new Mesh("Content/Models/sphere.obj");

			// === DEFAULT SCENE with and without skybox ===
			{
				Entity ground = new Entity(Sphere.Default, grayMatte);
				ground.Transform.SetPosition(0, -1000, 0);
				ground.Transform.SetScale(1000);

				Entity left = new Entity(Sphere.Default, crate);
				left.Transform.SetPosition(-5, 2, 0);
				left.Transform.SetScale(2);

				Entity middle = new Entity(Sphere.Default, mirror);
				middle.Transform.SetPosition(0, 4, 0);
				middle.Transform.SetScale(2);

				Entity right = new Entity(Sphere.Default, metalTiles);
				right.Transform.SetPosition(5, 2, 0);
				right.Transform.SetScale(2);

				Entity close = new Entity(Sphere.Default, transparent);
				close.Transform.SetPosition(0, 2, 5);
				close.Transform.SetScale(2);

				Scene scene = new Scene("Default", environment, sceneBounds);
				scene.Add(ground);
				scene.Add(left);
				scene.Add(middle);
				scene.Add(right);
				scene.Add(close);

				Scene sceneSkybox = new Scene("Default + Skybox", skybox, sceneBounds);
				sceneSkybox.Add(ground);
				sceneSkybox.Add(left);
				sceneSkybox.Add(middle);
				sceneSkybox.Add(right);
				sceneSkybox.Add(close);

				scenes.Add(scene);
				scenes.Add(sceneSkybox);
			}

			// === MESH SCENE ===
			{
				// Entities ===
				Entity cube = new Entity(cubeMesh, transparent);
				cube.Transform.ScaleRelative(3.0f);
				cube.Transform.Rotate(MathHelper.PiOver4, MathHelper.PiOver4, 0.0f);
				cube.Transform.MoveAbsolute(0, 2.0f, 0);

				Entity helix = new Entity(helixMesh, blueMatte);
				helix.Transform.MoveAbsolute(0, 2.5f, 0);
				helix.Transform.ScaleRelative(5.0f);

				Entity sphere = new Entity(sphereMesh, mirror);
				sphere.Transform.MoveAbsolute(0, 1, 0);
				sphere.Transform.ScaleRelative(2.0f);

				Entity ground = new Entity(Sphere.Default, grayMatte);
				ground.Transform.SetPosition(0, -1000, 0);
				ground.Transform.SetScale(1000);

				// Create scene
				Scene scene = new Scene("Mesh Test", environment, sceneBounds);
				//scene.Add(cube);
				scene.Add(ground);
				scene.Add(helix);
				//scene.Add(sphere);

				// Add to scene list
				scenes.Add(scene);
			}

			// === RANDOM SCENE ===
			{
				// Entities
				Entity ground = new Entity(Sphere.Default, grayMatte);
				ground.Transform.SetPosition(0, -1000, 0);
				ground.Transform.SetScale(1000);

				// Create scene
				Scene scene = new Scene("Random Spheres", environment, sceneBounds);
				scene.Add(ground);

				// Add random entities
				Random rng = ThreadSafeRandom.Instance;
				for (int i = 0; i < 100; i++)
				{
					// Random scale (used for height, too)
					float scale = rng.NextFloat(0.1f, 1.0f);

					
					Material m = null;
					float randomMaterial = rng.NextFloat();
					if (randomMaterial < 0.5f) 
						m = new DiffuseMaterial(rng.NextColor());
					else if (randomMaterial < 0.8f) 
						m = new MetalMaterial(rng.NextColor(), null, null, rng.NextFloat());
					else 
						m = new TransparentMaterial(Vector3.One, 1.5f);

					Entity s = new Entity(Sphere.Default, m);
					s.Transform.SetPosition(rng.NextFloat(-20, 20), scale, rng.NextFloat(-20, 20));
					s.Transform.SetScale(scale);
					scene.Add(s);
				}

				// Add to scene list
				scenes.Add(scene);
			}

			// Finalize all scenes and return
			foreach (Scene s in scenes)	
				s.FinalizeOctree();
			return scenes;
		}
	}
}
