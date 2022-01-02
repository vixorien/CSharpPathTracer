using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Windows.Input;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;

namespace CSharpPathTracer
{
	public enum RaytracingMode
	{
		None,
		Realtime,
		Once
	}

	public partial class MainForm : Form
	{
		private const float CameraMoveSpeed = 0.25f;
		private const float CameraMoveSpeedSlow = 0.05f;
		private const float CameraMoveSpeedFast = 1.0f;
		private const float CameraRotationSpeed = 0.01f;

		private const int RealtimeSamplesPerPixel = 1;
		private const int RealtimeResolutionReduction = 8;
		private const int RealtimeMaxRecursion = 8;


		private BackgroundWorker worker;
		private Bitmap renderTarget;
		private Raytracer raytracer;
		private Camera camera;

		private List<Scene> scenes;

		private Stopwatch stopwatch;

		private byte[] progressColorScanline;

		private RaytracingMode raytracingMode;
		private bool raytracingInProgress;

		public int MaxRecursion { get { return sliderMaxRecursion.Value; } }
		public int SamplesPerPixel { get { return sliderSamplesPerPixel.Value; } }
		public int ResolutionReduction { get { return (int)Math.Pow(2, sliderResReduction.Value); } }

		public MainForm()
		{
			InitializeComponent();
		}

		private void MainForm_Load(object sender, EventArgs e)
		{
			// Camera for scene
			camera = new Camera(
				new Vector3(0, 8, 20),
				raytracingDisplay.AspectRatio,
				MathF.PI / 4.0f,
				0.01f,
				1000.0f);
			camera.Transform.Rotate(-0.25f, 0, 0);

			// Create the ray tracer before the background worker
			raytracer = new Raytracer();
			raytracingInProgress = false;
			raytracingMode = RaytracingMode.None;

			stopwatch = new Stopwatch();

			// Update labels and such
			labelSamplesPerPixel.Text = "Samples Per Pixel: " + SamplesPerPixel;
			labelMaxRecursion.Text = "Max Recursion Depth: " + MaxRecursion;
			labelResReduction.Text = "Resolution Reduction: " + ResolutionReduction;
			textWidth.Text = raytracingDisplay.Width.ToString();
			textHeight.Text = raytracingDisplay.Height.ToString();

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
			

			// Start the full raytrace with the user's values
			BeginRaytrace(
				RaytracingMode.Once,
				SamplesPerPixel,
				ResolutionReduction,
				MaxRecursion);
		}

		private void BeginRaytrace(RaytracingMode mode, int samplesPerPixel, int resolutionReduction, int maxRecursion)
		{
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
			worker.DoWork += raytracer.RaytraceScene;
			worker.ProgressChanged += ScanlineComplete;
			worker.RunWorkerCompleted += RaytraceComplete;

			// Create/re-create the render target using the display dimensions
			if (renderTarget == null || renderTarget.Width != raytracingDisplay.Width || renderTarget.Height != raytracingDisplay.Height)
			{
				renderTarget?.Dispose();
				renderTarget = new Bitmap(raytracingDisplay.Width, raytracingDisplay.Height, PixelFormat.Format24bppRgb);
				raytracingDisplay.Bitmap = renderTarget;

				progressColorScanline = new byte[renderTarget.Width * Raytracer.ChannelsPerPixel]; // Assume black
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
				maxRecursion);
			worker.RunWorkerAsync(rtParams);

			// Restart the stopwatch to track raytracing time
			stopwatch.Restart();
		}


		private void ScanlineComplete(object sender, ProgressChangedEventArgs e)
		{
			RaytracingProgress progress = e.UserState as RaytracingProgress;
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
				progressRT.ProgressBar.IncrementNoAnimation(progress.ScanlineDuplicateCount); // One or more rows are done

				labelStatus.Text = "Status: Raytracing..." + Math.Round(progress.CompletionPercent, 2) + "%";
				labelTotalRays.Text = "Total Rays: " + progress.Stats.TotalRays.ToString("N0");
				labelDeepestRecursion.Text = "Deepest Recursion: " + progress.Stats.DeepestRecursion;
				labelTime.Text = "Total Time: " + stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff");
			}
		}

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


		private void sliderSamplesPerPixel_Scroll(object sender, EventArgs e)
		{
			labelSamplesPerPixel.Text = "Samples Per Pixel: " + SamplesPerPixel;
		}

		private void sliderMaxRecursion_Scroll(object sender, EventArgs e)
		{
			labelMaxRecursion.Text = "Max Recursion Depth: " + MaxRecursion;
		}

		private void sliderResReduction_Scroll(object sender, EventArgs e)
		{
			labelResReduction.Text = "Resolution Reduction: " + ResolutionReduction;
		}

		private void MainForm_Resize(object sender, EventArgs e)
		{
			textWidth.Text = raytracingDisplay.Width.ToString();
			textHeight.Text = raytracingDisplay.Height.ToString();
			raytracingDisplay.Invalidate();
		}



		private void comboScene_SelectedIndexChanged(object sender, EventArgs e)
		{
			// Perform a single low res raytrace now that the scene has changed
			BeginRaytrace(
				RaytracingMode.Once,
				RealtimeSamplesPerPixel,
				RealtimeResolutionReduction,
				RealtimeMaxRecursion);
		}


		private bool displayHasMouse = false;
		private int prevMouseX;
		private int prevMouseY;
		private void raytracingDisplay_MouseDown(object sender, MouseEventArgs e)
		{
			raytracingDisplay.Focus();
			displayHasMouse = true;
			prevMouseX = e.X;
			prevMouseY = e.Y;
			raytracingMode = RaytracingMode.Realtime;

			// Cancel any existing rendering
			worker?.CancelAsync();
		}

		private void raytracingDisplay_MouseUp(object sender, MouseEventArgs e)
		{
			displayHasMouse = false;

			// Stop any realtime raytracing (might swap to progressive?)
			if(raytracingMode == RaytracingMode.Realtime)
				raytracingMode = RaytracingMode.None;
		}

		private void raytracingDisplay_MouseMove(object sender, MouseEventArgs e)
		{
			if (displayHasMouse)
			{
				// Rotate based on mouse movement
				camera.Transform.Rotate(
					(prevMouseY - e.Y) * CameraRotationSpeed,
					(prevMouseX - e.X) * CameraRotationSpeed,
					0);

				// Remember previous location
				prevMouseX = e.X;
				prevMouseY = e.Y;
			}
		}

		private void timerFrameLoop_Tick(object sender, EventArgs e)
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


			// If we're in realtime mode and the worker is not currently busy (or doesn't exist yet),
			// then we can go ahead and begin a new low-res raytracing frame
			if (raytracingMode == RaytracingMode.Realtime &&
				(worker == null || (worker != null && !worker.IsBusy)))
			{
				BeginRaytrace(
					RaytracingMode.Realtime,
					RealtimeSamplesPerPixel,
					RealtimeResolutionReduction,
					RealtimeMaxRecursion);	
			}

		}

		private bool[] keyStates = new bool[256];

		private bool IsKeyDown(Keys key)
		{
			return keyStates[(int)key];
		}

		private void MainForm_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyValue >= 0 && e.KeyValue < keyStates.Length)
			{
				keyStates[e.KeyValue] = true;
			}

			e.Handled = true;
		}

		private void MainForm_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyValue >= 0 && e.KeyValue < keyStates.Length)
			{
				keyStates[e.KeyValue] = false;
			}

			e.Handled = true;
		}



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
	}
}
