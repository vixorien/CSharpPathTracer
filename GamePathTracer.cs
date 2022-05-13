using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ImGuiNET;
using SIMD = System.Numerics;

namespace CSharpPathTracer
{
	class GamePathTracer : Game
	{
		// Graphics stuff
		private GraphicsDeviceManager _graphics;
		private SpriteBatch _spriteBatch;
		private ImGuiHelper uiHelper;

		// Raytracing
		private Camera camera;
		private List<Scene> scenes;
		private RaytracerMonoGame raytracer;
		private Texture2D raytraceTexture;

		public GamePathTracer()
		{
			_graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
			IsMouseVisible = true;
		}

		protected override void Initialize()
		{
			// Set initial window size
			_graphics.PreferredBackBufferWidth = 1600;
			_graphics.PreferredBackBufferHeight = 900;
			_graphics.ApplyChanges();
			this.Window.AllowUserResizing = true;

			uiHelper = new ImGuiHelper(this);

			raytraceTexture = new Texture2D(
				GraphicsDevice,
				GraphicsDevice.PresentationParameters.BackBufferWidth,
				GraphicsDevice.PresentationParameters.BackBufferHeight,
				false,
				SurfaceFormat.Vector4);

			// Set up scene stuff
			raytracer = new RaytracerMonoGame();
			scenes = Scene.GenerateScenes();
			camera = new Camera(
				new SIMD.Vector3(0, 8, 20),
				(float)_graphics.PreferredBackBufferWidth / _graphics.PreferredBackBufferHeight,
				MathF.PI / 4.0f,
				0.01f,
				1000.0f,
				0.0f,
				20.0f);
			camera.Transform.Rotate(-0.25f, 0, 0);


			base.Initialize();
		}


		protected override void LoadContent()
		{
			_spriteBatch = new SpriteBatch(GraphicsDevice);

			// TODO: use this.Content to load your game content here
		}

		protected override void Update(GameTime gameTime)
		{
			if (Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
				Exit();

			uiHelper.PreUpdate(gameTime);

			ImGui.Text("Hello, world!");
			if (ImGui.Button("Raytrace"))
			{
				RaytracingParameters rtParams = new RaytracingParameters(
					scenes[0],
					camera,
					_graphics.PreferredBackBufferWidth,
					_graphics.PreferredBackBufferHeight,
					10,
					1,
					25,
					false);

				RaytracingResults results = raytracer.RaytraceScene(rtParams);

				// Copy data into texture
				raytraceTexture.SetData<SIMD.Vector4>(results.Pixels);
			}

			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);

			_spriteBatch.Begin();
			_spriteBatch.Draw(raytraceTexture, Vector2.Zero, Color.White);
			_spriteBatch.End();

			uiHelper.Draw();

			base.Draw(gameTime);
		}
	}
}
