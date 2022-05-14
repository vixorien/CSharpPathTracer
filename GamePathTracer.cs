using System;
using System.Diagnostics;
using System.ComponentModel;
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
		Realtime,
		Full
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

		// Resources
		private Camera camera;
		private List<Scene> scenes;
		private Texture2D raytraceTexture;

		// Raytracing options
		private RaytracerMonoGame raytracer;
		private RaytracingOptions rtOptionsRealTime;
		private RaytracingOptions rtOptionsFull;

		private RaytracingModeMonoGame rtMode;
		private bool raytraceInProgress;
		private int currentProgressiveScanline;

		private BackgroundWorker worker;
		private Stopwatch stopwatch;

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
			stopwatch = new Stopwatch();
			//this.Window.AllowUserResizing = true;

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
			//Raytrace(rtOptionsRealTime);
			raytraceInProgress = false;
			currentProgressiveScanline = 0;
			rtMode = RaytracingModeMonoGame.None;

			base.Initialize();
		}

		protected override void Update(GameTime gameTime)
		{
			if (Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
				Exit();

			// Handle the UI
			uiHelper.PreUpdate(gameTime);
			CreateUI();

			bool input = UpdateCamera(gameTime);
			if (input)
			{
				// Input means we do a real-time render
				rtMode = RaytracingModeMonoGame.Realtime;
			}
			else if(rtMode == RaytracingModeMonoGame.Realtime)
			{
				// No input means we can stop a realtime render
				rtMode = RaytracingModeMonoGame.None;
			}

			// If we're in realtime mode and the worker is not currently busy (or doesn't exist yet),
			// then we can go ahead and begin a new low-res raytracing frame
			if (rtMode == RaytracingModeMonoGame.Realtime &&
				(worker == null || (worker != null && !worker.IsBusy)))
			{
				BeginRaytrace(RaytracingModeMonoGame.Realtime);
			}

			// Save old state
			prevMS = Mouse.GetState();
			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);

			// Draw the raytracing results
			_spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp);
			_spriteBatch.Draw(raytraceTexture, new Rectangle(0, 0, windowWidth, windowHeight), Color.White);
			_spriteBatch.End();

			// UI on top
			uiHelper.Draw();

			base.Draw(gameTime);
		}

		//private void Raytrace(RaytracingOptions rtOptions)
		//{
		//	int scaledWidth = windowWidth / rtOptions.ResolutionReduction;
		//	int scaledHeight = windowHeight / rtOptions.ResolutionReduction;

		//	// Resize if necessary
		//	ResizeRaytracingTexture(scaledWidth, scaledHeight);

		//	RaytracingParameters rtParams = new RaytracingParameters(
		//		scenes[0],
		//		camera,
		//		windowWidth,
		//		windowHeight,
		//		rtOptions.SamplesPerPixel,
		//		rtOptions.ResolutionReduction,
		//		rtOptions.MaxRecursionDepth,
		//		false);

		//	// Which mode?
		//	switch (rtMode)
		//	{

		//		case RaytracingModeMonoGame.Full:
		//			{
		//				// Perform the full raytrace and copy results into texture
		//				RaytracingResults results = raytracer.RaytraceScene(rtParams);
		//				raytraceTexture.SetData<SIMD.Vector4>(results.Pixels);
		//				rtMode = RaytracingModeMonoGame.None;
		//			}
		//			break;

		//		case RaytracingModeMonoGame.ProgressiveAdditive:
		//			break;

		//		case RaytracingModeMonoGame.ProgressiveScanline:
		//			{
		//				// Perform a single scanline
		//				RaytracingResults results = raytracer.RaytraceScanline(rtParams, currentProgressiveScanline);
		//				raytraceTexture.SetData<SIMD.Vector4>(
		//					0,
		//					new Rectangle(0, currentProgressiveScanline, scaledWidth, 1),
		//					results.Pixels,
		//					0,//currentProgressiveScanline * scaledWidth,
		//					scaledWidth);

		//				// Move to the next line and end if necessary
		//				currentProgressiveScanline++;
		//				if (currentProgressiveScanline >= scaledHeight)
		//					rtMode = RaytracingModeMonoGame.None;
		//			}
		//			break;
		//	}


		//}

		private void BeginRaytrace(RaytracingModeMonoGame mode)
		{
			// Check the mode for the correct options
			// or an early out
			RaytracingOptions options;
			switch (mode)
			{
				case RaytracingModeMonoGame.Realtime:
					options = rtOptionsRealTime;
					break;

				case RaytracingModeMonoGame.Full:
					options = rtOptionsFull;
					break;

				default: // Not a valid mode
					return;
			}

			// Set up the worker for threading
			worker?.Dispose();
			worker = new BackgroundWorker();
			worker.WorkerReportsProgress = true;
			worker.WorkerSupportsCancellation = true;
			worker.DoWork += raytracer.RaytraceSceneBackgroundWorker;
			worker.ProgressChanged += ScanlineComplete;
			worker.RunWorkerCompleted += RaytraceComplete;

			// Update the texture if necessary
			int scaledWidth = windowWidth / options.ResolutionReduction;
			int scaledHeight = windowHeight / options.ResolutionReduction;
			ResizeRaytracingTexture(scaledWidth, scaledHeight);

			// Track progress
			raytraceInProgress = true;
			rtMode = mode;

			// Set up params and start thread
			RaytracingParameters rtParams = new RaytracingParameters(
				scenes[0],
				camera,
				windowWidth,
				windowHeight,
				options.SamplesPerPixel,
				options.ResolutionReduction,
				options.MaxRecursionDepth,
				false);
			worker.RunWorkerAsync(rtParams);

			// Restart the stopwatch to track raytracing time
			stopwatch.Restart();
		}



		private void ScanlineComplete(object sender, ProgressChangedEventArgs e)
		{
			RaytracingProgressMonoGame progress = e.UserState as RaytracingProgressMonoGame;
			if (progress == null)
				return;

			// We usually want to copy one extra line to simulate the black
			// progress line across the final image (unless doing realtime)
			int copyHeight = 3;
			if (progress.ScanlineIndex >= raytraceTexture.Height - 2 || rtMode == RaytracingModeMonoGame.Realtime)
				copyHeight = 1;

			// Copy the given scanline into the final image
			raytraceTexture.SetData<SIMD.Vector4>(
				0,
				new Rectangle(0, progress.ScanlineIndex, progress.ScanlineWidth, copyHeight),
				progress.Pixels,
				progress.ScanlineIndex * progress.ScanlineWidth,
				progress.ScanlineWidth * copyHeight);
		}

		private void RaytraceComplete(object sender, RunWorkerCompletedEventArgs e)
		{
			stopwatch.Stop();
			raytraceInProgress = false;
			if (rtMode == RaytracingModeMonoGame.Full)
			{
				rtMode = RaytracingModeMonoGame.None;
			}
		}

		private void ResizeRaytracingTexture(int width, int height)
		{
			// Already the right size?
			if (raytraceTexture != null &&
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

		private bool UpdateCamera(GameTime gt)
		{
			// Camera movement constants
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
			if (!io.WantCaptureKeyboard && this.IsActive)
			{
				float speed = CameraMoveSpeed;
				if (kb.IsKeyDown(Keys.LeftShift)) speed = CameraMoveSpeedFast;
				if (kb.IsKeyDown(Keys.LeftControl)) speed = CameraMoveSpeedSlow;
				//speed *= (float)gt.ElapsedGameTime.TotalSeconds;

				if (kb.IsKeyDown(Keys.W)) { camera.Transform.MoveRelative(0, 0, -speed); input = true; }
				if (kb.IsKeyDown(Keys.S)) { camera.Transform.MoveRelative(0, 0, speed); input = true; }
				if (kb.IsKeyDown(Keys.A)) { camera.Transform.MoveRelative(-speed, 0, 0); input = true; }
				if (kb.IsKeyDown(Keys.D)) { camera.Transform.MoveRelative(speed, 0, 0); input = true; }
				if (kb.IsKeyDown(Keys.Space)) { camera.Transform.MoveAbsolute(0, speed, 0); input = true; }
				if (kb.IsKeyDown(Keys.X)) { camera.Transform.MoveAbsolute(0, -speed, 0); input = true; }
			}

			// Handle rotation
			if (!io.WantCaptureMouse && ms.LeftButton == ButtonState.Pressed && this.IsActive)
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
				if (ImGui.TreeNode("Live (Real-Time) Options"))
				{
					ImGui.SliderInt("Samples Per Pixel", ref rtOptionsRealTime.SamplesPerPixel, 1, 1000);
					ImGui.SliderInt("Resolution Reduction", ref rtOptionsRealTime.ResolutionReduction, 1, 16);
					ImGui.SliderInt("Max Recursion Depth", ref rtOptionsRealTime.MaxRecursionDepth, 1, 100);
					ImGui.TreePop();
				}

				// Sliders for real time
				if (ImGui.TreeNode("Full (Slow) Options"))
				{
					ImGui.SliderInt("Samples Per Pixel", ref rtOptionsFull.SamplesPerPixel, 1, 1000);
					ImGui.SliderInt("Resolution Reduction", ref rtOptionsFull.ResolutionReduction, 1, 16);
					ImGui.SliderInt("Max Recursion Depth", ref rtOptionsFull.MaxRecursionDepth, 1, 100);
					ImGui.TreePop();
				}

				ImGui.Spacing();
				//if (ImGui.Button("Raytrace Progressive (Additive)")) { rtMode = RaytracingModeMonoGame.ProgressiveAdditive; }
				//if (ImGui.Button("Raytrace Progressive (Scanline/Frame)")) { rtMode = RaytracingModeMonoGame.ProgressiveScanline; currentProgressiveScanline = 0; }
				//if (ImGui.Button("Raytrace Full (All at once)")) { rtMode = RaytracingModeMonoGame.Full; }
				if (ImGui.Button(raytraceInProgress ? "Cancel Raytrace" : "Start Full Raytrace"))
				{
					// If we're in progress, we're canceling the exisitng
					// work instead, which will be caught elsewhere
					if (raytraceInProgress)
					{
						worker.CancelAsync();
						return;
					}

					// Perform raytrace
					BeginRaytrace(RaytracingModeMonoGame.Full);
				}
			}
			ImGui.End();
		}
	}
}
