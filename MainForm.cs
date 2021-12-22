using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Threading;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.Xna.Framework;

namespace CSharpRaytracing
{
	public partial class MainForm : Form
	{
		private Bitmap renderTarget;
		private Raytracer raytracer;
		private Camera camera;
		private Environment environment;

		public MainForm()
		{
			InitializeComponent();
		}

		private void MainForm_Load(object sender, EventArgs e)
		{
			environment = new Environment(
				System.Drawing.Color.CornflowerBlue.ToVector3(),
				System.Drawing.Color.White.ToVector3(),
				System.Drawing.Color.White.ToVector3());

			raytracer = new Raytracer(environment);
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
			numWidth.Value = raytracingDisplay.Width;
			numHeight.Value = raytracingDisplay.Height;
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
			RaytracingParameters rtParams = new RaytracingParameters(renderTarget, camera, sliderSamplesPerPixel.Value, sliderMaxRecursion.Value);
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
			numWidth.Value = raytracingDisplay.Width;
			numHeight.Value = raytracingDisplay.Height;
			raytracingDisplay.Invalidate();
		}
	}
}
