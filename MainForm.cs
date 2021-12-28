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
			scenes = new List<Scene>();
			CreateScenes();

			// Populate combo box with scenes
			foreach (Scene s in scenes)
			{
				comboScene.Items.Add(s.Name);
			}
			comboScene.SelectedIndex = 0;

			// Camera for scene
			camera = new Camera(
				new Vector3(0, 5, 20),
				Vector3.Normalize(new Vector3(0, -0.2f, -1)),
				raytracingDisplay.AspectRatio,
				MathF.PI / 4.0f,
				0.01f,
				1000.0f);

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
			renderTarget?.Dispose();
			renderTarget = new Bitmap(raytracingDisplay.Width, raytracingDisplay.Height, PixelFormat.Format24bppRgb);
			raytracingDisplay.Bitmap = renderTarget;

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

		private void CreateScenes()
		{
			// Overall scene bounds
			BoundingBox sceneBounds = new BoundingBox(
				new Vector3(-10000),
				new Vector3(10000));

			// === Environments ===
			Environment environment = new Environment(
				System.Drawing.Color.CornflowerBlue.ToVector3(),
				System.Drawing.Color.White.ToVector3(),
				System.Drawing.Color.White.ToVector3());

			// === Materials ===
			Material grayMatte = new Material(System.Drawing.Color.LightGray.ToVector3(), false);
			Material greenMatte = new Material(new Vector3(0.2f, 1.0f, 0.2f), false);
			Material blueMatte = new Material(new Vector3(0.2f, 0.2f, 1.0f), false);
			Material mirror = new Material(new Vector3(1, 1, 1), true);
			Material gold = new Material(new Vector3(1.000f, 0.766f, 0.336f), true);
			Material transparent = new Material(new Vector3(1, 1, 1), false, true, 1.5f);

			// === Meshes ===
			Mesh cubeMesh = new Mesh("Content/Models/cube.obj");
			Mesh helixMesh = new Mesh("Content/Models/helix.obj");
			Mesh sphereMesh = new Mesh("Content/Models/sphere.obj");

			// === SCENE 0 ===
			{
				Entity ground = new Entity(Sphere.Default, grayMatte);
				ground.Transform.SetPosition(0, -1000, 0);
				ground.Transform.SetScale(1000);

				Entity left = new Entity(Sphere.Default, greenMatte);
				left.Transform.SetPosition(-5, 2, 0);
				left.Transform.SetScale(2);

				Entity middle = new Entity(Sphere.Default, mirror);
				middle.Transform.SetPosition(0, 4, 0);
				middle.Transform.SetScale(2);

				Entity right = new Entity(Sphere.Default, gold);
				right.Transform.SetPosition(5, 2, 0);
				right.Transform.SetScale(2);

				Entity close = new Entity(Sphere.Default, transparent);
				close.Transform.SetPosition(0, 2, 3);
				close.Transform.SetScale(2);

				Scene scene = new Scene("Default", environment, sceneBounds);
				scene.Add(ground);
				scene.Add(left);
				scene.Add(middle);
				scene.Add(right);
				scene.Add(close);

				scenes.Add(scene);
			}

			// === SCENE 1 ===
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
				scene.Add(cube);
				scene.Add(ground);
				//scene.Add(helix);
				//scene.Add(sphere);

				// Add to scene list
				scenes.Add(scene);
			}

			// === SCENE 2 ===
			{
				// Entities
				Entity ground = new Entity(Sphere.Default, grayMatte);
				ground.Transform.SetPosition(0, -1000, 0);
				ground.Transform.SetScale(1000);

				// Create scene
				Scene scene = new Scene("Random Spheres", environment, sceneBounds);
				scene.Add(ground);

				// Add random entities
				Random rng = new Random();
				for (int i = 0; i < 100; i++)
				{
					// Random scale (used for height, too)
					float scale = rng.NextFloat(0.1f, 1.0f);

					Entity s = new Entity(Sphere.Default, new Material(rng.NextColor(), rng.NextBool(), rng.NextBool(), 1.5f));
					s.Transform.SetPosition(rng.NextFloat(-20, 20), scale, rng.NextFloat(-20, 20));
					s.Transform.SetScale(scale);
					scene.Add(s);
				}

				// Add to scene list
				scenes.Add(scene);
			}
		}

		
	}
}
