using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;

namespace CSharpPathTracer
{
	public partial class MainForm : Form
	{
		private BackgroundWorker worker;
		private Bitmap renderTarget;
		private Raytracer raytracer;
		private Camera camera;
		
		private List<Scene> scenes;

		private Stopwatch stopwatch;
		private bool raytracingInProgress;

		public MainForm()
		{
			InitializeComponent();
		}

		private void MainForm_Load(object sender, EventArgs e)
		{
			// Set up scene(s)
			scenes = Scene.GenerateScenes();

			// Populate combo box with scenes
			foreach (Scene s in scenes)
			{
				comboScene.Items.Add(s.Name);
			}
			comboScene.SelectedIndex = 0;

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
			stopwatch = new Stopwatch();

			// Update labels and such
			labelSamplesPerPixel.Text = "Samples Per Pixel: " + sliderSamplesPerPixel.Value;
			labelMaxRecursion.Text = "Max Recursion Depth: " + sliderMaxRecursion.Value;
			labelResReduction.Text = "Resolution Reduction: " + sliderResReduction.Value;
			textWidth.Text = raytracingDisplay.Width.ToString();
			textHeight.Text = raytracingDisplay.Height.ToString();
		}

		
		private void buttonStartRaytrace_Click(object sender, EventArgs e)
		{
			// If we're in progress, we simply cancel and swap text
			if (raytracingInProgress)
			{
				worker.CancelAsync();
				return;
			}

			// Set up the worker for threading
			worker?.Dispose();
			worker = new BackgroundWorker();
			worker.WorkerReportsProgress = true;
			worker.WorkerSupportsCancellation = true;
			worker.DoWork += raytracer.RaytraceScene;
			worker.ProgressChanged += ScanlineComplete;
			worker.RunWorkerCompleted += RaytraceComplete;

			// Update status
			labelStatus.Text = "Status: Raytracing...";

			// Create/re-create the render target using the display dimensions
			if (renderTarget == null || renderTarget.Width != raytracingDisplay.Width || renderTarget.Height != raytracingDisplay.Height)
			{
				renderTarget?.Dispose();
				renderTarget = new Bitmap(raytracingDisplay.Width, raytracingDisplay.Height, PixelFormat.Format24bppRgb);
				raytracingDisplay.Bitmap = renderTarget;
			}

			// Set up progress bar
			progressRT.Minimum = 0;
			progressRT.Maximum = raytracingDisplay.Width * raytracingDisplay.Height;
			progressRT.Value = 0;

			// Update camera to match new viewport
			camera.AspectRatio = (float)raytracingDisplay.Width / raytracingDisplay.Height;

			// Raytrace the scene
			RaytracingParameters rtParams = new RaytracingParameters(
				scenes[comboScene.SelectedIndex], 
				camera, 
				renderTarget.Width,
				renderTarget.Height,
				sliderSamplesPerPixel.Value, 
				sliderResReduction.Value,
				sliderMaxRecursion.Value);
			worker.RunWorkerAsync(rtParams);

			raytracingInProgress = true;
			buttonStartRaytrace.Text = "Cancel Raytrace";

			stopwatch.Restart();
		}


		private void ScanlineComplete(object sender, ProgressChangedEventArgs e)
		{
			RaytracingProgress progress = e.UserState as RaytracingProgress;
			if (progress == null)
				return;

			// For threading reference: http://csharpexamples.com/tag/parallel-bitmap-processing/
			// Copy data to the correct scanline in the bitmap
			BitmapData pixels = renderTarget.LockBits(
				new System.Drawing.Rectangle(0, 0, renderTarget.Width, renderTarget.Height),
				System.Drawing.Imaging.ImageLockMode.WriteOnly,
				renderTarget.PixelFormat);

			// Duplicate the scanline as necessary
			for (int y = progress.ScanlineIndex; y < progress.ScanlineDuplicateCount + progress.ScanlineIndex && y < renderTarget.Height; y++)
			{
				Marshal.Copy(
					progress.Scanline,
					0,
					pixels.Scan0 + pixels.Stride * y,
					progress.Scanline.Length);
			}

			renderTarget.UnlockBits(pixels);

			if (checkDisplayProgress.Checked)
				raytracingDisplay.Invalidate();

			// Update progress bar and other status
			progressRT.ProgressBar.IncrementNoAnimation(raytracingDisplay.Width * sliderResReduction.Value); // An entire row

			labelStatus.Text = "Status: Raytracing..." + Math.Round(progress.CompletionPercent, 2) + "%";
			labelTotalRays.Text = "Total Rays: " + progress.Stats.TotalRays.ToString("N0");
			labelDeepestRecursion.Text = "Deepest Recursion: " + progress.Stats.DeepestRecursion;
			labelTime.Text = "Total Time: " + stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff");
		}

		private void RaytraceComplete(object sender, RunWorkerCompletedEventArgs e)
		{
			// Note: Can check for a cancel here!
			if (!e.Cancelled)
			{
				labelStatus.Text = "Status: Complete!";
			}

			progressRT.ProgressBar.StopMarquee();
			buttonStartRaytrace.Text = "Start Raytrace";
			raytracingInProgress = false;
			stopwatch.Stop();
		}


		private void sliderSamplesPerPixel_Scroll(object sender, EventArgs e)
		{
			labelSamplesPerPixel.Text = "Samples Per Pixel: " + sliderSamplesPerPixel.Value;
		}

		private void sliderMaxRecursion_Scroll(object sender, EventArgs e)
		{
			labelMaxRecursion.Text = "Max Recursion Depth: " + sliderMaxRecursion.Value;
		}

		private void sliderResReduction_Scroll(object sender, EventArgs e)
		{
			labelResReduction.Text = "Resolution Reduction: " + sliderResReduction.Value;
		}

		private void MainForm_Resize(object sender, EventArgs e)
		{
			textWidth.Text = raytracingDisplay.Width.ToString();
			textHeight.Text = raytracingDisplay.Height.ToString();
			raytracingDisplay.Invalidate();
		}
		

		private bool displayHasMouse = false;
		private void raytracingDisplay_MouseDown(object sender, MouseEventArgs e)
		{
			raytracingDisplay.Focus();
			displayHasMouse = true;
		}

		private void raytracingDisplay_MouseUp(object sender, MouseEventArgs e)
		{
			displayHasMouse = false;
		}

		private void raytracingDisplay_MouseMove(object sender, MouseEventArgs e)
		{
			if (displayHasMouse)
			{
				buttonStartRaytrace.BackColor = ThreadSafeRandom.Instance.NextBool() ? System.Drawing.Color.AliceBlue : System.Drawing.Color.Brown;
			}
		}
	}
}
