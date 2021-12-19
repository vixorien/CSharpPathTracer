using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace CSharpRaytracing
{
	public partial class BitmapDisplay : Control
	{
		public Bitmap Bitmap { get; set; }
		public float AspectRatio { get { return (float)Width / Height; } }

		public BitmapDisplay()
		{
			InitializeComponent();

			this.DoubleBuffered = true;
		}

		protected override void OnPaint(PaintEventArgs pe)
		{
			// If we have a bitmap, render!
			if (Bitmap != null)
			{
				// Some details for performance found here: https://stackoverflow.com/questions/11020710/is-graphics-drawimage-too-slow-for-bigger-images
				pe.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Bilinear;
				pe.Graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
				pe.Graphics.DrawImage(Bitmap, this.ClientRectangle);
			}

			base.OnPaint(pe);
		}
	}
}
