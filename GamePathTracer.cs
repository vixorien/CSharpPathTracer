using System;
using System.IO;
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
		Full,
		FullProgressive
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
		private string[] sceneNames;
		private int currentSceneIndex;
		private Texture2D raytraceTexture;

		// Raytracing options
		private RaytracerMonoGame raytracer;
		private RaytracingOptions rtOptionsRealTime;
		private RaytracingOptions rtOptionsFull;

		private RaytracingModeMonoGame rtMode;
		private bool raytraceInProgress;
		private bool progressiveRaytrace;
		private double percentComplete;
		private ulong totalRaysFromLastRaytrace;
		private int deepestRecursionFromLastRaytrace;

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
			progressiveRaytrace = true;
			totalRaysFromLastRaytrace = 0;
			deepestRecursionFromLastRaytrace = 0;
			percentComplete = 0.0;

			// UI setup
			uiHelper = new ImGuiHelper(this);

			// Set up scene stuff
			raytracer = new RaytracerMonoGame();
			camera = new Camera(
				new SIMD.Vector3(0, 8, 20),
				(float)_graphics.PreferredBackBufferWidth / _graphics.PreferredBackBufferHeight,
				MathF.PI / 4.0f,
				0.01f,
				1000.0f,
				0.0f,
				20.0f);
			camera.Transform.Rotate(-0.25f, 0, 0);
			scenes = Scene.GenerateScenes();
			sceneNames = new string[scenes.Count];
			for (int i = 0; i < scenes.Count; i++) 
				sceneNames[i] = scenes[i].Name;
			currentSceneIndex = 0;

			// Perform a single raytrace
			raytraceInProgress = false;
			rtMode = RaytracingModeMonoGame.None;
			BeginRaytrace(RaytracingModeMonoGame.Realtime);

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

				case RaytracingModeMonoGame.FullProgressive:
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
			deepestRecursionFromLastRaytrace = 0;
			totalRaysFromLastRaytrace = 0;
			percentComplete = 0.0;

			// Set up params and start thread
			RaytracingParameters rtParams = new RaytracingParameters(
				scenes[currentSceneIndex],
				camera,
				windowWidth,
				windowHeight,
				options.SamplesPerPixel,
				options.ResolutionReduction,
				options.MaxRecursionDepth,
				mode == RaytracingModeMonoGame.FullProgressive);
			worker.RunWorkerAsync(rtParams);

			// Restart the stopwatch to track raytracing time
			stopwatch.Restart();
		}



		private void ScanlineComplete(object sender, ProgressChangedEventArgs e)
		{
			RaytracingProgressMonoGame progress = e.UserState as RaytracingProgressMonoGame;
			if (progress == null)
				return;

			// Save stats
			totalRaysFromLastRaytrace = progress.Stats.TotalRays;
			deepestRecursionFromLastRaytrace = progress.Stats.DeepestRecursion;
			percentComplete = progress.CompletionPercent;

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
			percentComplete = 1.0;
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

			// Only move camera is left button is down
			if (!io.WantCaptureMouse && ms.LeftButton == ButtonState.Pressed && this.IsActive)
			{
				// Is this a first click?
				if (prevMS.LeftButton == ButtonState.Released && worker != null && worker.IsBusy)
				{
					// Cancel pending raytrace
					worker.CancelAsync();
				}

				// Determine if the mouse has moved
				float xDiff = (prevMS.X - ms.X) * CameraRotationSpeed;
				float yDiff = (prevMS.Y - ms.Y) * CameraRotationSpeed;

				if (xDiff != 0.0f || yDiff != 0.0f)
				{
					camera.Transform.Rotate(yDiff, xDiff, 0);
					input = true;
				}

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
			}

			return input;
		}

		private void CreateUI()
		{
			ImGui.ShowDemoWindow();
			ImGui.Begin("Raytracing Details");
			{
				ImGui.Text("=== STATS ===");
				ImGui.Spacing();
				ImGui.Indent(10);
				ImGui.Text("FPS: " + ImGui.GetIO().Framerate);
				ImGui.Text("Trace Time: " + stopwatch.Elapsed);
				ImGui.Text("Total Rays: " + totalRaysFromLastRaytrace.ToString("N0"));
				ImGui.Text("Deepest Recursion: " + deepestRecursionFromLastRaytrace);
				ImGui.Text("Progress..." + percentComplete.ToString("P2"));
				ImGui.ProgressBar((float)percentComplete, new SIMD.Vector2(-1,0));
				ImGui.Indent(-10);


				ImGui.Spacing();
				ImGui.Separator();
				ImGui.Spacing();

				// Scene options
				ImGui.Text("=== SCENE & CAMERA OPTIONS ===");
				ImGui.Spacing();
				ImGui.Indent(10);

				if (raytraceInProgress)
				{
					ImGui.Text("Cancel raytrace to change scenes");
				}
				else if (ImGui.Combo("Scene", ref currentSceneIndex, sceneNames, sceneNames.Length))
				{
					if (raytraceInProgress)
					{
						worker.CancelAsync();
					}
					else
					{
						BeginRaytrace(RaytracingModeMonoGame.Realtime);
					}
				}

				ImGui.Spacing();

				// Local camera vars
				float fov = camera.FieldOfView;
				float ap = camera.Aperture;
				float focDist = camera.FocalDistance;

				bool camChange = false;
				if (ImGui.SliderFloat("Camera Field of View", ref fov, 0.0f, MathF.PI * 2)) { camera.FieldOfView = fov; camChange = true; }
				if (ImGui.SliderFloat("Camera Aperture", ref ap, 0.0f, 100.0f)) { camera.Aperture = ap; camChange = true; }
				if (ImGui.SliderFloat("Camera Focal Distance", ref focDist, 0.0f, 100.0f)) { camera.FocalDistance = focDist; camChange = true; }
				if (camChange) { BeginRaytrace(RaytracingModeMonoGame.Realtime); }

				ImGui.Indent(-10);


				ImGui.Spacing();
				ImGui.Separator();
				ImGui.Spacing();

				// Sliders for real time
				ImGui.Text("=== RAYTRACE OPTIONS ===");
				ImGui.Spacing();
				ImGui.Indent(10);
				ImGui.Text("Live (Real-Time) Options");
				{
					ImGui.Indent(10);
					ImGui.PushID("Live");
					ImGui.SliderInt("Samples Per Pixel", ref rtOptionsRealTime.SamplesPerPixel, 1, 1000);
					ImGui.SliderInt("Resolution Reduction", ref rtOptionsRealTime.ResolutionReduction, 1, 16);
					ImGui.SliderInt("Max Recursion Depth", ref rtOptionsRealTime.MaxRecursionDepth, 1, 100);
					ImGui.PopID();
					ImGui.Indent(-10);
				}

				// Sliders for full
				ImGui.Spacing();
				ImGui.Text("Full (Slow) Options");
				{
					ImGui.Indent(10);
					ImGui.PushID("Full");
					ImGui.SliderInt("Samples Per Pixel", ref rtOptionsFull.SamplesPerPixel, 1, 1000);
					ImGui.SliderInt("Resolution Reduction", ref rtOptionsFull.ResolutionReduction, 1, 16);
					ImGui.SliderInt("Max Recursion Depth", ref rtOptionsFull.MaxRecursionDepth, 1, 100);
					ImGui.PopID();
					ImGui.Indent(-10);
				}

				ImGui.Indent(-10);

				// Starting the raytrace
				ImGui.Spacing();
				ImGui.Checkbox("Progressive Raytrace", ref progressiveRaytrace);
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
					BeginRaytrace(progressiveRaytrace ? RaytracingModeMonoGame.FullProgressive : RaytracingModeMonoGame.Full);
				}

				if (ImGui.Button("Save Image"))
				{
					// Need different texture format ughhhh
					//using (FileStream fs = File.OpenWrite("output.png"))
					//	raytraceTexture.SaveAsPng(fs, windowWidth, windowHeight);
				}

			}
			ImGui.End();
		}
	}
}
