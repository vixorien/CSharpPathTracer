
#define USE_MONOGAME

using System;
using System.Windows.Forms;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace CSharpPathTracer
{
	public enum PathTracerUI
	{
		WindowsForms,
		MonoGame
	}

	public class Program
	{
		/// <summary>
		/// Gets or sets the UI mode of the path tracer app.  Setting this
		/// is only valid at application startup.
		/// </summary>
		public static PathTracerUI UIMode { get; set; } = PathTracerUI.WindowsForms;

		[STAThread]
		static void Main()
		{
			// Set app options (for windows forms)
			Application.SetHighDpiMode(HighDpiMode.SystemAware);
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			// Show the mode selection form
			Application.Run(new ModeSelectionForm());

			// Which mode?
			switch (UIMode)
			{
				case PathTracerUI.MonoGame:

					// Start as a monogame application
					using (GamePathTracer game = new GamePathTracer())
						game.Run();
					
					break;

				case PathTracerUI.WindowsForms:

					// Start as a windows forms app
					Application.Run(new MainForm());
					break;
			}
		}
	}
}
