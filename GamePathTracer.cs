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
	enum RaytracingState
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
		private Stopwatch stopwatch;
		private Camera camera;
		private List<Scene> scenes;
		private string[] sceneNames;
		private int currentSceneIndex;
		private Texture2D raytraceTexture;
		private RenderTarget2D finalRenderTarget;
		private SIMD.Vector4[] raytraceProgressLine;

		// Raytracing
		private BackgroundWorker worker;
		private RaytracerMonoGame raytracer;
		private RaytracingOptions rtOptionsRealTime;
		private RaytracingOptions rtOptionsFull;

		private RaytracingState rtState;
		private bool rtStateTransitionPeriod;
		private bool raytraceInProgress;
		private bool progressiveRaytrace;
		private bool showProgressLine;
		private bool inputDuringFrame;
		private double percentComplete;
		private ulong totalRaysFromLastRaytrace;
		private int deepestRecursionFromLastRaytrace;

		/// <summary>
		/// The main MonoGame class for the path tracer
		/// </summary>
		public GamePathTracer()
		{
			_graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
			IsMouseVisible = true;
			Window.ClientSizeChanged += Window_ClientSizeChanged;
		}

		/// <summary>
		/// Handles the window resizing and resizes the final render target
		/// </summary>
		private void Window_ClientSizeChanged(object sender, EventArgs e)
		{
			windowWidth = Window.ClientBounds.Width;
			windowHeight = Window.ClientBounds.Height;

			// Resize the final render target
			finalRenderTarget?.Dispose();
			finalRenderTarget = new RenderTarget2D(GraphicsDevice, windowWidth, windowHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
		}

		/// <summary>
		/// Initializes the path tracer
		/// </summary>
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
			finalRenderTarget = new RenderTarget2D(
				GraphicsDevice,
				windowWidth,
				windowHeight,
				false,
				SurfaceFormat.Color,
				DepthFormat.None,
				0,
				RenderTargetUsage.PreserveContents);

			// Set up raytracing options
			ResizeRaytracingTexture(windowWidth, windowHeight);
			rtOptionsRealTime = new RaytracingOptions(1, 16, 10);
			rtOptionsFull = new RaytracingOptions(10, 1, 50);
			progressiveRaytrace = false;
			showProgressLine = true;
			totalRaysFromLastRaytrace = 0;
			deepestRecursionFromLastRaytrace = 0;
			percentComplete = 0.0;
			worker = new BackgroundWorker(); // So it's never null

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
			rtStateTransitionPeriod = false;
			rtState = RaytracingState.None;
			BeginRaytrace(RaytracingState.Realtime);

			base.Initialize();
		}


		/// <summary>
		/// Resizes the raytracing texture if necessary
		/// </summary>
		/// <param name="width">The new width</param>
		/// <param name="height">The new height</param>
		private void ResizeRaytracingTexture(int width, int height)
		{
			// Already the right size?
			if (raytraceTexture != null &&
				raytraceTexture.Width == width &&
				raytraceTexture.Height == height)
				return;

			// Dispose if necessary then recreate
			raytraceTexture?.Dispose();
			raytraceTexture = new Texture2D(
				GraphicsDevice,
				width,
				height,
				false,
				SurfaceFormat.Vector4);

			// Set up the progress line, which is a single black line of pixels
			raytraceProgressLine = new SIMD.Vector4[width];
			Array.Fill<SIMD.Vector4>(raytraceProgressLine, new SIMD.Vector4(0, 0, 0, 1));
		}


		/// <summary>
		/// Updates the UI, camera and overall raytracing state machine
		/// </summary>
		/// <param name="gameTime"></param>
		protected override void Update(GameTime gameTime)
		{
			if (Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
				Exit();

			// Reset input tracking
			inputDuringFrame = false;

			// Handle the UI (may cause input)
			uiHelper.PreUpdate(gameTime);
			CreateUI();

			// Scene updates
			bool camInput = UpdateCamera(gameTime);
			inputDuringFrame = inputDuringFrame || camInput;

			// States
			switch (rtState)
			{
				case RaytracingState.None: // Not currently raytracing

					// Was there input?  If so, start a realtime
					if (inputDuringFrame)
					{
						rtState = RaytracingState.Realtime;
						rtStateTransitionPeriod = true;
						worker?.CancelAsync();
					}
					else if (!raytraceInProgress)
					{
						rtStateTransitionPeriod = false;
					}

					break;

				case RaytracingState.Realtime:

					// Lack of input should send us to progressive
					if (!inputDuringFrame)
					{
						rtState = RaytracingState.FullProgressive;
						rtStateTransitionPeriod = true;
						worker?.CancelAsync();
					}
					else if (!raytraceInProgress) // Otherwise, keep going ASAP!
					{
						BeginRaytrace(RaytracingState.Realtime);
					}

					break;

				case RaytracingState.Full:

					// Was there input?  If so, start a realtime
					if (inputDuringFrame)
					{
						rtState = RaytracingState.Realtime;
						rtStateTransitionPeriod = true;
						worker?.CancelAsync();
					}
					else if (rtStateTransitionPeriod && !raytraceInProgress) // Waiting to start a full raytrace?
					{
						BeginRaytrace(RaytracingState.Full);
						rtStateTransitionPeriod = false;
					}
					else if (!rtStateTransitionPeriod)
					{
						rtState = RaytracingState.None;
					}

					break;

				case RaytracingState.FullProgressive:

					// Was there input?  If so, start a realtime
					if (inputDuringFrame)
					{
						rtState = RaytracingState.Realtime;
						rtStateTransitionPeriod = true;
						worker?.CancelAsync();
					}
					else if (rtStateTransitionPeriod && !raytraceInProgress) // Waiting to start a full progressive raytrace?
					{
						BeginRaytrace(RaytracingState.FullProgressive);
						rtStateTransitionPeriod = false;
					}
					else if (!rtStateTransitionPeriod)
					{
						rtState = RaytracingState.None;
					}

					break;
			}

			// Save old state
			prevMS = Mouse.GetState();
			base.Update(gameTime);
		}

		/// <summary>
		/// Updates the camera based on input and returns
		/// whether or not input was detected
		/// </summary>
		/// <param name="gt">Game time info</param>
		/// <returns>True if there was camera-related input, false otherwise</returns>
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
				// Definitely counts as input
				input = true;

				// Determine if the mouse has moved
				float xDiff = (prevMS.X - ms.X) * CameraRotationSpeed;
				float yDiff = (prevMS.Y - ms.Y) * CameraRotationSpeed;

				if (xDiff != 0.0f || yDiff != 0.0f)
				{
					camera.Transform.Rotate(yDiff, xDiff, 0);
				}

				// Handle movement
				if (!io.WantCaptureKeyboard && this.IsActive)
				{
					float speed = CameraMoveSpeed;
					if (kb.IsKeyDown(Keys.LeftShift)) speed = CameraMoveSpeedFast;
					if (kb.IsKeyDown(Keys.LeftControl)) speed = CameraMoveSpeedSlow;
					//speed *= (float)gt.ElapsedGameTime.TotalSeconds;

					if (kb.IsKeyDown(Keys.W)) { camera.Transform.MoveRelative(0, 0, -speed); }
					if (kb.IsKeyDown(Keys.S)) { camera.Transform.MoveRelative(0, 0, speed); }
					if (kb.IsKeyDown(Keys.A)) { camera.Transform.MoveRelative(-speed, 0, 0); }
					if (kb.IsKeyDown(Keys.D)) { camera.Transform.MoveRelative(speed, 0, 0); }
					if (kb.IsKeyDown(Keys.Space)) { camera.Transform.MoveAbsolute(0, speed, 0); }
					if (kb.IsKeyDown(Keys.X)) { camera.Transform.MoveAbsolute(0, -speed, 0); }
				}
			}

			return input;
		}

		/// <summary>
		/// Draws the raytracing results to a render target and then to the screen
		/// </summary>
		/// <param name="gameTime">Time info</param>
		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);

			// Draw the raytracing results to the full size render target
			GraphicsDevice.SetRenderTarget(finalRenderTarget);
			_spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
			_spriteBatch.Draw(raytraceTexture, new Rectangle(0, 0, windowWidth, windowHeight), Color.White);
			_spriteBatch.End();

			// Draw the final render target to the back buffer
			GraphicsDevice.SetRenderTarget(null);
			_spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp);
			_spriteBatch.Draw(finalRenderTarget, new Rectangle(0, 0, windowWidth, windowHeight), Color.White);
			_spriteBatch.End();

			// UI on top
			uiHelper.Draw();

			base.Draw(gameTime);
		}


		/// <summary>
		/// Actually causes a raytrace to begin using the given state
		/// </summary>
		/// <param name="state">The new raytracing state</param>
		private void BeginRaytrace(RaytracingState state)
		{
			// Valid mode?
			if (state == RaytracingState.None)
				return;

			// Check the mode for the correct options
			RaytracingOptions options =
				state == RaytracingState.Realtime
					? rtOptionsRealTime
					: rtOptionsFull;

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
			rtState = state;
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
				state == RaytracingState.FullProgressive);
			worker.RunWorkerAsync(rtParams);

			// Restart the stopwatch to track raytracing time
			stopwatch.Restart();
		}


		/// <summary>
		/// Callback for a single scanline (horizontal line of pixels) being
		/// completed by the raytracing background worker
		/// </summary>
		private void ScanlineComplete(object sender, ProgressChangedEventArgs e)
		{
			// Verify the progress parameter exists
			RaytracingProgressMonoGame progress = e.UserState as RaytracingProgressMonoGame;
			if (progress == null)
				return;

			// Save stats
			totalRaysFromLastRaytrace = progress.Stats.TotalRays;
			deepestRecursionFromLastRaytrace = progress.Stats.DeepestRecursion;
			percentComplete = progress.CompletionPercent;

			// Copy the given scanline into the final image
			raytraceTexture.SetData<SIMD.Vector4>(
				0,
				new Rectangle(0, progress.ScanlineIndex, progress.ScanlineWidth, 1),
				progress.Pixels,
				progress.ScanlineIndex * progress.ScanlineWidth,
				progress.ScanlineWidth);

			// Need to copy the black progress line?
			if (showProgressLine &&
				progress.ScanlineIndex < raytraceTexture.Height - 1 &&
				rtState != RaytracingState.Realtime)
			{
				// TODO: This could probably be handled directly in draw
				// now by stretching a 1x1 black pixel where necessary...
				raytraceTexture.SetData<SIMD.Vector4>(
					0,
					new Rectangle(0, progress.ScanlineIndex + 1, progress.ScanlineWidth, 1),
					raytraceProgressLine,
					0,
					progress.ScanlineWidth);
			}
		}


		/// <summary>
		/// Callback for raytracing ending, either due to the worker finishing
		/// or a cancellation of work during the raytrace
		/// </summary>
		private void RaytraceComplete(object sender, RunWorkerCompletedEventArgs e)
		{
			stopwatch.Stop();
			raytraceInProgress = false;

			// If this wasn't cancelled the ensure
			// the percentage is exactly 100%
			if (!e.Cancelled)
			{
				percentComplete = 1.0;
			}
		}

		
		
		/// <summary>
		/// Creates the ImGui interface
		/// </summary>
		private void CreateUI()
		{
			ImGui.ShowDemoWindow();
			ImGui.Begin("Raytracing Details");
			{
				ImGui.Text("=== STATS ===");
				ImGui.Spacing();
				ImGui.Indent(10);
				ImGui.Text("FPS: " + ImGui.GetIO().Framerate);

				// Don't print stats during realtime mode
				if (rtState == RaytracingState.Realtime)
				{
					ImGui.Text("Trace Time: ");
					ImGui.Text("Total Rays: ");
					ImGui.Text("Deepest Recursion: ");
					ImGui.Text("Progress...");
					ImGui.ProgressBar(0.0f, new SIMD.Vector2(-1, 0), "Realtime Mode");
				}
				else
				{
					ImGui.Text("Trace Time: " + stopwatch.Elapsed);
					ImGui.Text("Total Rays: " + totalRaysFromLastRaytrace.ToString("N0"));
					ImGui.Text("Deepest Recursion: " + deepestRecursionFromLastRaytrace);
					ImGui.Text("Progress..." + percentComplete.ToString("P2") + "%");
					ImGui.ProgressBar((float)percentComplete, new SIMD.Vector2(-1, 0));
				}
				ImGui.Indent(-10);


				ImGui.Spacing();
				ImGui.Separator();
				ImGui.Spacing();

				// Scene options
				ImGui.Text("=== SCENE & CAMERA OPTIONS ===");
				ImGui.Spacing();
				ImGui.Indent(10);

				// Scene combo box
				if (ImGui.Combo("Current Scene", ref currentSceneIndex, sceneNames, sceneNames.Length))
				{
					worker.CancelAsync();
					rtState = RaytracingState.FullProgressive;
					rtStateTransitionPeriod = true;
				}

				if (ImGui.TreeNode("Scene Details"))
				{
					// Work in progress!
					for (int i = 0; i < scenes[currentSceneIndex].EntityCount; i++)
					{
						Entity e = scenes[currentSceneIndex][i];
						ImGui.Text(e.Geometry.GetType().ToString());
					}

					ImGui.TreePop();
				}


				if (ImGui.TreeNode("Camera Details"))
				{
					// Local camera vars
					float fov = camera.FieldOfView;
					float ap = camera.Aperture;
					float focDist = camera.FocalDistance;

					if (ImGui.SliderFloat("Camera Field of View", ref fov, 0.0f, MathF.PI * 2)) { camera.FieldOfView = fov; }
					if (ImGui.IsItemActive()) inputDuringFrame = true;
					if (ImGui.SliderFloat("Camera Aperture", ref ap, 0.0f, 100.0f)) { camera.Aperture = ap; }
					if (ImGui.IsItemActive()) inputDuringFrame = true;
					if (ImGui.SliderFloat("Camera Focal Distance", ref focDist, 0.0f, 100.0f)) { camera.FocalDistance = focDist; }
					if (ImGui.IsItemActive()) inputDuringFrame = true;

					ImGui.TreePop();
				}

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
				ImGui.Checkbox("Show Progress Line", ref showProgressLine);
				ImGui.Checkbox("Progressive Raytrace", ref progressiveRaytrace);
				if (ImGui.Button(raytraceInProgress ? "Cancel Raytrace" : "Start Full Raytrace"))
				{
					// If we're in progress, we're canceling the exisitng
					// work instead, which will be caught elsewhere
					if (raytraceInProgress)
					{
						worker.CancelAsync();
						rtState = RaytracingState.None;
						rtStateTransitionPeriod = true;
						return;
					}

					// Perform raytrace
					BeginRaytrace(progressiveRaytrace ? RaytracingState.FullProgressive : RaytracingState.Full);
				}

				// Handle "Save" button
				if (raytraceInProgress ? ButtonDisabled("Save Image") : ImGui.Button("Save Image"))
				{
					System.Windows.Forms.SaveFileDialog dialog = new System.Windows.Forms.SaveFileDialog();
					dialog.Title = "Save Image As...";
					dialog.Filter = "PNG (*.png)|*.png";
					System.Windows.Forms.DialogResult result = dialog.ShowDialog();

					if (result == System.Windows.Forms.DialogResult.OK)
					{
						using (FileStream fs = File.OpenWrite(dialog.FileName))
							finalRenderTarget.SaveAsPng(fs, windowWidth, windowHeight);
					}
				}

			}
			ImGui.End();
		}

		/// <summary>
		/// Helper for showing a "disabled" button that can't be clicked
		/// </summary>
		/// <param name="text">The text of the button</param>
		/// <returns>Always returns false</returns>
		private bool ButtonDisabled(string text)
		{
			ImGui.PushStyleColor(ImGuiCol.Button, new SIMD.Vector4(0.5f, 0.5f, 0.5f, 1.0f));
			ImGui.PushStyleColor(ImGuiCol.ButtonActive, new SIMD.Vector4(0.5f, 0.5f, 0.5f, 1.0f));
			ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new SIMD.Vector4(0.5f, 0.5f, 0.5f, 1.0f));
			ImGui.Button("Save Image");
			ImGui.PopStyleColor();
			ImGui.PopStyleColor();
			ImGui.PopStyleColor();
			return false;
		}
	}
}
