
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
			this.buttonStartRaytrace = new System.Windows.Forms.Button();
			this.sliderSamplesPerPixel = new System.Windows.Forms.TrackBar();
			this.labelSamplesPerPixel = new System.Windows.Forms.Label();
			this.labelMaxRecursion = new System.Windows.Forms.Label();
			this.sliderMaxRecursion = new System.Windows.Forms.TrackBar();
			this.raytracingDisplay = new CSharpPathTracer.BitmapDisplay();
			this.labelWidth = new System.Windows.Forms.Label();
			this.labelHeight = new System.Windows.Forms.Label();
			this.checkDisplayProgress = new System.Windows.Forms.CheckBox();
			this.raytracingStatus = new System.Windows.Forms.StatusStrip();
			this.labelStatus = new System.Windows.Forms.ToolStripStatusLabel();
			this.progressRT = new System.Windows.Forms.ToolStripProgressBar();
			this.labelTotalRays = new System.Windows.Forms.ToolStripStatusLabel();
			this.labelDeepestRecursion = new System.Windows.Forms.ToolStripStatusLabel();
			this.labelTime = new System.Windows.Forms.ToolStripStatusLabel();
			this.textWidth = new System.Windows.Forms.TextBox();
			this.textHeight = new System.Windows.Forms.TextBox();
			this.labelScene = new System.Windows.Forms.Label();
			this.comboScene = new System.Windows.Forms.ComboBox();
			this.labelResReduction = new System.Windows.Forms.Label();
			this.sliderResReduction = new System.Windows.Forms.TrackBar();
			((System.ComponentModel.ISupportInitialize)(this.sliderSamplesPerPixel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.sliderMaxRecursion)).BeginInit();
			this.raytracingStatus.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.sliderResReduction)).BeginInit();
			this.SuspendLayout();
			// 
			// buttonStartRaytrace
			// 
			this.buttonStartRaytrace.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonStartRaytrace.Location = new System.Drawing.Point(12, 429);
			this.buttonStartRaytrace.Name = "buttonStartRaytrace";
			this.buttonStartRaytrace.Size = new System.Drawing.Size(189, 46);
			this.buttonStartRaytrace.TabIndex = 1;
			this.buttonStartRaytrace.Text = "Start Raytrace";
			this.buttonStartRaytrace.UseVisualStyleBackColor = true;
			this.buttonStartRaytrace.Click += new System.EventHandler(this.buttonStartRaytrace_Click);
			// 
			// sliderSamplesPerPixel
			// 
			this.sliderSamplesPerPixel.LargeChange = 128;
			this.sliderSamplesPerPixel.Location = new System.Drawing.Point(12, 82);
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
			this.labelSamplesPerPixel.Location = new System.Drawing.Point(12, 64);
			this.labelSamplesPerPixel.Name = "labelSamplesPerPixel";
			this.labelSamplesPerPixel.Size = new System.Drawing.Size(98, 15);
			this.labelSamplesPerPixel.TabIndex = 4;
			this.labelSamplesPerPixel.Text = "Samples Per Pixel";
			// 
			// labelMaxRecursion
			// 
			this.labelMaxRecursion.AutoSize = true;
			this.labelMaxRecursion.Location = new System.Drawing.Point(12, 130);
			this.labelMaxRecursion.Name = "labelMaxRecursion";
			this.labelMaxRecursion.Size = new System.Drawing.Size(151, 15);
			this.labelMaxRecursion.TabIndex = 6;
			this.labelMaxRecursion.Text = "Maximum Recursion Depth";
			// 
			// sliderMaxRecursion
			// 
			this.sliderMaxRecursion.Location = new System.Drawing.Point(12, 148);
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
			this.raytracingDisplay.Size = new System.Drawing.Size(609, 463);
			this.raytracingDisplay.TabIndex = 7;
			this.raytracingDisplay.Text = "renderTarget1";
			// 
			// labelWidth
			// 
			this.labelWidth.AutoSize = true;
			this.labelWidth.Location = new System.Drawing.Point(12, 264);
			this.labelWidth.Name = "labelWidth";
			this.labelWidth.Size = new System.Drawing.Size(80, 15);
			this.labelWidth.TabIndex = 8;
			this.labelWidth.Text = "Output Width";
			// 
			// labelHeight
			// 
			this.labelHeight.AutoSize = true;
			this.labelHeight.Location = new System.Drawing.Point(110, 264);
			this.labelHeight.Name = "labelHeight";
			this.labelHeight.Size = new System.Drawing.Size(84, 15);
			this.labelHeight.TabIndex = 9;
			this.labelHeight.Text = "Output Height";
			// 
			// checkDisplayProgress
			// 
			this.checkDisplayProgress.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.checkDisplayProgress.AutoSize = true;
			this.checkDisplayProgress.Checked = true;
			this.checkDisplayProgress.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkDisplayProgress.Location = new System.Drawing.Point(19, 404);
			this.checkDisplayProgress.Name = "checkDisplayProgress";
			this.checkDisplayProgress.Size = new System.Drawing.Size(171, 19);
			this.checkDisplayProgress.TabIndex = 12;
			this.checkDisplayProgress.Text = "Display Raytracing Progress";
			this.checkDisplayProgress.UseVisualStyleBackColor = true;
			// 
			// raytracingStatus
			// 
			this.raytracingStatus.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.labelStatus,
            this.progressRT,
            this.labelTotalRays,
            this.labelDeepestRecursion,
            this.labelTime});
			this.raytracingStatus.Location = new System.Drawing.Point(0, 485);
			this.raytracingStatus.Name = "raytracingStatus";
			this.raytracingStatus.Size = new System.Drawing.Size(828, 22);
			this.raytracingStatus.TabIndex = 13;
			this.raytracingStatus.Text = "statusStrip1";
			// 
			// labelStatus
			// 
			this.labelStatus.Name = "labelStatus";
			this.labelStatus.Size = new System.Drawing.Size(152, 17);
			this.labelStatus.Spring = true;
			this.labelStatus.Text = "Status: Waiting";
			this.labelStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// progressRT
			// 
			this.progressRT.Name = "progressRT";
			this.progressRT.Size = new System.Drawing.Size(200, 16);
			this.progressRT.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
			// 
			// labelTotalRays
			// 
			this.labelTotalRays.Name = "labelTotalRays";
			this.labelTotalRays.Size = new System.Drawing.Size(152, 17);
			this.labelTotalRays.Spring = true;
			this.labelTotalRays.Text = "Total Rays: ?";
			this.labelTotalRays.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelDeepestRecursion
			// 
			this.labelDeepestRecursion.Name = "labelDeepestRecursion";
			this.labelDeepestRecursion.Size = new System.Drawing.Size(152, 17);
			this.labelDeepestRecursion.Spring = true;
			this.labelDeepestRecursion.Text = "Deepest Recursion: ?";
			this.labelDeepestRecursion.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelTime
			// 
			this.labelTime.Name = "labelTime";
			this.labelTime.Size = new System.Drawing.Size(152, 17);
			this.labelTime.Spring = true;
			this.labelTime.Text = "Total Time: ?";
			this.labelTime.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// textWidth
			// 
			this.textWidth.Location = new System.Drawing.Point(10, 285);
			this.textWidth.Name = "textWidth";
			this.textWidth.ReadOnly = true;
			this.textWidth.Size = new System.Drawing.Size(82, 23);
			this.textWidth.TabIndex = 14;
			this.textWidth.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// textHeight
			// 
			this.textHeight.Location = new System.Drawing.Point(110, 285);
			this.textHeight.Name = "textHeight";
			this.textHeight.ReadOnly = true;
			this.textHeight.Size = new System.Drawing.Size(82, 23);
			this.textHeight.TabIndex = 15;
			this.textHeight.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// labelScene
			// 
			this.labelScene.AutoSize = true;
			this.labelScene.Location = new System.Drawing.Point(12, 9);
			this.labelScene.Name = "labelScene";
			this.labelScene.Size = new System.Drawing.Size(38, 15);
			this.labelScene.TabIndex = 16;
			this.labelScene.Text = "Scene";
			// 
			// comboScene
			// 
			this.comboScene.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboScene.FormattingEnabled = true;
			this.comboScene.Location = new System.Drawing.Point(12, 27);
			this.comboScene.Name = "comboScene";
			this.comboScene.Size = new System.Drawing.Size(182, 23);
			this.comboScene.TabIndex = 17;
			// 
			// labelResReduction
			// 
			this.labelResReduction.AutoSize = true;
			this.labelResReduction.Location = new System.Drawing.Point(12, 195);
			this.labelResReduction.Name = "labelResReduction";
			this.labelResReduction.Size = new System.Drawing.Size(120, 15);
			this.labelResReduction.TabIndex = 19;
			this.labelResReduction.Text = "Resolution Reduction";
			// 
			// sliderResReduction
			// 
			this.sliderResReduction.Location = new System.Drawing.Point(12, 213);
			this.sliderResReduction.Maximum = 16;
			this.sliderResReduction.Minimum = 1;
			this.sliderResReduction.Name = "sliderResReduction";
			this.sliderResReduction.Size = new System.Drawing.Size(189, 45);
			this.sliderResReduction.TabIndex = 18;
			this.sliderResReduction.TickFrequency = 5;
			this.sliderResReduction.Value = 1;
			this.sliderResReduction.Scroll += new System.EventHandler(this.sliderResReduction_Scroll);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(828, 507);
			this.Controls.Add(this.labelResReduction);
			this.Controls.Add(this.sliderResReduction);
			this.Controls.Add(this.comboScene);
			this.Controls.Add(this.labelScene);
			this.Controls.Add(this.textHeight);
			this.Controls.Add(this.textWidth);
			this.Controls.Add(this.raytracingStatus);
			this.Controls.Add(this.checkDisplayProgress);
			this.Controls.Add(this.labelHeight);
			this.Controls.Add(this.labelWidth);
			this.Controls.Add(this.raytracingDisplay);
			this.Controls.Add(this.labelMaxRecursion);
			this.Controls.Add(this.sliderMaxRecursion);
			this.Controls.Add(this.labelSamplesPerPixel);
			this.Controls.Add(this.sliderSamplesPerPixel);
			this.Controls.Add(this.buttonStartRaytrace);
			this.MinimumSize = new System.Drawing.Size(500, 400);
			this.Name = "MainForm";
			this.Text = "C# Path Tracer";
			this.Load += new System.EventHandler(this.MainForm_Load);
			this.Resize += new System.EventHandler(this.MainForm_Resize);
			((System.ComponentModel.ISupportInitialize)(this.sliderSamplesPerPixel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.sliderMaxRecursion)).EndInit();
			this.raytracingStatus.ResumeLayout(false);
			this.raytracingStatus.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.sliderResReduction)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.Button buttonStartRaytrace;
		private System.Windows.Forms.TrackBar sliderSamplesPerPixel;
		private System.Windows.Forms.Label labelSamplesPerPixel;
		private System.Windows.Forms.Label labelMaxRecursion;
		private System.Windows.Forms.TrackBar sliderMaxRecursion;
		private BitmapDisplay raytracingDisplay;
		private System.Windows.Forms.Label labelWidth;
		private System.Windows.Forms.Label labelHeight;
		private System.Windows.Forms.CheckBox checkDisplayProgress;
		private System.Windows.Forms.StatusStrip raytracingStatus;
		private System.Windows.Forms.ToolStripStatusLabel labelTotalRays;
		private System.Windows.Forms.ToolStripStatusLabel labelDeepestRecursion;
		private System.Windows.Forms.ToolStripStatusLabel labelTime;
		private System.Windows.Forms.ToolStripProgressBar progressRT;
		private System.Windows.Forms.ToolStripStatusLabel labelStatus;
		private System.Windows.Forms.TextBox textWidth;
		private System.Windows.Forms.TextBox textHeight;
		private System.Windows.Forms.Label labelScene;
		private System.Windows.Forms.ComboBox comboScene;
		private System.Windows.Forms.Label labelResReduction;
		private System.Windows.Forms.TrackBar sliderResReduction;
	}
}