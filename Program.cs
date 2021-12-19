using System;
using System.Windows.Forms;

namespace CSharpRaytracing
{
	public class Program
	{
		[STAThread]
		static void Main()
		{
			string file = "../../../image.ppm";

			//Raytracer rt = new Raytracer(400, 300);
			//rt.RaytraceScene();

			//ImageWriter.WritePPMImageFile(file, rt.Pixels);

			Application.SetHighDpiMode(HighDpiMode.SystemAware);
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainForm());
		}
	}
}
