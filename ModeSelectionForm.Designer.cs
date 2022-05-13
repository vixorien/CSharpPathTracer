
namespace CSharpPathTracer
{
	partial class ModeSelectionForm
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
			this.buttonWinForms = new System.Windows.Forms.Button();
			this.buttonMonoGame = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// buttonWinForms
			// 
			this.buttonWinForms.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.buttonWinForms.Location = new System.Drawing.Point(12, 12);
			this.buttonWinForms.Name = "buttonWinForms";
			this.buttonWinForms.Size = new System.Drawing.Size(160, 157);
			this.buttonWinForms.TabIndex = 0;
			this.buttonWinForms.Text = "Windows Forms";
			this.buttonWinForms.UseVisualStyleBackColor = true;
			this.buttonWinForms.Click += new System.EventHandler(this.buttonWinForms_Click);
			// 
			// buttonMonoGame
			// 
			this.buttonMonoGame.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.buttonMonoGame.Location = new System.Drawing.Point(178, 12);
			this.buttonMonoGame.Name = "buttonMonoGame";
			this.buttonMonoGame.Size = new System.Drawing.Size(160, 157);
			this.buttonMonoGame.TabIndex = 1;
			this.buttonMonoGame.Text = "MonoGame";
			this.buttonMonoGame.UseVisualStyleBackColor = true;
			this.buttonMonoGame.Click += new System.EventHandler(this.buttonMonoGame_Click);
			// 
			// ModeSelectionForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new System.Drawing.Size(350, 181);
			this.ControlBox = false;
			this.Controls.Add(this.buttonMonoGame);
			this.Controls.Add(this.buttonWinForms);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.Name = "ModeSelectionForm";
			this.Text = "C# Path Tracer Mode Selection";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button buttonWinForms;
		private System.Windows.Forms.Button buttonMonoGame;
	}
}