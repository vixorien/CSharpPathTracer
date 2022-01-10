using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace CSharpPathTracer
{
	/// <summary>
	/// A basic control that displays a bitmap
	/// </summary>
	public partial class BitmapDisplay : Control
	{
		/// <summary>
		/// Gets or sets the bitmap to display
		/// </summary>
		public Bitmap Bitmap { get; set; }

		/// <summary>
		/// Gets the aspect ratio of this display
		/// </summary>
		public float AspectRatio { get { return (float)Width / Height; } }

		/// <summary>
		/// Creates the basic display
		/// </summary>
		public BitmapDisplay()
		{
			InitializeComponent();
			this.DoubleBuffered = true;
		}

		/// <summary>
		/// Handles drawing the bitmap whenever a paint is requested
		/// </summary>
		/// <param name="pe"></param>
		protected override void OnPaint(PaintEventArgs pe)
		{
			// If we have a bitmap, render!
			if (Bitmap != null)
			{
				// Some details for performance found here: https://stackoverflow.com/questions/11020710/is-graphics-drawimage-too-slow-for-bigger-images
				pe.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
				pe.Graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
				pe.Graphics.DrawImage(Bitmap, this.ClientRectangle);
			}

			base.OnPaint(pe);
		}
	}
}
