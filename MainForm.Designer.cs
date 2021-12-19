
namespace CSharpRaytracing
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
			this.buttonStartRaytrace = new System.Windows.Forms.Button();
			this.progressRaytrace = new System.Windows.Forms.ProgressBar();
			this.sliderSamplesPerPixel = new System.Windows.Forms.TrackBar();
			this.labelSamplesPerPixel = new System.Windows.Forms.Label();
			this.labelMaxRecursion = new System.Windows.Forms.Label();
			this.sliderMaxRecursion = new System.Windows.Forms.TrackBar();
			this.raytracingDisplay = new CSharpRaytracing.BitmapDisplay();
			this.labelWidth = new System.Windows.Forms.Label();
			this.labelHeight = new System.Windows.Forms.Label();
			this.numWidth = new System.Windows.Forms.NumericUpDown();
			this.numHeight = new System.Windows.Forms.NumericUpDown();
			this.checkDisplayProgress = new System.Windows.Forms.CheckBox();
			((System.ComponentModel.ISupportInitialize)(this.sliderSamplesPerPixel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.sliderMaxRecursion)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numWidth)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numHeight)).BeginInit();
			this.SuspendLayout();
			// 
			// buttonStartRaytrace
			// 
			this.buttonStartRaytrace.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonStartRaytrace.Location = new System.Drawing.Point(12, 417);
			this.buttonStartRaytrace.Name = "buttonStartRaytrace";
			this.buttonStartRaytrace.Size = new System.Drawing.Size(189, 46);
			this.buttonStartRaytrace.TabIndex = 1;
			this.buttonStartRaytrace.Text = "Start Raytrace";
			this.buttonStartRaytrace.UseVisualStyleBackColor = true;
			this.buttonStartRaytrace.Click += new System.EventHandler(this.buttonStartRaytrace_Click);
			// 
			// progressRaytrace
			// 
			this.progressRaytrace.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.progressRaytrace.Location = new System.Drawing.Point(207, 440);
			this.progressRaytrace.Name = "progressRaytrace";
			this.progressRaytrace.Size = new System.Drawing.Size(609, 23);
			this.progressRaytrace.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
			this.progressRaytrace.TabIndex = 2;
			// 
			// sliderSamplesPerPixel
			// 
			this.sliderSamplesPerPixel.LargeChange = 128;
			this.sliderSamplesPerPixel.Location = new System.Drawing.Point(12, 30);
			this.sliderSamplesPerPixel.Maximum = 2048;
			this.sliderSamplesPerPixel.Minimum = 1;
			this.sliderSamplesPerPixel.Name = "sliderSamplesPerPixel";
			this.sliderSamplesPerPixel.Size = new System.Drawing.Size(189, 45);
			this.sliderSamplesPerPixel.TabIndex = 3;
			this.sliderSamplesPerPixel.TickFrequency = 128;
			this.sliderSamplesPerPixel.Value = 10;
			this.sliderSamplesPerPixel.Scroll += new System.EventHandler(this.sliderSamplesPerPixel_Scroll);
			// 
			// labelSamplesPerPixel
			// 
			this.labelSamplesPerPixel.AutoSize = true;
			this.labelSamplesPerPixel.Location = new System.Drawing.Point(12, 12);
			this.labelSamplesPerPixel.Name = "labelSamplesPerPixel";
			this.labelSamplesPerPixel.Size = new System.Drawing.Size(99, 15);
			this.labelSamplesPerPixel.TabIndex = 4;
			this.labelSamplesPerPixel.Text = "Samples Per Pixel";
			// 
			// labelMaxRecursion
			// 
			this.labelMaxRecursion.AutoSize = true;
			this.labelMaxRecursion.Location = new System.Drawing.Point(12, 78);
			this.labelMaxRecursion.Name = "labelMaxRecursion";
			this.labelMaxRecursion.Size = new System.Drawing.Size(152, 15);
			this.labelMaxRecursion.TabIndex = 6;
			this.labelMaxRecursion.Text = "Maximum Recursion Depth";
			// 
			// sliderMaxRecursion
			// 
			this.sliderMaxRecursion.Location = new System.Drawing.Point(12, 96);
			this.sliderMaxRecursion.Maximum = 50;
			this.sliderMaxRecursion.Minimum = 1;
			this.sliderMaxRecursion.Name = "sliderMaxRecursion";
			this.sliderMaxRecursion.Size = new System.Drawing.Size(189, 45);
			this.sliderMaxRecursion.TabIndex = 5;
			this.sliderMaxRecursion.TickFrequency = 5;
			this.sliderMaxRecursion.Value = 25;
			this.sliderMaxRecursion.Scroll += new System.EventHandler(this.sliderMaxRecursion_Scroll);
			// 
			// raytracingDisplay
			// 
			this.raytracingDisplay.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.raytracingDisplay.BackColor = System.Drawing.SystemColors.ControlDark;
			this.raytracingDisplay.Bitmap = null;
			this.raytracingDisplay.Location = new System.Drawing.Point(207, 12);
			this.raytracingDisplay.Name = "raytracingDisplay";
			this.raytracingDisplay.Size = new System.Drawing.Size(609, 422);
			this.raytracingDisplay.TabIndex = 7;
			this.raytracingDisplay.Text = "renderTarget1";
			// 
			// labelWidth
			// 
			this.labelWidth.AutoSize = true;
			this.labelWidth.Location = new System.Drawing.Point(12, 144);
			this.labelWidth.Name = "labelWidth";
			this.labelWidth.Size = new System.Drawing.Size(80, 15);
			this.labelWidth.TabIndex = 8;
			this.labelWidth.Text = "Output Width";
			// 
			// labelHeight
			// 
			this.labelHeight.AutoSize = true;
			this.labelHeight.Location = new System.Drawing.Point(110, 144);
			this.labelHeight.Name = "labelHeight";
			this.labelHeight.Size = new System.Drawing.Size(84, 15);
			this.labelHeight.TabIndex = 9;
			this.labelHeight.Text = "Output Height";
			// 
			// numWidth
			// 
			this.numWidth.Location = new System.Drawing.Point(12, 162);
			this.numWidth.Maximum = new decimal(new int[] {
            4096,
            0,
            0,
            0});
			this.numWidth.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
			this.numWidth.Name = "numWidth";
			this.numWidth.ReadOnly = true;
			this.numWidth.Size = new System.Drawing.Size(80, 23);
			this.numWidth.TabIndex = 10;
			this.numWidth.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
			// 
			// numHeight
			// 
			this.numHeight.Location = new System.Drawing.Point(110, 162);
			this.numHeight.Maximum = new decimal(new int[] {
            4096,
            0,
            0,
            0});
			this.numHeight.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
			this.numHeight.Name = "numHeight";
			this.numHeight.ReadOnly = true;
			this.numHeight.Size = new System.Drawing.Size(80, 23);
			this.numHeight.TabIndex = 11;
			this.numHeight.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
			// 
			// checkDisplayProgress
			// 
			this.checkDisplayProgress.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.checkDisplayProgress.AutoSize = true;
			this.checkDisplayProgress.Checked = true;
			this.checkDisplayProgress.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkDisplayProgress.Location = new System.Drawing.Point(19, 392);
			this.checkDisplayProgress.Name = "checkDisplayProgress";
			this.checkDisplayProgress.Size = new System.Drawing.Size(171, 19);
			this.checkDisplayProgress.TabIndex = 12;
			this.checkDisplayProgress.Text = "Display Raytracing Progress";
			this.checkDisplayProgress.UseVisualStyleBackColor = true;
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(828, 475);
			this.Controls.Add(this.checkDisplayProgress);
			this.Controls.Add(this.numHeight);
			this.Controls.Add(this.numWidth);
			this.Controls.Add(this.labelHeight);
			this.Controls.Add(this.labelWidth);
			this.Controls.Add(this.raytracingDisplay);
			this.Controls.Add(this.labelMaxRecursion);
			this.Controls.Add(this.sliderMaxRecursion);
			this.Controls.Add(this.labelSamplesPerPixel);
			this.Controls.Add(this.sliderSamplesPerPixel);
			this.Controls.Add(this.progressRaytrace);
			this.Controls.Add(this.buttonStartRaytrace);
			this.MinimumSize = new System.Drawing.Size(500, 400);
			this.Name = "MainForm";
			this.Text = "C# Path Tracer";
			this.Load += new System.EventHandler(this.MainForm_Load);
			this.Resize += new System.EventHandler(this.MainForm_Resize);
			((System.ComponentModel.ISupportInitialize)(this.sliderSamplesPerPixel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.sliderMaxRecursion)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numWidth)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numHeight)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.Button buttonStartRaytrace;
		private System.Windows.Forms.ProgressBar progressRaytrace;
		private System.Windows.Forms.TrackBar sliderSamplesPerPixel;
		private System.Windows.Forms.Label labelSamplesPerPixel;
		private System.Windows.Forms.Label labelMaxRecursion;
		private System.Windows.Forms.TrackBar sliderMaxRecursion;
		private BitmapDisplay raytracingDisplay;
		private System.Windows.Forms.Label labelWidth;
		private System.Windows.Forms.Label labelHeight;
		private System.Windows.Forms.NumericUpDown numWidth;
		private System.Windows.Forms.NumericUpDown numHeight;
		private System.Windows.Forms.CheckBox checkDisplayProgress;
	}
}