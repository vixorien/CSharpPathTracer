﻿using System;
using System.Collections.Generic;
using System.Numerics;

namespace CSharpPathTracer
{
	/// <summary>
	/// A 3D scene that can be raytraced
	/// </summary>
	class Scene : IBoundable, IRayIntersectable
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
		public AABB AABB { get; private set; }

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
		public Scene(string name, Environment env, AABB bounds)
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
			AABB sceneBounds = new AABB(
				new Vector3(-10000),
				new Vector3(10000));

			// === Textures ===
			Texture crateTexture = new Texture("Content/Textures/crate.png");
			Texture checkerboardTexture = Texture.CreateCheckerboard(256, 256, 4, 4, new Vector4(0,0,0,1), new Vector4(1,1,1,1));
			Texture tilesTextureNoGamma = new Texture("Content/Textures/tiles.png", false);
			Texture lavaAlbedoTexture = new Texture("Content/Textures/lava_albedo.png");
			Texture lavaEmissiveTexture = new Texture("Content/Textures/lava_emissive.png");
			Texture lavaNormalTexture = new Texture("Content/Textures/lava_normals.png", false);

			Texture skyRight = new Texture("Content/Skies/Clouds Blue/right.png");
			Texture skyLeft = new Texture("Content/Skies/Clouds Blue/left.png");
			Texture skyUp = new Texture("Content/Skies/Clouds Blue/up.png");
			Texture skyDown = new Texture("Content/Skies/Clouds Blue/down.png");
			Texture skyBack = new Texture("Content/Skies/Clouds Blue/back.png");
			Texture skyFront = new Texture("Content/Skies/Clouds Blue/front.png");

			// === Environments ===
			Environment environmentLight = new EnvironmentGradient(
				System.Drawing.Color.CornflowerBlue.ToVector3(),
				System.Drawing.Color.White.ToVector3(),
				System.Drawing.Color.White.ToVector3());

			Environment blackEnv = new EnvironmentSolid(Vector3.Zero);

			Environment skybox = new EnvironmentSkybox(
				skyRight,
				skyLeft,
				skyUp,
				skyDown,
				skyBack,
				skyFront);


			// === Materials ===
			Material crate = new DiffuseMaterial(Vector3.One, crateTexture, null, null, TextureAddressMode.Wrap, TextureFilter.Linear);
			Material tiles = new DiffuseMaterial(new Vector3(0.2f, 1.0f, 0.2f), checkerboardTexture);
			Material tilesRepeat = new DiffuseMaterial(Vector3.One, checkerboardTexture, null, new Vector2(10, 10));
			Material lava = new EmissiveMaterial(Vector3.One, 5.0f, lavaEmissiveTexture, Vector3.One, lavaAlbedoTexture);

			Material grayMatte = new DiffuseMaterial(System.Drawing.Color.LightGray.ToVector3());
			Material whiteMatte = new DiffuseMaterial(new Vector3(1, 1, 1));
			Material redMatte = new DiffuseMaterial(new Vector3(1, 0, 0));
			Material greenMatte = new DiffuseMaterial(new Vector3(0, 1, 0));
			Material blueMatteChecker = new DiffuseMaterial(new Vector3(0.2f, 0.2f, 1.0f), checkerboardTexture);

			Material metalTiles = new MetalMaterial(new Vector3(1.000f, 0.766f, 0.336f), 0, null, checkerboardTexture, null, new Vector2(3, 3));
			Material mirror = new MetalMaterial(new Vector3(1, 1, 1));
			Material gold = new MetalMaterial(new Vector3(1.000f, 0.766f, 0.336f));

			Material transparent = new TransparentMaterial(new Vector3(1, 1, 1), 1.5f);
			Material transparentRough = new TransparentMaterial(Vector3.One, 1.5f, 0.0f, null, tilesTextureNoGamma);

			Material normalMapTest = new MetalMaterial(Vector3.One, 0.0f, null, null, lavaNormalTexture);

			Material whiteLight = new EmissiveMaterial(new Vector3(1, 1, 1));
			Material redLight = new EmissiveMaterial(new Vector3(1, 0, 0));
			Material greenLight = new EmissiveMaterial(new Vector3(0, 1, 0));
			Material blueLight = new EmissiveMaterial(new Vector3(0, 0, 1));

			// === Meshes ===
			Mesh cubeMesh = new Mesh("Content/Models/cube.obj");
			Mesh helixMesh = new Mesh("Content/Models/helix.obj");
			Mesh sphereMesh = new Mesh("Content/Models/sphere.obj");
			Mesh quadMesh = new Mesh("Content/Models/quad.obj");


			// === DEFAULT SCENE with and without skybox ===
			{
				Entity ground = new Entity(quadMesh, grayMatte);
				ground.Transform.SetScale(100.0f);

				Entity left = new Entity(Sphere.Default, crate);
				left.Transform.SetPosition(-5, 2, 0);
				left.Transform.SetScale(2);

				Entity middle = new Entity(Sphere.Default, mirror);
				middle.Transform.SetPosition(0, 4, 0);
				middle.Transform.SetScale(2);

				Entity right = new Entity(Sphere.Default, metalTiles);
				right.Transform.SetPosition(5, 2, 0);
				right.Transform.SetScale(2);

				Entity above = new Entity(Sphere.Default, greenMatte);
				above.Transform.SetPosition(0, 9, 0);
				above.Transform.SetScale(2);

				Entity close = new Entity(Sphere.Default, transparent);
				close.Transform.SetPosition(0, 2, 5);
				close.Transform.SetScale(2);

				Scene scene = new Scene("Default", environmentLight, sceneBounds);
				scene.Add(ground);
				scene.Add(left);
				scene.Add(middle);
				scene.Add(right);
				scene.Add(above);
				scene.Add(close);

				// Copy the first scene's objects
				Scene sceneSkybox = new Scene("Default + Skybox", skybox, sceneBounds);
				for (int i = 0; i < scene.EntityCount; i++)
					sceneSkybox.Add(scene[i]);

				// Add both to the scene list
				scenes.Add(scene);
				scenes.Add(sceneSkybox);
			}

			// === SPECULAR TEST ===
			{
				Entity ground = new Entity(quadMesh, grayMatte);
				ground.Transform.SetScale(100.0f);

				// Set up the scene
				Scene scene = new Scene("Specular Test", skybox, sceneBounds);
				scene.Add(ground);

				// Create several entities with variable roughness
				int numSpheres = 5;
				float scale = 2.0f;
				for (int i = 0; i < numSpheres; i++)
				{
					float x = (i - numSpheres / 2.0f + 0.5f) * 1.5f * scale * 2;

					// Diffuse+Specular material
					DiffuseAndSpecularMaterial m = new DiffuseAndSpecularMaterial(new Vector3(0.25f, 0.25f, 1.0f), (float)i / (numSpheres - 1));
					Entity e = new Entity(Sphere.Default, m);
					e.Transform.SetPosition(x, scale, 0);
					e.Transform.SetScale(scale);
					scene.Add(e);

					// Metal material
					MetalMaterial met = new MetalMaterial(new Vector3(1.000f, 0.766f, 0.336f), (float)i / (numSpheres - 1));
					Entity eMet = new Entity(Sphere.Default, met);
					eMet.Transform.SetPosition(x, scale * 3 + 1, 0);
					eMet.Transform.SetScale(scale);
					scene.Add(eMet);
				}

				// Add to the scene list
				scenes.Add(scene);
			}

			// === MESH SCENE ===
			{
				// Entities ===
				Entity helix = new Entity(helixMesh, blueMatteChecker);
				helix.Transform.MoveAbsolute(0, 2.5f, 0);
				helix.Transform.ScaleRelative(5.0f);

				Entity sphere = new Entity(sphereMesh, gold);
				sphere.Transform.MoveAbsolute(0, 1, 0);
				sphere.Transform.ScaleRelative(2.0f);

				Entity ground = new Entity(quadMesh, grayMatte);
				ground.Transform.SetScale(100.0f);

				// Create scene
				Scene scene = new Scene("Mesh Test", environmentLight, sceneBounds);
				scene.Add(ground);
				scene.Add(helix);
				scene.Add(sphere);

				// Add to scene list
				scenes.Add(scene);
			}

			// === RANDOM SCENE ===
			{
				// Entities
				Entity ground = new Entity(quadMesh, grayMatte);
				ground.Transform.SetScale(100.0f);

				// Create scene
				Scene scene = new Scene("Random Spheres", environmentLight, sceneBounds);
				scene.Add(ground);

				// Add random entities
				Random rng = ThreadSafeRandom.Instance;
				for (int i = 0; i < 100; i++)
				{
					// Random scale (used for height, too)
					float scale = rng.NextFloat(0.1f, 1.0f);

					// Choose a random material
					Material m = null;
					int randMaterial = rng.Next(4);
					switch (randMaterial)
					{
						case 0: m = new DiffuseMaterial(rng.NextColor()); break;
						case 1: m = new MetalMaterial(rng.NextColor(), rng.NextFloat()); break;
						case 2: m = new TransparentMaterial(Vector3.One, 1.5f); break;
						case 3: m = new DiffuseAndSpecularMaterial(rng.NextColor(), rng.NextFloat()); break;
					}

					Entity s = new Entity(Sphere.Default, m);
					s.Transform.SetPosition(rng.NextFloat(-20, 20), scale, rng.NextFloat(-20, 20));
					s.Transform.SetScale(scale);
					scene.Add(s);
				}

				// Add to scene list
				scenes.Add(scene);
			}

			// === NORMAL MAP SCENE ===
			{
				Entity ground = new Entity(quadMesh, grayMatte);
				ground.Transform.SetScale(100.0f);

				Entity sphere = new Entity(Sphere.Default, normalMapTest);
				sphere.Transform.SetPosition(0, 4, 0);
				sphere.Transform.SetScale(2);

				Entity leftLight = new Entity(Sphere.Default, redLight);
				leftLight.Transform.SetPosition(-4, 4, 0);

				Entity upLight = new Entity(Sphere.Default, greenLight);
				upLight.Transform.SetPosition(0, 8, 0);

				Entity rightLight = new Entity(Sphere.Default, blueLight);
				rightLight.Transform.SetPosition(4, 4, 0);

				Scene scene = new Scene("Normal Map", blackEnv, sceneBounds);
				scene.Add(ground);
				scene.Add(sphere);
				scene.Add(leftLight);
				scene.Add(rightLight);
				scene.Add(upLight);

				// Add to scene list
				scenes.Add(scene);
			}

			// === EMISSIVE SCENE ===
			{
				Entity ground = new Entity(quadMesh, grayMatte);
				ground.Transform.SetScale(100.0f);

				Entity left = new Entity(Sphere.Default, greenMatte);
				left.Transform.SetPosition(-5, 2, 0);
				left.Transform.SetScale(2);

				Entity middle = new Entity(Sphere.Default, lava);
				middle.Transform.SetPosition(0, 4, 0);
				middle.Transform.SetScale(2);

				Entity right = new Entity(Sphere.Default, metalTiles);
				right.Transform.SetPosition(5, 2, 0);
				right.Transform.SetScale(2);

				Scene scene = new Scene("Emissive", blackEnv, sceneBounds);
				scene.Add(ground);
				scene.Add(left);
				scene.Add(middle);
				scene.Add(right);

				// Add to scene list
				scenes.Add(scene);
			}

			// === CORNELL BOX ===
			{
				Entity ground = new Entity(quadMesh, whiteMatte);
				Entity ceiling = new Entity(quadMesh, whiteMatte);
				Entity backWall = new Entity(quadMesh, whiteMatte);
				Entity leftWall = new Entity(quadMesh, greenMatte);
				Entity rightWall = new Entity(quadMesh, redMatte);

				// Scale the box
				float boxSize = 5;
				float halfBox = boxSize;// / 2.0f;
				float halfPi = MathF.PI / 2.0f;
				ground.Transform.SetScale(boxSize);
				ceiling.Transform.SetScale(boxSize);
				backWall.Transform.SetScale(boxSize);
				leftWall.Transform.SetScale(boxSize);
				rightWall.Transform.SetScale(boxSize);

				// Arrange the sides
				ground.Transform.MoveAbsolute(0, -halfBox, 0);
				ceiling.Transform.MoveAbsolute(0, halfBox, 0);

				backWall.Transform.MoveAbsolute(0, 0, -halfBox);
				backWall.Transform.Rotate(halfPi, 0, 0);

				leftWall.Transform.MoveAbsolute(-halfBox, 0, 0);
				leftWall.Transform.Rotate(0, 0, halfPi);

				rightWall.Transform.MoveAbsolute(halfBox, 0, 0);
				rightWall.Transform.Rotate(0, 0, halfPi);

				// Light
				Entity light = new Entity(quadMesh, whiteLight);
				light.Transform.MoveAbsolute(0, halfBox - 0.01f, 0);
				light.Transform.SetScale(3);

				// Objects inside box
				Entity obj1 = new Entity(cubeMesh, grayMatte);
				obj1.Transform.SetScale(3, 6, 3);
				obj1.Transform.MoveAbsolute(-2f, -3, -1);
				obj1.Transform.Rotate(0, 0.35f, 0);

				Entity obj2 = new Entity(cubeMesh, grayMatte);
				obj2.Transform.SetScale(2.5f, 4, 2.5f);
				obj2.Transform.MoveAbsolute(1.5f, -4, 1);
				obj2.Transform.Rotate(0, -MathF.PI / 4, 0);

				Scene scene = new Scene("Cornell Box", blackEnv, sceneBounds);
				scene.Add(ground);
				scene.Add(ceiling);
				scene.Add(backWall);
				scene.Add(leftWall);
				scene.Add(rightWall);
				scene.Add(light);
				scene.Add(obj1);
				scene.Add(obj2);

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
