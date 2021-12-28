using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CSharpPathTracer
{
	class Material
	{
		public Vector3 Color { get; set;  }
		public bool Metal { get; set; }
		public bool Transparent { get; set; }
		public float IndexOfRefraction { get; set; }

		public Material(Vector3 color, bool metal, bool transparent = false, float indexOfRefraction = 1.0f)
		{
			Color = color;
			Metal = metal;
			Transparent = transparent;
			IndexOfRefraction = indexOfRefraction;
		}
	}
}
