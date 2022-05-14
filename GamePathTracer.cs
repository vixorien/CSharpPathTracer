using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ImGuiNET;
using SIMD = System.Numerics;

namespace CSharpPathTracer
{
	enum RaytracingModeMonoGame
	{
		None,
		Full,
		ProgressiveAdditive,
		ProgressiveScanline
	}

	struct RaytracingOptions
	{
		public int SamplesPerPixel;
		public int ResolutionReduction;
		public int MaxRecursionDepth;

		public RaytracingOptions(int samplesPerPixel, int resolutionReduction, int maxRecursionDepth)
		{
			SamplesPerPixel = samplesPerPixel;
			ResolutionReduction = resolutionReduction;
			MaxRecursionDepth = maxRecursionDepth;
		}
	}

	class GamePathTracer : Game
	{
		// Input
		private MouseState prevMS;

		// Graphics stuff
		private GraphicsDeviceManager _graphics;
		private SpriteBatch _spriteBatch;
		private ImGuiHelper uiHelper;
		private int windowWidth;
		private int windowHeight;

		// Raytracing elements
		private Camera camera;
		private List<Scene> scenes;
		private RaytracerMonoGame raytracer;
		private Texture2D raytraceTexture;

		// Raytracing options
		private RaytracingOptions rtOptionsRealTime;
		private RaytracingOptions rtOptionsFull;

		private RaytracingModeMonoGame rtMode;

		public GamePathTracer()
		{
			_graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
			IsMouseVisible = true;
			Window.ClientSizeChanged += Window_ClientSizeChanged;
		}

		private void Window_ClientSizeChanged(object sender, EventArgs e)
		{
			windowWidth = Window.ClientBounds.Width;
			windowHeight = Window.ClientBounds.Height;
		}

		protected override void Initialize()
		{
			// General MonoGame setup
			_spriteBatch = new SpriteBatch(GraphicsDevice);
			this.Window.AllowUserResizing = true;

			// Set initial window size
			windowWidth = 1600;
			windowHeight = 900;
			_graphics.PreferredBackBufferWidth = windowWidth;
			_graphics.PreferredBackBufferHeight = windowHeight;
			_graphics.ApplyChanges();

			// Set up raytracing options
			ResizeRaytracingTexture(windowWidth, windowHeight);
			rtOptionsRealTime = new RaytracingOptions(1, 16, 10);
			rtOptionsFull = new RaytracingOptions(10, 1, 50);

			// UI setup
			uiHelper = new ImGuiHelper(this);

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

			// Perform a single raytrace
			rtMode = RaytracingModeMonoGame.None;
			Raytrace(rtOptionsRealTime);

			base.Initialize();
		}

		protected override void Update(GameTime gameTime)
		{
			if (Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
				Exit();

			// Handle the UI
			uiHelper.PreUpdate(gameTime);
			CreateUI();

			// Camera movement causes a raytrace
			if(UpdateCamera())
			{
				Raytrace(rtOptionsRealTime);
			}

			prevMS = Mouse.GetState();
			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);

			// Draw the raytracing results
			_spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp);
			_spriteBatch.Draw(raytraceTexture, new Rectangle(0, 0, windowWidth, windowHeight), Color.White);
			_spriteBatch.End();

			// UI on top
			uiHelper.Draw();

			base.Draw(gameTime);
		}

		private void Raytrace(RaytracingOptions rtOptions)
		{
			// Resize if necessary
			ResizeRaytracingTexture(windowWidth / rtOptions.ResolutionReduction, windowHeight / rtOptions.ResolutionReduction);

			RaytracingParameters rtParams = new RaytracingParameters(
				scenes[0],
				camera,
				windowWidth,
				windowHeight,
				rtOptions.SamplesPerPixel,
				rtOptions.ResolutionReduction,
				rtOptions.MaxRecursionDepth,
				false);

			RaytracingResults results = raytracer.RaytraceScene(rtParams);

			// Copy data into texture
			raytraceTexture.SetData<SIMD.Vector4>(results.Pixels);
		}

		private void ResizeRaytracingTexture(int width, int height)
		{
			// Already the right size?
			if (raytraceTexture!= null &&
				raytraceTexture.Width == width &&
				raytraceTexture.Height == height)
				return;

			// Dispose if necessary
			if (raytraceTexture != null)
				raytraceTexture.Dispose();

			raytraceTexture = new Texture2D(
				GraphicsDevice,
				width,
				height,
				false,
				SurfaceFormat.Vector4);
		}

		private bool UpdateCamera()
		{
			// Camera movement constantns
			const float CameraMoveSpeed = 0.25f;
			const float CameraMoveSpeedSlow = 0.05f;
			const float CameraMoveSpeedFast = 1.0f;
			const float CameraRotationSpeed = 0.01f;

			// Grab input
			KeyboardState kb = Keyboard.GetState();
			MouseState ms = Mouse.GetState();
			ImGuiIOPtr io = ImGui.GetIO();

			// Track input change
			bool input = false;

			// Handle movement
			if (!io.WantCaptureKeyboard)
			{
				float speed = CameraMoveSpeed;
				if (kb.IsKeyDown(Keys.LeftShift)) speed = CameraMoveSpeedFast;
				if (kb.IsKeyDown(Keys.LeftControl)) speed = CameraMoveSpeedSlow;

				if (kb.IsKeyDown(Keys.W)) { camera.Transform.MoveRelative(0, 0, -speed); input = true; }
				if (kb.IsKeyDown(Keys.S)) { camera.Transform.MoveRelative(0, 0, speed); input = true; }
				if (kb.IsKeyDown(Keys.A)) { camera.Transform.MoveRelative(-speed, 0, 0); input = true; }
				if (kb.IsKeyDown(Keys.D)) { camera.Transform.MoveRelative(speed, 0, 0); input = true; }
				if (kb.IsKeyDown(Keys.Space)) { camera.Transform.MoveAbsolute(0, speed, 0); input = true; }
				if (kb.IsKeyDown(Keys.X)) { camera.Transform.MoveAbsolute(0, -speed, 0); input = true; }
			}

			// Handle rotation
			if (!io.WantCaptureMouse && ms.LeftButton == ButtonState.Pressed)
			{
				float xDiff = (prevMS.X - ms.X) * CameraRotationSpeed;
				float yDiff = (prevMS.Y - ms.Y) * CameraRotationSpeed;

				if (xDiff != 0.0f || yDiff != 0.0f)
				{
					camera.Transform.Rotate(yDiff, xDiff, 0);
					input = true;
				}
			}

			return input;
		}

		private void CreateUI()
		{
			//ImGui.ShowDemoWindow();
			ImGui.Begin("Raytracing Options");
			{
				ImGui.Text("FPS: " + ImGui.GetIO().Framerate);

				// Sliders for real time
				if(ImGui.TreeNode("Live (Real-Time)"))
				{
					ImGui.SliderInt("Samples Per Pixel", ref rtOptionsRealTime.SamplesPerPixel, 1, 1000);
					ImGui.SliderInt("Resolution Reduction", ref rtOptionsRealTime.ResolutionReduction, 1, 16);
					ImGui.SliderInt("Max Recursion Depth", ref rtOptionsRealTime.MaxRecursionDepth, 1, 100);
					ImGui.TreePop();
				}

				// Sliders for real time
				if(ImGui.TreeNode("Full (Slow)"))
				{
					ImGui.SliderInt("Samples Per Pixel", ref rtOptionsFull.SamplesPerPixel, 1, 1000);
					ImGui.SliderInt("Resolution Reduction", ref rtOptionsFull.ResolutionReduction, 1, 16);
					ImGui.SliderInt("Max Recursion Depth", ref rtOptionsFull.MaxRecursionDepth, 1, 100);
					ImGui.TreePop();
				}

				if (ImGui.Button("Full Raytrace"))
				{
					Raytrace(rtOptionsFull);
				}

			}
			ImGui.End();
		}
	}
}
