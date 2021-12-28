using System;
using System.Collections.Generic;
using System.Text;

using System.Drawing;

namespace CSharpPathTracer
{
	class Texture
	{
		

		public Texture(string filepath)
		{
			Bitmap bitmap = new Bitmap(filepath);
			
			// TODO:
			// - Lock
			// - Copy bits out
			// - Convert to actual colors?
			// - Unlock
			// - Dispose the bitmap to release the file
		}

	}
}
