﻿
namespace CSharpPathTracer
{
	partial class MainForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.buttonStartRaytrace = new System.Windows.Forms.Button();
			this.raytracingDisplay = new CSharpPathTracer.BitmapDisplay();
			this.raytracingStatus = new System.Windows.Forms.StatusStrip();
			this.labelDimensions = new System.Windows.Forms.ToolStripStatusLabel();
			this.labelStatus = new System.Windows.Forms.ToolStripStatusLabel();
			this.progressRT = new System.Windows.Forms.ToolStripProgressBar();
			this.labelTotalRays = new System.Windows.Forms.ToolStripStatusLabel();
			this.labelDeepestRecursion = new System.Windows.Forms.ToolStripStatusLabel();
			this.labelTime = new System.Windows.Forms.ToolStripStatusLabel();
			this.labelScene = new System.Windows.Forms.Label();
			this.comboScene = new System.Windows.Forms.ComboBox();
			this.timerFrameLoop = new System.Windows.Forms.Timer(this.components);
			this.buttonSave = new System.Windows.Forms.Button();
			this.checkProgressive = new System.Windows.Forms.CheckBox();
			this.tabTraceOptions = new System.Windows.Forms.TabControl();
			this.tabPageRealTime = new System.Windows.Forms.TabPage();
			this.labelResReductionLive = new System.Windows.Forms.Label();
			this.sliderResReductionLive = new System.Windows.Forms.TrackBar();
			this.labelMaxRecursionLive = new System.Windows.Forms.Label();
			this.sliderMaxRecursionLive = new System.Windows.Forms.TrackBar();
			this.labelSamplesPerPixelLive = new System.Windows.Forms.Label();
			this.sliderSamplesPerPixelLive = new System.Windows.Forms.TrackBar();
			this.tabPageFullTrace = new System.Windows.Forms.TabPage();
			this.labelResReduction = new System.Windows.Forms.Label();
			this.sliderResReduction = new System.Windows.Forms.TrackBar();
			this.labelMaxRecursion = new System.Windows.Forms.Label();
			this.sliderMaxRecursion = new System.Windows.Forms.TrackBar();
			this.labelSamplesPerPixel = new System.Windows.Forms.Label();
			this.sliderSamplesPerPixel = new System.Windows.Forms.TrackBar();
			this.groupBoxCamera = new System.Windows.Forms.GroupBox();
			this.sliderFocalDistance = new System.Windows.Forms.TrackBar();
			this.sliderAperture = new System.Windows.Forms.TrackBar();
			this.sliderFieldOfView = new System.Windows.Forms.TrackBar();
			this.labelFocalDistance = new System.Windows.Forms.Label();
			this.labelAperture = new System.Windows.Forms.Label();
			this.labelFieldOfView = new System.Windows.Forms.Label();
			this.raytracingStatus.SuspendLayout();
			this.tabTraceOptions.SuspendLayout();
			this.tabPageRealTime.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.sliderResReductionLive)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.sliderMaxRecursionLive)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.sliderSamplesPerPixelLive)).BeginInit();
			this.tabPageFullTrace.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.sliderResReduction)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.sliderMaxRecursion)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.sliderSamplesPerPixel)).BeginInit();
			this.groupBoxCamera.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.sliderFocalDistance)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.sliderAperture)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.sliderFieldOfView)).BeginInit();
			this.SuspendLayout();
			// 
			// buttonStartRaytrace
			// 
			this.buttonStartRaytrace.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonStartRaytrace.Location = new System.Drawing.Point(13, 566);
			this.buttonStartRaytrace.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.buttonStartRaytrace.Name = "buttonStartRaytrace";
			this.buttonStartRaytrace.Size = new System.Drawing.Size(175, 49);
			this.buttonStartRaytrace.TabIndex = 1;
			this.buttonStartRaytrace.Text = "Start Full Raytrace";
			this.buttonStartRaytrace.UseVisualStyleBackColor = true;
			this.buttonStartRaytrace.Click += new System.EventHandler(this.buttonStartRaytrace_Click);
			// 
			// raytracingDisplay
			// 
			this.raytracingDisplay.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.raytracingDisplay.BackColor = System.Drawing.SystemColors.ControlDark;
			this.raytracingDisplay.Bitmap = null;
			this.raytracingDisplay.Location = new System.Drawing.Point(296, 20);
			this.raytracingDisplay.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.raytracingDisplay.Name = "raytracingDisplay";
			this.raytracingDisplay.Size = new System.Drawing.Size(870, 595);
			this.raytracingDisplay.TabIndex = 7;
			this.raytracingDisplay.Text = "renderTarget1";
			this.raytracingDisplay.MouseDown += new System.Windows.Forms.MouseEventHandler(this.raytracingDisplay_MouseDown);
			this.raytracingDisplay.MouseMove += new System.Windows.Forms.MouseEventHandler(this.raytracingDisplay_MouseMove);
			this.raytracingDisplay.MouseUp += new System.Windows.Forms.MouseEventHandler(this.raytracingDisplay_MouseUp);
			// 
			// raytracingStatus
			// 
			this.raytracingStatus.ImageScalingSize = new System.Drawing.Size(24, 24);
			this.raytracingStatus.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.labelDimensions,
            this.labelStatus,
            this.progressRT,
            this.labelTotalRays,
            this.labelDeepestRecursion,
            this.labelTime});
			this.raytracingStatus.Location = new System.Drawing.Point(0, 631);
			this.raytracingStatus.Name = "raytracingStatus";
			this.raytracingStatus.Padding = new System.Windows.Forms.Padding(1, 0, 20, 0);
			this.raytracingStatus.Size = new System.Drawing.Size(1183, 30);
			this.raytracingStatus.TabIndex = 13;
			this.raytracingStatus.Text = "statusStrip1";
			// 
			// labelDimensions
			// 
			this.labelDimensions.Name = "labelDimensions";
			this.labelDimensions.Size = new System.Drawing.Size(80, 25);
			this.labelDimensions.Text = "Dimensions: ?";
			this.labelDimensions.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelStatus
			// 
			this.labelStatus.Name = "labelStatus";
			this.labelStatus.Size = new System.Drawing.Size(198, 25);
			this.labelStatus.Spring = true;
			this.labelStatus.Text = "Status: Waiting";
			this.labelStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// progressRT
			// 
			this.progressRT.Name = "progressRT";
			this.progressRT.Size = new System.Drawing.Size(286, 24);
			this.progressRT.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
			// 
			// labelTotalRays
			// 
			this.labelTotalRays.Name = "labelTotalRays";
			this.labelTotalRays.Size = new System.Drawing.Size(198, 25);
			this.labelTotalRays.Spring = true;
			this.labelTotalRays.Text = "Total Rays: ?";
			this.labelTotalRays.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelDeepestRecursion
			// 
			this.labelDeepestRecursion.Name = "labelDeepestRecursion";
			this.labelDeepestRecursion.Size = new System.Drawing.Size(198, 25);
			this.labelDeepestRecursion.Spring = true;
			this.labelDeepestRecursion.Text = "Deepest Recursion: ?";
			this.labelDeepestRecursion.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelTime
			// 
			this.labelTime.Name = "labelTime";
			this.labelTime.Size = new System.Drawing.Size(198, 25);
			this.labelTime.Spring = true;
			this.labelTime.Text = "Total Time: ?";
			this.labelTime.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelScene
			// 
			this.labelScene.AutoSize = true;
			this.labelScene.Location = new System.Drawing.Point(13, 23);
			this.labelScene.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.labelScene.Name = "labelScene";
			this.labelScene.Size = new System.Drawing.Size(38, 15);
			this.labelScene.TabIndex = 16;
			this.labelScene.Text = "Scene";
			// 
			// comboScene
			// 
			this.comboScene.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboScene.FormattingEnabled = true;
			this.comboScene.Location = new System.Drawing.Point(59, 20);
			this.comboScene.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.comboScene.Name = "comboScene";
			this.comboScene.Size = new System.Drawing.Size(216, 23);
			this.comboScene.TabIndex = 17;
			this.comboScene.SelectedIndexChanged += new System.EventHandler(this.comboScene_SelectedIndexChanged);
			// 
			// timerFrameLoop
			// 
			this.timerFrameLoop.Interval = 16;
			this.timerFrameLoop.Tick += new System.EventHandler(this.timerFrameLoop_Tick);
			// 
			// buttonSave
			// 
			this.buttonSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonSave.Location = new System.Drawing.Point(200, 566);
			this.buttonSave.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.buttonSave.Name = "buttonSave";
			this.buttonSave.Size = new System.Drawing.Size(71, 49);
			this.buttonSave.TabIndex = 20;
			this.buttonSave.Text = "Save Results";
			this.buttonSave.UseVisualStyleBackColor = true;
			this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
			// 
			// checkProgressive
			// 
			this.checkProgressive.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.checkProgressive.AutoSize = true;
			this.checkProgressive.Location = new System.Drawing.Point(14, 537);
			this.checkProgressive.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.checkProgressive.Name = "checkProgressive";
			this.checkProgressive.Size = new System.Drawing.Size(145, 19);
			this.checkProgressive.TabIndex = 21;
			this.checkProgressive.Text = "Progressive Raytracing";
			this.checkProgressive.UseVisualStyleBackColor = true;
			// 
			// tabTraceOptions
			// 
			this.tabTraceOptions.Controls.Add(this.tabPageRealTime);
			this.tabTraceOptions.Controls.Add(this.tabPageFullTrace);
			this.tabTraceOptions.Location = new System.Drawing.Point(14, 51);
			this.tabTraceOptions.Name = "tabTraceOptions";
			this.tabTraceOptions.SelectedIndex = 0;
			this.tabTraceOptions.Size = new System.Drawing.Size(261, 242);
			this.tabTraceOptions.TabIndex = 22;
			// 
			// tabPageRealTime
			// 
			this.tabPageRealTime.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.tabPageRealTime.Controls.Add(this.labelResReductionLive);
			this.tabPageRealTime.Controls.Add(this.sliderResReductionLive);
			this.tabPageRealTime.Controls.Add(this.labelMaxRecursionLive);
			this.tabPageRealTime.Controls.Add(this.sliderMaxRecursionLive);
			this.tabPageRealTime.Controls.Add(this.labelSamplesPerPixelLive);
			this.tabPageRealTime.Controls.Add(this.sliderSamplesPerPixelLive);
			this.tabPageRealTime.Location = new System.Drawing.Point(4, 24);
			this.tabPageRealTime.Name = "tabPageRealTime";
			this.tabPageRealTime.Padding = new System.Windows.Forms.Padding(3);
			this.tabPageRealTime.Size = new System.Drawing.Size(253, 214);
			this.tabPageRealTime.TabIndex = 0;
			this.tabPageRealTime.Text = "Live (Real-Time) Settings";
			// 
			// labelResReductionLive
			// 
			this.labelResReductionLive.AutoSize = true;
			this.labelResReductionLive.Location = new System.Drawing.Point(10, 148);
			this.labelResReductionLive.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.labelResReductionLive.Name = "labelResReductionLive";
			this.labelResReductionLive.Size = new System.Drawing.Size(120, 15);
			this.labelResReductionLive.TabIndex = 31;
			this.labelResReductionLive.Text = "Resolution Reduction";
			// 
			// sliderResReductionLive
			// 
			this.sliderResReductionLive.LargeChange = 1;
			this.sliderResReductionLive.Location = new System.Drawing.Point(10, 168);
			this.sliderResReductionLive.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.sliderResReductionLive.Maximum = 4;
			this.sliderResReductionLive.Name = "sliderResReductionLive";
			this.sliderResReductionLive.Size = new System.Drawing.Size(226, 45);
			this.sliderResReductionLive.TabIndex = 30;
			this.sliderResReductionLive.Value = 3;
			this.sliderResReductionLive.Scroll += new System.EventHandler(this.sliderResReductionLive_Scroll);
			// 
			// labelMaxRecursionLive
			// 
			this.labelMaxRecursionLive.AutoSize = true;
			this.labelMaxRecursionLive.Location = new System.Drawing.Point(10, 80);
			this.labelMaxRecursionLive.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.labelMaxRecursionLive.Name = "labelMaxRecursionLive";
			this.labelMaxRecursionLive.Size = new System.Drawing.Size(152, 15);
			this.labelMaxRecursionLive.TabIndex = 29;
			this.labelMaxRecursionLive.Text = "Maximum Recursion Depth";
			// 
			// sliderMaxRecursionLive
			// 
			this.sliderMaxRecursionLive.Location = new System.Drawing.Point(10, 100);
			this.sliderMaxRecursionLive.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.sliderMaxRecursionLive.Maximum = 50;
			this.sliderMaxRecursionLive.Minimum = 1;
			this.sliderMaxRecursionLive.Name = "sliderMaxRecursionLive";
			this.sliderMaxRecursionLive.Size = new System.Drawing.Size(226, 45);
			this.sliderMaxRecursionLive.TabIndex = 28;
			this.sliderMaxRecursionLive.TickFrequency = 5;
			this.sliderMaxRecursionLive.Value = 8;
			this.sliderMaxRecursionLive.Scroll += new System.EventHandler(this.sliderMaxRecursionLive_Scroll);
			// 
			// labelSamplesPerPixelLive
			// 
			this.labelSamplesPerPixelLive.AutoSize = true;
			this.labelSamplesPerPixelLive.Location = new System.Drawing.Point(10, 12);
			this.labelSamplesPerPixelLive.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.labelSamplesPerPixelLive.Name = "labelSamplesPerPixelLive";
			this.labelSamplesPerPixelLive.Size = new System.Drawing.Size(79, 15);
			this.labelSamplesPerPixelLive.TabIndex = 27;
			this.labelSamplesPerPixelLive.Text = "Rays Per Pixel";
			// 
			// sliderSamplesPerPixelLive
			// 
			this.sliderSamplesPerPixelLive.LargeChange = 2;
			this.sliderSamplesPerPixelLive.Location = new System.Drawing.Point(10, 32);
			this.sliderSamplesPerPixelLive.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.sliderSamplesPerPixelLive.Maximum = 16;
			this.sliderSamplesPerPixelLive.Minimum = 1;
			this.sliderSamplesPerPixelLive.Name = "sliderSamplesPerPixelLive";
			this.sliderSamplesPerPixelLive.Size = new System.Drawing.Size(226, 45);
			this.sliderSamplesPerPixelLive.TabIndex = 26;
			this.sliderSamplesPerPixelLive.Value = 1;
			this.sliderSamplesPerPixelLive.Scroll += new System.EventHandler(this.sliderSamplesPerPixelLive_Scroll);
			// 
			// tabPageFullTrace
			// 
			this.tabPageFullTrace.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.tabPageFullTrace.Controls.Add(this.labelResReduction);
			this.tabPageFullTrace.Controls.Add(this.sliderResReduction);
			this.tabPageFullTrace.Controls.Add(this.labelMaxRecursion);
			this.tabPageFullTrace.Controls.Add(this.sliderMaxRecursion);
			this.tabPageFullTrace.Controls.Add(this.labelSamplesPerPixel);
			this.tabPageFullTrace.Controls.Add(this.sliderSamplesPerPixel);
			this.tabPageFullTrace.Location = new System.Drawing.Point(4, 24);
			this.tabPageFullTrace.Name = "tabPageFullTrace";
			this.tabPageFullTrace.Padding = new System.Windows.Forms.Padding(3);
			this.tabPageFullTrace.Size = new System.Drawing.Size(253, 214);
			this.tabPageFullTrace.TabIndex = 1;
			this.tabPageFullTrace.Text = "Full (Slow) Settings";
			// 
			// labelResReduction
			// 
			this.labelResReduction.AutoSize = true;
			this.labelResReduction.Location = new System.Drawing.Point(10, 148);
			this.labelResReduction.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.labelResReduction.Name = "labelResReduction";
			this.labelResReduction.Size = new System.Drawing.Size(120, 15);
			this.labelResReduction.TabIndex = 25;
			this.labelResReduction.Text = "Resolution Reduction";
			// 
			// sliderResReduction
			// 
			this.sliderResReduction.LargeChange = 1;
			this.sliderResReduction.Location = new System.Drawing.Point(10, 168);
			this.sliderResReduction.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.sliderResReduction.Maximum = 4;
			this.sliderResReduction.Name = "sliderResReduction";
			this.sliderResReduction.Size = new System.Drawing.Size(226, 45);
			this.sliderResReduction.TabIndex = 24;
			this.sliderResReduction.Scroll += new System.EventHandler(this.sliderResReduction_Scroll);
			// 
			// labelMaxRecursion
			// 
			this.labelMaxRecursion.AutoSize = true;
			this.labelMaxRecursion.Location = new System.Drawing.Point(10, 80);
			this.labelMaxRecursion.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.labelMaxRecursion.Name = "labelMaxRecursion";
			this.labelMaxRecursion.Size = new System.Drawing.Size(152, 15);
			this.labelMaxRecursion.TabIndex = 23;
			this.labelMaxRecursion.Text = "Maximum Recursion Depth";
			// 
			// sliderMaxRecursion
			// 
			this.sliderMaxRecursion.Location = new System.Drawing.Point(10, 100);
			this.sliderMaxRecursion.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.sliderMaxRecursion.Maximum = 50;
			this.sliderMaxRecursion.Minimum = 1;
			this.sliderMaxRecursion.Name = "sliderMaxRecursion";
			this.sliderMaxRecursion.Size = new System.Drawing.Size(226, 45);
			this.sliderMaxRecursion.TabIndex = 22;
			this.sliderMaxRecursion.TickFrequency = 5;
			this.sliderMaxRecursion.Value = 25;
			this.sliderMaxRecursion.Scroll += new System.EventHandler(this.sliderMaxRecursion_Scroll);
			// 
			// labelSamplesPerPixel
			// 
			this.labelSamplesPerPixel.AutoSize = true;
			this.labelSamplesPerPixel.Location = new System.Drawing.Point(10, 12);
			this.labelSamplesPerPixel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.labelSamplesPerPixel.Name = "labelSamplesPerPixel";
			this.labelSamplesPerPixel.Size = new System.Drawing.Size(79, 15);
			this.labelSamplesPerPixel.TabIndex = 21;
			this.labelSamplesPerPixel.Text = "Rays Per Pixel";
			// 
			// sliderSamplesPerPixel
			// 
			this.sliderSamplesPerPixel.LargeChange = 50;
			this.sliderSamplesPerPixel.Location = new System.Drawing.Point(10, 32);
			this.sliderSamplesPerPixel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.sliderSamplesPerPixel.Maximum = 2048;
			this.sliderSamplesPerPixel.Minimum = 1;
			this.sliderSamplesPerPixel.Name = "sliderSamplesPerPixel";
			this.sliderSamplesPerPixel.Size = new System.Drawing.Size(226, 45);
			this.sliderSamplesPerPixel.TabIndex = 20;
			this.sliderSamplesPerPixel.TickFrequency = 128;
			this.sliderSamplesPerPixel.Value = 10;
			this.sliderSamplesPerPixel.Scroll += new System.EventHandler(this.sliderSamplesPerPixel_Scroll);
			// 
			// groupBoxCamera
			// 
			this.groupBoxCamera.Controls.Add(this.sliderFocalDistance);
			this.groupBoxCamera.Controls.Add(this.sliderAperture);
			this.groupBoxCamera.Controls.Add(this.sliderFieldOfView);
			this.groupBoxCamera.Controls.Add(this.labelFocalDistance);
			this.groupBoxCamera.Controls.Add(this.labelAperture);
			this.groupBoxCamera.Controls.Add(this.labelFieldOfView);
			this.groupBoxCamera.Location = new System.Drawing.Point(14, 299);
			this.groupBoxCamera.Name = "groupBoxCamera";
			this.groupBoxCamera.Size = new System.Drawing.Size(257, 227);
			this.groupBoxCamera.TabIndex = 23;
			this.groupBoxCamera.TabStop = false;
			this.groupBoxCamera.Text = "Camera Settings";
			// 
			// sliderFocalDistance
			// 
			this.sliderFocalDistance.Location = new System.Drawing.Point(17, 171);
			this.sliderFocalDistance.Maximum = 1000;
			this.sliderFocalDistance.Name = "sliderFocalDistance";
			this.sliderFocalDistance.Size = new System.Drawing.Size(223, 45);
			this.sliderFocalDistance.TabIndex = 5;
			this.sliderFocalDistance.TickFrequency = 100;
			this.sliderFocalDistance.Scroll += new System.EventHandler(this.sliderFocalDistance_Scroll);
			// 
			// sliderAperture
			// 
			this.sliderAperture.Location = new System.Drawing.Point(14, 105);
			this.sliderAperture.Maximum = 100;
			this.sliderAperture.Name = "sliderAperture";
			this.sliderAperture.Size = new System.Drawing.Size(226, 45);
			this.sliderAperture.TabIndex = 4;
			this.sliderAperture.TickFrequency = 10;
			this.sliderAperture.Scroll += new System.EventHandler(this.sliderAperture_Scroll);
			// 
			// sliderFieldOfView
			// 
			this.sliderFieldOfView.LargeChange = 15;
			this.sliderFieldOfView.Location = new System.Drawing.Point(14, 43);
			this.sliderFieldOfView.Maximum = 120;
			this.sliderFieldOfView.Minimum = 1;
			this.sliderFieldOfView.Name = "sliderFieldOfView";
			this.sliderFieldOfView.Size = new System.Drawing.Size(226, 45);
			this.sliderFieldOfView.TabIndex = 3;
			this.sliderFieldOfView.TickFrequency = 15;
			this.sliderFieldOfView.Value = 45;
			this.sliderFieldOfView.Scroll += new System.EventHandler(this.sliderFieldOfView_Scroll);
			// 
			// labelFocalDistance
			// 
			this.labelFocalDistance.AutoSize = true;
			this.labelFocalDistance.Location = new System.Drawing.Point(14, 153);
			this.labelFocalDistance.Name = "labelFocalDistance";
			this.labelFocalDistance.Size = new System.Drawing.Size(115, 15);
			this.labelFocalDistance.TabIndex = 2;
			this.labelFocalDistance.Text = "Focal Plane Distance";
			// 
			// labelAperture
			// 
			this.labelAperture.AutoSize = true;
			this.labelAperture.Location = new System.Drawing.Point(14, 87);
			this.labelAperture.Name = "labelAperture";
			this.labelAperture.Size = new System.Drawing.Size(53, 15);
			this.labelAperture.TabIndex = 1;
			this.labelAperture.Text = "Aperture";
			// 
			// labelFieldOfView
			// 
			this.labelFieldOfView.AutoSize = true;
			this.labelFieldOfView.Location = new System.Drawing.Point(14, 25);
			this.labelFieldOfView.Name = "labelFieldOfView";
			this.labelFieldOfView.Size = new System.Drawing.Size(74, 15);
			this.labelFieldOfView.TabIndex = 0;
			this.labelFieldOfView.Text = "Field of View";
			// 
			// MainForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new System.Drawing.Size(1183, 661);
			this.Controls.Add(this.groupBoxCamera);
			this.Controls.Add(this.tabTraceOptions);
			this.Controls.Add(this.checkProgressive);
			this.Controls.Add(this.buttonSave);
			this.Controls.Add(this.comboScene);
			this.Controls.Add(this.labelScene);
			this.Controls.Add(this.raytracingStatus);
			this.Controls.Add(this.raytracingDisplay);
			this.Controls.Add(this.buttonStartRaytrace);
			this.KeyPreview = true;
			this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.MinimumSize = new System.Drawing.Size(705, 700);
			this.Name = "MainForm";
			this.Text = "C# Path Tracer";
			this.Load += new System.EventHandler(this.MainForm_Load);
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyDown);
			this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyUp);
			this.Resize += new System.EventHandler(this.MainForm_Resize);
			this.raytracingStatus.ResumeLayout(false);
			this.raytracingStatus.PerformLayout();
			this.tabTraceOptions.ResumeLayout(false);
			this.tabPageRealTime.ResumeLayout(false);
			this.tabPageRealTime.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.sliderResReductionLive)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.sliderMaxRecursionLive)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.sliderSamplesPerPixelLive)).EndInit();
			this.tabPageFullTrace.ResumeLayout(false);
			this.tabPageFullTrace.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.sliderResReduction)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.sliderMaxRecursion)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.sliderSamplesPerPixel)).EndInit();
			this.groupBoxCamera.ResumeLayout(false);
			this.groupBoxCamera.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.sliderFocalDistance)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.sliderAperture)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.sliderFieldOfView)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.Button buttonStartRaytrace;
		private BitmapDisplay raytracingDisplay;
		private System.Windows.Forms.StatusStrip raytracingStatus;
		private System.Windows.Forms.ToolStripStatusLabel labelTotalRays;
		private System.Windows.Forms.ToolStripStatusLabel labelDeepestRecursion;
		private System.Windows.Forms.ToolStripStatusLabel labelTime;
		private System.Windows.Forms.ToolStripProgressBar progressRT;
		private System.Windows.Forms.ToolStripStatusLabel labelStatus;
		private System.Windows.Forms.Label labelScene;
		private System.Windows.Forms.ComboBox comboScene;
		private System.Windows.Forms.Timer timerFrameLoop;
		private System.Windows.Forms.Button buttonSave;
		private System.Windows.Forms.CheckBox checkProgressive;
		private System.Windows.Forms.TabControl tabTraceOptions;
		private System.Windows.Forms.TabPage tabPageRealTime;
		private System.Windows.Forms.Label labelResReductionLive;
		private System.Windows.Forms.TrackBar sliderResReductionLive;
		private System.Windows.Forms.Label labelMaxRecursionLive;
		private System.Windows.Forms.TrackBar sliderMaxRecursionLive;
		private System.Windows.Forms.Label labelSamplesPerPixelLive;
		private System.Windows.Forms.TrackBar sliderSamplesPerPixelLive;
		private System.Windows.Forms.TabPage tabPageFullTrace;
		private System.Windows.Forms.Label labelResReduction;
		private System.Windows.Forms.TrackBar sliderResReduction;
		private System.Windows.Forms.Label labelMaxRecursion;
		private System.Windows.Forms.TrackBar sliderMaxRecursion;
		private System.Windows.Forms.Label labelSamplesPerPixel;
		private System.Windows.Forms.TrackBar sliderSamplesPerPixel;
		private System.Windows.Forms.ToolStripStatusLabel labelDimensions;
		private System.Windows.Forms.GroupBox groupBoxCamera;
		private System.Windows.Forms.TrackBar sliderFocalDistance;
		private System.Windows.Forms.TrackBar sliderAperture;
		private System.Windows.Forms.TrackBar sliderFieldOfView;
		private System.Windows.Forms.Label labelFocalDistance;
		private System.Windows.Forms.Label labelAperture;
		private System.Windows.Forms.Label labelFieldOfView;
	}
}