﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Numerics;

namespace CSharpPathTracer
{
	/// <summary>
	/// Possible raytracing modes
	/// </summary>
	public enum RaytracingMode
	{
		None,
		Realtime,
		Once
	}

	/// <summary>
	/// The main form of the application
	/// </summary>
	public partial class MainForm : Form
	{
		// Camera constants
		private const float CameraMoveSpeed = 0.25f;
		private const float CameraMoveSpeedSlow = 0.05f;
		private const float CameraMoveSpeedFast = 1.0f;
		private const float CameraRotationSpeed = 0.01f;

		// Options for real-time movement
		//private const int RealtimeSamplesPerPixel = 1;
		//private const int RealtimeResolutionReduction = 8;
		//private const int RealtimeMaxRecursion = 8;

		// Threading and UI
		private BackgroundWorker worker;
		private Bitmap renderTarget;
		private byte[] progressColorScanline;

		// Scene and rendering
		private Camera camera;
		private List<Scene> scenes;
		private RaytracerWindowsForms raytracer; 
		private RaytracingMode raytracingMode;
		private bool raytracingInProgress;

		// Timing
		private Stopwatch stopwatch;

		/// <summary>
		/// Gets the current maximum recursion value
		/// </summary>
		public int MaxRecursion { get { return sliderMaxRecursion.Value; } }

		/// <summary>
		/// Gets the current samples per pixel
		/// </summary>
		public int SamplesPerPixel { get { return sliderSamplesPerPixel.Value; } }

		/// <summary>
		/// Gets the current resolution reduction
		/// </summary>
		public int ResolutionReduction { get { return (int)Math.Pow(2, sliderResReduction.Value); } }

		/// <summary>
		/// Gets the current maximum recursion value
		/// </summary>
		public int MaxRecursionLive { get { return sliderMaxRecursionLive.Value; } }

		/// <summary>
		/// Gets the current samples per pixel
		/// </summary>
		public int SamplesPerPixelLive { get { return sliderSamplesPerPixelLive.Value; } }

		/// <summary>
		/// Gets the current resolution reduction
		/// </summary>
		public int ResolutionReductionLive { get { return (int)Math.Pow(2, sliderResReductionLive.Value); } }

		/// <summary>
		/// Creates the form
		/// </summary>
		public MainForm()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Form loaded setup
		/// </summary>
		private void MainForm_Load(object sender, EventArgs e)
		{
			// Camera for scene
			camera = new Camera(
				new Vector3(0, 8, 20),
				raytracingDisplay.AspectRatio,
				MathF.PI / 4.0f,
				0.01f,
				1000.0f,
				1.0f,
				20.0f);
			camera.Transform.Rotate(-0.25f, 0, 0);

			// Create the ray tracer before the background worker
			raytracer = new RaytracerWindowsForms();
			raytracingInProgress = false;
			raytracingMode = RaytracingMode.None;

			stopwatch = new Stopwatch();

			// Update labels and such
			labelSamplesPerPixel.Text = "Samples Per Pixel: " + SamplesPerPixel;
			labelMaxRecursion.Text = "Max Recursion Depth: " + MaxRecursion;
			labelResReduction.Text = "Resolution Reduction: " + ResolutionReduction;
			labelSamplesPerPixelLive.Text = "Samples Per Pixel: " + SamplesPerPixelLive;
			labelMaxRecursionLive.Text = "Max Recursion Depth: " + MaxRecursionLive;
			labelResReductionLive.Text = "Resolution Reduction: " + ResolutionReductionLive;
			labelDimensions.Text = "Dimensions: " + raytracingDisplay.Width.ToString() + "x" + raytracingDisplay.Height.ToString();
			labelFieldOfView.Text = "Field of View: " + (camera.FieldOfView * 180.0f / MathF.PI);
			labelAperture.Text = "Aperture: " + camera.Aperture;
			labelFocalDistance.Text = "Focal Plane Distance: " + camera.FocalDistance;

			// Set up initial camera slider values
			sliderFieldOfView.Value = (int)(camera.FieldOfView * 180.0f / MathF.PI);
			sliderFocalDistance.Value = (int)(camera.FocalDistance * 10);
			sliderAperture.Value = (int)(camera.Aperture * 10);

			// Set up scene(s)
			scenes = Scene.GenerateScenes();

			// Populate combo box with scenes
			foreach (Scene s in scenes)
			{
				comboScene.Items.Add(s.Name);
			}

			// Select the first scene (which will cause a raytrace!)
			comboScene.SelectedIndex = 0;

			// Start the frame loop
			timerFrameLoop.Start();
		}

		/// <summary>
		/// Button click for starting a full raytrace
		/// </summary>
		private void buttonStartRaytrace_Click(object sender, EventArgs e)
		{
			// If we're in progress, we're canceling the exisitng
			// work instead, which will be caught elsewhere
			if (raytracingInProgress)
			{
				worker.CancelAsync();
				return;
			}

			// Update the UI
			labelStatus.Text = "Status: Raytracing...";
			buttonStartRaytrace.Text = "Cancel Raytrace";

			// Swap to the full tab
			tabTraceOptions.SelectedTab = tabPageFullTrace;

			// Start the full raytrace with the user's values
			BeginRaytrace(
				RaytracingMode.Once,
				SamplesPerPixel,
				ResolutionReduction,
				MaxRecursion);
		}

		/// <summary>
		/// Begins an actual raytrace using a background worker
		/// </summary>
		/// <param name="mode">Which mode?</param>
		/// <param name="samplesPerPixel">How many samples per pixel?</param>
		/// <param name="resolutionReduction">What's the resolution reduction amount?</param>
		/// <param name="maxRecursion">What's the max allowable recursion depth?</param>
		private void BeginRaytrace(RaytracingMode mode, int samplesPerPixel, int resolutionReduction, int maxRecursion)
		{
			if (mode == RaytracingMode.None)
				return;

			// Update UI
			progressRT.Minimum = 0;
			progressRT.Maximum = raytracingDisplay.Height; // This is "locked in" when we start
			progressRT.Value = 0;
			buttonSave.Enabled = false;

			// Set up the worker for threading
			worker?.Dispose();
			worker = new BackgroundWorker();
			worker.WorkerReportsProgress = true;
			worker.WorkerSupportsCancellation = true;
			worker.DoWork += raytracer.RaytraceSceneBackgroundWorker;
			worker.ProgressChanged += ScanlineComplete;
			worker.RunWorkerCompleted += RaytraceComplete;

			// Create/re-create the render target using the display dimensions
			if (renderTarget == null || renderTarget.Width != raytracingDisplay.Width || renderTarget.Height != raytracingDisplay.Height)
			{
				renderTarget?.Dispose();
				renderTarget = new Bitmap(raytracingDisplay.Width, raytracingDisplay.Height, PixelFormat.Format24bppRgb);
				raytracingDisplay.Bitmap = renderTarget;

				progressColorScanline = new byte[renderTarget.Width * RaytracerWindowsForms.ChannelsPerPixel]; // Assume black
			}

			// Update camera to match new viewport
			camera.AspectRatio = (float)raytracingDisplay.Width / raytracingDisplay.Height;

			// Track our progress
			raytracingInProgress = true;
			raytracingMode = mode;

			// Set up parameters and start the thread
			RaytracingParameters rtParams = new RaytracingParameters(
				scenes[comboScene.SelectedIndex],
				camera,
				renderTarget.Width,
				renderTarget.Height,
				samplesPerPixel,
				resolutionReduction,
				maxRecursion,
				checkProgressive.Checked);
			worker.RunWorkerAsync(rtParams);

			// Restart the stopwatch to track raytracing time
			stopwatch.Restart();
		}

		/// <summary>
		/// Handles when a scanline is completed by the raytracer
		/// </summary>
		private void ScanlineComplete(object sender, ProgressChangedEventArgs e)
		{
			RaytracingProgressWindowsForms progress = e.UserState as RaytracingProgressWindowsForms;
			if (progress == null)
				return;

			// For reference: http://csharpexamples.com/tag/parallel-bitmap-processing/
			// Lock the bits to allow for very quick memory access
			BitmapData pixels = renderTarget.LockBits(
				new System.Drawing.Rectangle(0, 0, renderTarget.Width, renderTarget.Height),
				System.Drawing.Imaging.ImageLockMode.WriteOnly,
				renderTarget.PixelFormat);

			// Copy the scanline into the render target, duplicating
			// it as necessary based on the resolution reduction
			int y = progress.ScanlineIndex;
			for (; y < progress.ScanlineDuplicateCount + progress.ScanlineIndex && y < renderTarget.Height; y++)
			{
				Marshal.Copy(
					progress.Scanline,
					0,
					pixels.Scan0 + pixels.Stride * y,
					progress.Scanline.Length);
			}
			
			// Display the "progress" line as necessary (after any duplication)
			if (raytracingMode != RaytracingMode.Realtime && y < renderTarget.Height - 1)
			{
				Marshal.Copy(
					progressColorScanline,
					0,
					pixels.Scan0 + pixels.Stride * (y + 1),
					progressColorScanline.Length);
			}

			// Unlock the bits and invalidate the display to redraw
			renderTarget.UnlockBits(pixels);
			raytracingDisplay.Invalidate();

			// Update progress bar and other status if not realtime
			if (raytracingMode != RaytracingMode.Realtime)
			{
				//progressRT.ProgressBar.IncrementNoAnimation(progress.ScanlineDuplicateCount); // One or more rows are done
				progressRT.ProgressBar.SetPercentage(progress.CompletionPercent);

				labelStatus.Text = "Status: Raytracing..." + Math.Round(progress.CompletionPercent * 100, 2) + "%";
				labelTotalRays.Text = "Total Rays: " + progress.Stats.TotalRays.ToString("N0");
				labelDeepestRecursion.Text = "Deepest Recursion: " + progress.Stats.DeepestRecursion;
				labelTime.Text = "Total Time: " + stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff");
			}
		}

		/// <summary>
		/// Handles when the raytracing is complete (or canceled)
		/// </summary>
		private void RaytraceComplete(object sender, RunWorkerCompletedEventArgs e)
		{
			// Note: Can check for a cancel here!
			if (e.Cancelled)
			{
				labelStatus.Text = "Status: Canceled!";
			}
			else // Successful raytrace
			{
				labelStatus.Text = "Status: Complete!";
			}

			// Check the current mode
			if (raytracingMode == RaytracingMode.Once)
			{
				raytracingMode = RaytracingMode.None;
			}

			// Update UI
			raytracingInProgress = false;
			buttonSave.Enabled = true; 
			progressRT.ProgressBar.StopMarquee();
			stopwatch.Stop();

			// Update the button
			buttonStartRaytrace.Text = "Start Full Raytrace";
		}

		/// <summary>
		/// Update the samples text label
		/// </summary>
		private void sliderSamplesPerPixel_Scroll(object sender, EventArgs e)
		{
			labelSamplesPerPixel.Text = "Samples Per Pixel: " + SamplesPerPixel;
		}

		/// <summary>
		/// Update the recursion text label
		/// </summary>
		private void sliderMaxRecursion_Scroll(object sender, EventArgs e)
		{
			labelMaxRecursion.Text = "Max Recursion Depth: " + MaxRecursion;
		}

		/// <summary>
		/// Update the resolution reduction text label
		/// </summary>
		private void sliderResReduction_Scroll(object sender, EventArgs e)
		{
			labelResReduction.Text = "Resolution Reduction: " + ResolutionReduction;
		}

		/// <summary>
		/// Update the live samples text label
		/// </summary>
		private void sliderSamplesPerPixelLive_Scroll(object sender, EventArgs e)
		{
			labelSamplesPerPixelLive.Text = "Samples Per Pixel: " + SamplesPerPixelLive;
		}

		/// <summary>
		/// Update the live recursion text label
		/// </summary>
		private void sliderMaxRecursionLive_Scroll(object sender, EventArgs e)
		{
			labelMaxRecursionLive.Text = "Max Recursion Depth: " + MaxRecursionLive;
		}

		/// <summary>
		/// Update the live resolution reduction text label
		/// </summary>
		private void sliderResReductionLive_Scroll(object sender, EventArgs e)
		{
			labelResReductionLive.Text = "Resolution Reduction: " + ResolutionReductionLive;
		}

		/// <summary>
		/// Update the field of view of the camera and label
		/// </summary>
		private void sliderFieldOfView_Scroll(object sender, EventArgs e)
		{
			camera.FieldOfView = (float)(sliderFieldOfView.Value * MathF.PI / 180.0f);
			labelFieldOfView.Text = "Field of View: " + (camera.FieldOfView * 180.0f / MathF.PI);

			// Swap to the real-time tab
			tabTraceOptions.SelectedTab = tabPageRealTime;

			// Perform a single raytrace since the camera has changed
			BeginRaytrace(
				RaytracingMode.Once,
				SamplesPerPixelLive,
				ResolutionReductionLive,
				MaxRecursionLive);
		}

		/// <summary>
		/// Update the aperture of the camera and label
		/// </summary>
		private void sliderAperture_Scroll(object sender, EventArgs e)
		{
			camera.Aperture = sliderAperture.Value / 10.0f;
			labelAperture.Text = "Aperture: " + camera.Aperture;

			// Swap to the real-time tab
			tabTraceOptions.SelectedTab = tabPageRealTime;

			// Perform a single raytrace since the camera has changed
			BeginRaytrace(
				RaytracingMode.Once,
				SamplesPerPixelLive,
				ResolutionReductionLive,
				MaxRecursionLive);
		}

		/// <summary>
		/// Update the focal plane distance of the camera and label
		/// </summary>
		private void sliderFocalDistance_Scroll(object sender, EventArgs e)
		{
			camera.FocalDistance = sliderFocalDistance.Value / 10.0f;
			labelFocalDistance.Text = "Focal Plane Distance: " + camera.FocalDistance;

			// Swap to the real-time tab
			tabTraceOptions.SelectedTab = tabPageRealTime;

			// Perform a single raytrace since the camera has changed
			BeginRaytrace(
				RaytracingMode.Once,
				SamplesPerPixelLive,
				ResolutionReductionLive,
				MaxRecursionLive);
		}

		/// <summary>
		/// Adjusts the display now that the form has been resized
		/// </summary>
		private void MainForm_Resize(object sender, EventArgs e)
		{
			labelDimensions.Text = "Dimensions: " + raytracingDisplay.Width.ToString() + "x" + raytracingDisplay.Height.ToString();
			raytracingDisplay.Invalidate();
		}


		/// <summary>
		/// Re-renders a low-res version when the scene is changed
		/// </summary>
		private void comboScene_SelectedIndexChanged(object sender, EventArgs e)
		{
			// Perform a single low res raytrace now that the scene has changed
			BeginRaytrace(
				RaytracingMode.Once,
				SamplesPerPixelLive,
				ResolutionReductionLive,
				MaxRecursionLive);
		}

		/// <summary>
		/// Saves the current render target image to a 100% quality PNG
		/// </summary>
		private void buttonSave_Click(object sender, EventArgs e)
		{
			// Set up the save dialog for just PNG files
			SaveFileDialog diag = new SaveFileDialog();
			diag.Filter = "PNG|*.png";
			diag.AddExtension = true;

			// Show the dialog and only save on a confirmation
			if (diag.ShowDialog() == DialogResult.OK)
			{
				// Get the PNG encoder
				ImageCodecInfo pngEncoder = null;
				ImageCodecInfo[] encoders = ImageCodecInfo.GetImageEncoders();
				foreach (ImageCodecInfo enc in encoders)
				{
					if (enc.MimeType.Contains("png"))
					{
						pngEncoder = enc;
						break;
					}
				}

				// Verify it was found
				if (pngEncoder == null)
				{
					MessageBox.Show(
						"Error saving image: PNG encoder not found.",
						"Error Saving Image",
						MessageBoxButtons.OK,
						MessageBoxIcon.Error);
					return;
				}

				// Set up the quality level of the save
				EncoderParameters encParams = new EncoderParameters(1);
				encParams.Param[0] = new EncoderParameter(Encoder.Quality, 100L);

				// Perform the save
				renderTarget.Save(diag.FileName, pngEncoder, encParams);
			}
		}


		// ================================================
		//  Mouse input
		// ================================================

		private bool displayHasMouse = false;
		private Point prevMouse;

		/// <summary>
		/// Handles the mouse being clicked in the display area
		/// </summary>
		private void raytracingDisplay_MouseDown(object sender, MouseEventArgs e)
		{
			raytracingDisplay.Focus();
			displayHasMouse = true;
			prevMouse = e.Location;
			raytracingMode = RaytracingMode.Realtime;

			// Swap to the real-time tab
			tabTraceOptions.SelectedTab = tabPageRealTime;

			// Cancel any existing rendering
			worker?.CancelAsync();
		}

		/// <summary>
		/// Handles the mouse being released after being clicked in the display area
		/// </summary>
		private void raytracingDisplay_MouseUp(object sender, MouseEventArgs e)
		{
			displayHasMouse = false;

			// Stop any realtime raytracing (might swap to progressive?)
			if(raytracingMode == RaytracingMode.Realtime)
				raytracingMode = RaytracingMode.None;
		}

		/// <summary>
		/// Handles the mouse moving in the display area, which rotates the
		/// camera if the mouse button is currently down
		/// </summary>
		private void raytracingDisplay_MouseMove(object sender, MouseEventArgs e)
		{
			if (displayHasMouse)
			{
				// Rotate based on mouse movement
				camera.Transform.Rotate(
					(prevMouse.Y - e.Y) * CameraRotationSpeed,
					(prevMouse.X - e.X) * CameraRotationSpeed,
					0);

				// Remember previous location
				prevMouse = e.Location;
			}
		}

		/// <summary>
		/// The timer-based frame loop tick, which checks for keyboard
		/// input and updates real-time raytracing frames
		/// </summary>
		private void timerFrameLoop_Tick(object sender, EventArgs e)
		{
			// Only handle keyboard input for the camera while
			// the display has the mouse (while the mouse is down)
			if (displayHasMouse)
			{
				float speed = CameraMoveSpeed;
				if (IsKeyDown(Keys.ShiftKey)) { speed = CameraMoveSpeedFast; }
				else if (IsKeyDown(Keys.ControlKey)) { speed = CameraMoveSpeedSlow; }

				if (IsKeyDown(Keys.W)) { camera.Transform.MoveRelative(0, 0, -speed); }
				if (IsKeyDown(Keys.S)) { camera.Transform.MoveRelative(0, 0, speed); }
				if (IsKeyDown(Keys.A)) { camera.Transform.MoveRelative(-speed, 0, 0); }
				if (IsKeyDown(Keys.D)) { camera.Transform.MoveRelative(speed, 0, 0); }

				if (IsKeyDown(Keys.Space)) { camera.Transform.MoveRelative(0, speed, 0); }
				if (IsKeyDown(Keys.X)) { camera.Transform.MoveRelative(0, -speed, 0); }
			}

			// If we're in realtime mode and the worker is not currently busy (or doesn't exist yet),
			// then we can go ahead and begin a new low-res raytracing frame
			if (raytracingMode == RaytracingMode.Realtime &&
				(worker == null || (worker != null && !worker.IsBusy)))
			{
				BeginRaytrace(
					RaytracingMode.Realtime,
					SamplesPerPixelLive,
					ResolutionReductionLive,
					MaxRecursionLive);
			}

			Application.DoEvents();
		}

		// ================================================
		//  Keyboard input
		// ================================================

		private bool[] keyStates = new bool[256];

		/// <summary>
		/// Is the given key down?
		/// </summary>
		/// <param name="key">The key to check</param>
		/// <returns>True if the key is down, false if it's up</returns>
		private bool IsKeyDown(Keys key)
		{
			return keyStates[(int)key];
		}

		/// <summary>
		/// Handles a key being pressed and tracks the state
		/// </summary>
		private void MainForm_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyValue >= 0 && e.KeyValue < keyStates.Length)
			{
				keyStates[e.KeyValue] = true;
			}

			e.Handled = true;
		}

		/// <summary>
		/// Handles a key being released and tracks the state
		/// </summary>
		private void MainForm_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyValue >= 0 && e.KeyValue < keyStates.Length)
			{
				keyStates[e.KeyValue] = false;
			}

			e.Handled = true;
		}
	}
}
