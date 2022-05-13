using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace CSharpPathTracer
{
	public partial class ModeSelectionForm : Form
	{
		public ModeSelectionForm()
		{
			InitializeComponent();
		}

		private void buttonWinForms_Click(object sender, EventArgs e)
		{
			Program.UIMode = PathTracerUI.WindowsForms;
			this.Close();
		}

		private void buttonMonoGame_Click(object sender, EventArgs e)
		{
			Program.UIMode = PathTracerUI.MonoGame;
			this.Close();
		}
	}
}
