using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CSharpPathTracer
{
	class Material
	{
		public Vector3 Color { get; set; }
		public Texture Texture { get; set; }
		public bool Metal { get; set; }
		public bool Transparent { get; set; }
		public float IndexOfRefraction { get; set; }

		public Material(Vector3 color, bool metal, bool transparent = false, float indexOfRefraction = 1.0f)
			: this(color, null, metal, transparent, indexOfRefraction)
		{
		}

		public Material(Vector3 color, Texture texture, bool metal, bool transparent = false, float indexOfRefraction = 1.0f)
		{
			Color = color;
			Texture = texture;
			Metal = metal;
			Transparent = transparent;
			IndexOfRefraction = indexOfRefraction;
		}

		public Vector3 GetColorAtUV(Vector2 uv)
		{
			if (Texture == null)
				return Color;

			return Texture.Sample(uv).ToVector3() * Color;
		}
	}
}
