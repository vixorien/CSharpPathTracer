
#define USE_MONOGAME

using System;
using System.Windows.Forms;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace CSharpPathTracer
{
	public class Program
	{
		[STAThread]
		static void Main()
		{
#if USE_MONOGAME
			// Start as a monogame application
			using (GamePathTracer game = new GamePathTracer())
				game.Run();
#else
			// Start as a windows forms app
			Application.SetHighDpiMode(HighDpiMode.SystemAware);
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainForm());
#endif
		}
	}
}
