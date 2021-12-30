using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace CSharpPathTracer
{
	/// <summary>
	/// Base class for all environments
	/// </summary>
	abstract class Environment
	{
		public abstract Vector3 GetColorFromDirection(Vector3 direction);
	}

	/// <summary>
	/// An environment that is a solid color in all directions
	/// </summary>
	class EnvironmentSolid : Environment
	{
		public Vector3 Color { get; }

		public EnvironmentSolid(Vector3 color)
		{
			Color = color;
		}

		public override Vector3 GetColorFromDirection(Vector3 direction)
		{
			return Color;
		}
	}

	/// <summary>
	/// An environment represented by a 3-point gradient
	/// </summary>
	class EnvironmentGradient : Environment
	{
		public Vector3 ColorUp { get; }
		public Vector3 ColorForward { get; }
		public Vector3 ColorDown { get; }

		public EnvironmentGradient(Vector3 colorUp, Vector3 colorForward, Vector3 colorDown)
		{
			ColorUp = colorUp;
			ColorForward = colorForward;
			ColorDown = colorDown;
		}

		public override Vector3 GetColorFromDirection(Vector3 direction)
		{
			// Normalize and grab the dot product and then get the actual angle
			direction.Normalize();
			float dir = MathF.Acos(Vector3.Dot(Vector3.Up, direction)) / MathF.PI * -2 + 1;

			// Interpolate and return the gradient result
			if (dir > 0.0f)
			{
				return Vector3.Lerp(ColorForward, ColorUp, dir);
			}
			else
			{
				return Vector3.Lerp(ColorForward, ColorDown, dir * -1);
			}
		}
	}

	/// <summary>
	/// An environment that uses a skybox of 6 textures
	/// </summary>
	class EnvironmentSkybox : Environment
	{
		public Texture Right { get; }
		public Texture Left { get; }
		public Texture Up { get; }
		public Texture Down { get; }
		public Texture Back { get; }
		public Texture Forward { get; }

		private Texture[] textures;


		public EnvironmentSkybox(Texture right, Texture left, Texture up, Texture down, Texture back, Texture forward)
		{
			Right = right;
			Left = left;
			Up = up;
			Down = down;
			Back = back;
			Forward = forward;

			// Array for easy sampling later
			textures = new Texture[]{ 
				Right,
				Left,
				Up,
				Down,
				Back,
				Forward
			};
		}


		//https://www.gamedev.net/forums/topic/687535-implementing-a-cube-map-lookup-function/5337472/
		public override Vector3 GetColorFromDirection(Vector3 direction)
		{
			// Which texture?
			Texture face = null;
			int faceIndex = -1;
			float mag;
			Vector3 absDir = direction.Abs();
			Vector2 uv;

			if (absDir.X >= absDir.Y && absDir.X >= absDir.Z)
			{
				// Left or right
				if (direction.X >= 0)
				{
					faceIndex = 0;
					uv = new Vector2(-direction.Z, -direction.Y);
				}
				else
				{
					faceIndex = 1;
					uv = new Vector2(direction.Z, -direction.Y);
				}

				mag = 0.5f / absDir.X;
			}
			else if (absDir.Y >= absDir.X && absDir.Y >= absDir.Z)
			{
				// Up or down
				if (direction.Y >= 0)
				{
					faceIndex = 2;
					uv = new Vector2(direction.X, direction.Z);
				}
				else
				{
					faceIndex = 3;
					uv = new Vector2(direction.X, -direction.Z);
				}

				mag = 0.5f / absDir.Y;
			}
			else
			{
				// Forward or back
				if (direction.Z >= 0)
				{
					faceIndex = 5;
					uv = new Vector2(direction.X, -direction.Y);
				}
				else
				{
					faceIndex = 4;
					uv = new Vector2(-direction.X, -direction.Y);
				}

				mag = 0.5f / absDir.Z;
			}

			// Now that we know the face, convert to a UV
			uv = uv * mag + new Vector2(0.5f, 0.5f);

			// Sample
			return textures[faceIndex].Sample(uv, Texture.AddressMode.Clamp).ToVector3();
		}
	}
}
