using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CSharpPathTracer
{
	class Material
	{
		public Vector3 Color { get; }
		public bool Metal { get; }
		public bool Transparent { get; }

		public Material(Vector3 color, bool metal, bool transparent)
		{
			Color = color;
			Metal = metal;
			Transparent = transparent;
		}

		public Material(Vector3 color, bool metal)
		{
			Color = color;
			Metal = metal;
			Transparent = false;
		}
	}
}
