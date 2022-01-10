using System;
using System.Windows.Forms;

namespace CSharpPathTracer
{
	public class Program
	{
		[STAThread]
		static void Main()
		{
			// Start as a windows forms app
			Application.SetHighDpiMode(HighDpiMode.SystemAware);
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainForm());
		}
	}
}
