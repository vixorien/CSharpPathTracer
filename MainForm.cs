using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Threading;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.Xna.Framework;

namespace CSharpPathTracer
{
	public partial class MainForm : Form
	{
		private Bitmap renderTarget;
		private Raytracer raytracer;
		private Camera camera;
		
		private Environment environment;
		private List<Scene> scenes;

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

			raytracer = new Raytracer();
			raytracer.RaytraceScanlineComplete += Raytracer_RaytraceScanlineComplete;
			raytracer.RaytraceComplete += Raytracer_RaytraceComplete;

			camera = new Camera(
				new Vector3(0, 5, 20),
				Vector3.Normalize(new Vector3(0, -0.2f, -1)),
				raytracingDisplay.AspectRatio,
				MathF.PI / 4.0f,
				0.01f,
				1000.0f);

			// Update labels and such
			labelSamplesPerPixel.Text = "Samples Per Pixel: " + sliderSamplesPerPixel.Value;
			labelMaxRecursion.Text = "Max Recursion Depth: " + sliderMaxRecursion.Value;
			textWidth.Text = raytracingDisplay.Width.ToString();
			textHeight.Text = raytracingDisplay.Height.ToString();
		}

		private void buttonStartRaytrace_Click(object sender, EventArgs e)
		{
			// Update status
			labelStatus.Text = "Status: Raytracing...";

			// Create/re-create the render target using the display dimensions
			renderTarget?.Dispose();
			renderTarget = new Bitmap(raytracingDisplay.Width, raytracingDisplay.Height);
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
				renderTarget, 
				camera, 
				sliderSamplesPerPixel.Value, 
				sliderMaxRecursion.Value);
			raytracer.RaytraceScene(rtParams);
		}


		private void Raytracer_RaytraceComplete(RaytracingStats stats)
		{
			// Invalidate on complete to redisplay, in the event
			// progress isn't being shown
			raytracingDisplay.Invalidate();

			// Update stats
			labelStatus.Text = "Status: Complete";
			labelTotalRays.Text = "Total Rays: " + stats.TotalRays.ToString("N0");
			labelDeepestRecursion.Text = "Deepest Recursion: " + stats.DeepestRecursion;
			labelTime.Text = "Total Time: " + stats.TotalTime.ToString(@"hh\:mm\:ss\.fff");
		}

		private void Raytracer_RaytraceScanlineComplete(int y, RaytracingStats stats)
		{
			if(checkDisplayProgress.Checked)
				raytracingDisplay.Invalidate();

			// Update progress bar and other status
			progressRT.ProgressBar.IncrementNoAnimation(raytracingDisplay.Width); // An entire row

			labelStatus.Text = "Status: Raytracing..." + Math.Round((float)y / raytracingDisplay.Height * 100, 2) + "%";
			labelTotalRays.Text = "Total Rays: " + stats.TotalRays.ToString("N0");
			labelDeepestRecursion.Text = "Deepest Recursion: " + stats.DeepestRecursion;
			labelTime.Text = "Total Time: " + stats.TotalTime.ToString(@"hh\:mm\:ss\.fff");
		}

		
		private void sliderSamplesPerPixel_Scroll(object sender, EventArgs e)
		{
			labelSamplesPerPixel.Text = "Samples Per Pixel: " + sliderSamplesPerPixel.Value;
		}

		private void sliderMaxRecursion_Scroll(object sender, EventArgs e)
		{
			labelMaxRecursion.Text = "Max Recursion Depth: " + sliderMaxRecursion.Value;
		}

		private void MainForm_Resize(object sender, EventArgs e)
		{
			textWidth.Text = raytracingDisplay.Width.ToString();
			textHeight.Text = raytracingDisplay.Height.ToString();
			raytracingDisplay.Invalidate();
		}

		private void CreateScenes()
		{
			// === Environments ===
			environment = new Environment(
				System.Drawing.Color.CornflowerBlue.ToVector3(),
				System.Drawing.Color.White.ToVector3(),
				System.Drawing.Color.White.ToVector3());

			// === Materials ===
			Material grayMatte = new Material(System.Drawing.Color.Gray.ToVector3(), false);
			Material greenMatte = new Material(new Vector3(0.2f, 1.0f, 0.2f), false);
			Material blueMatte = new Material(new Vector3(0.2f, 0.2f, 1.0f), false);
			Material mirror = new Material(new Vector3(1, 1, 1), true);
			Material gold = new Material(new Vector3(1.000f, 0.766f, 0.336f), true);

			// === Meshes ===
			Mesh cubeMesh = new Mesh("Content/Models/cube.obj");
			Sphere sphereLarge = new Sphere(new Vector3(0, -1000, 0), 1000);

			// === SCENE 1 ===

			// Entities ===
			Entity cube = new Entity(cubeMesh, blueMatte);
			cube.Transform.MoveAbsolute(1, 0, 0);
			Entity ground = new Entity(sphereLarge, grayMatte);

			// Add to scene ===
			Scene scene1 = new Scene("Test", environment);
			scene1.Add(cube);
			scene1.Add(ground);

			scenes.Add(scene1);


			// Large sphere below
			//scene.Add(new Sphere(new Vector3(0, -1000, 0), 1000, grayMatte));

			// Small spheres in front of camera
			//scene.Add(new Sphere(new Vector3(-5, 2.0f, 0), 2.0f, greenMatte));
			//scene.Add(new Sphere(new Vector3(0, 4.0f, 0), 2.0f, mirror));
			//scene.Add(new Sphere(new Vector3(5, 2.0f, 0), 2.0f, gold));

			// Random floating spheres
			//for (int i = 0; i < 25; i++)
			//{
			//	scene.Add(new Sphere(
			//		new Vector3(rng.NextFloat(-10, 10), rng.NextFloat(0, 5), rng.NextFloat(-10, 10)),
			//		rng.NextFloat(0.25f, 1.0f),
			//		new Material(rng.NextColor(), rng.NextBool())));
			//}
		}
	}
}
