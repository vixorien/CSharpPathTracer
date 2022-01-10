using System;
using System.Numerics;

namespace CSharpPathTracer
{
	/// <summary>
	/// Base class for all environments
	/// </summary>
	abstract class Environment
	{
		/// <summary>
		/// Returns a color from the given direction
		/// </summary>
		/// <param name="direction">The direction in which we're looking for a color</param>
		/// <returns>The color as a vector3</returns>
		public abstract Vector3 GetColorFromDirection(Vector3 direction);
	}

	/// <summary>
	/// An environment that is a solid color in all directions
	/// </summary>
	class EnvironmentSolid : Environment
	{
		/// <summary>
		/// Gets or sets the environment's color
		/// </summary>
		public Vector3 Color { get; set; }

		/// <summary>
		/// Creates a solid color environment
		/// </summary>
		/// <param name="color">The color of the environment</param>
		public EnvironmentSolid(Vector3 color)
		{
			Color = color;
		}

		/// <summary>
		/// Returns a color from the given direction.  In this case, it's always the same color
		/// </summary>
		/// <param name="direction">The direction in which we're looking for a color</param>
		/// <returns>The color as a vector3</returns>
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
		/// <summary>
		/// Gets or sets the color along the positive Y axis
		/// </summary>
		public Vector3 ColorUp { get; set; }

		/// <summary>
		/// Gets or sets the color along the X/Z plane
		/// </summary>
		public Vector3 ColorForward { get; set; }

		/// <summary>
		/// Gets or sets the color along the negative Y axis
		/// </summary>
		public Vector3 ColorDown { get; set; }

		/// <summary>
		/// Creates a new gradient environment given three colors
		/// </summary>
		/// <param name="colorUp">Positive Y axis color</param>
		/// <param name="colorForward">XZ plane color</param>
		/// <param name="colorDown">Negative Y axis color</param>
		public EnvironmentGradient(Vector3 colorUp, Vector3 colorForward, Vector3 colorDown)
		{
			ColorUp = colorUp;
			ColorForward = colorForward;
			ColorDown = colorDown;
		}

		/// <summary>
		/// Returns a color from the given direction.
		/// </summary>
		/// <param name="direction">The direction in which we're looking for a color</param>
		/// <returns>The color as a vector3</returns>
		public override Vector3 GetColorFromDirection(Vector3 direction)
		{
			// Normalize and grab the dot product and then get the actual angle
			float dir = MathF.Acos(Vector3.Dot(Vector3.UnitY, Vector3.Normalize(direction))) / MathF.PI * -2 + 1;

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
		/// <summary>
		/// Gets or sets the texture to the right
		/// </summary>
		public Texture Right { get; set; }

		/// <summary>
		/// Gets or sets the texture to the left
		/// </summary>
		public Texture Left { get; set; }

		/// <summary>
		/// Gets or sets the texture above
		/// </summary>
		public Texture Up { get; set; }
		
		/// <summary>
		/// Gets or sets the texture below
		/// </summary>
		public Texture Down { get; set; }

		/// <summary>
		/// Gets or sets the texture backwards
		/// </summary>
		public Texture Back { get; set; }

		/// <summary>
		/// Gets or sets the texture forward
		/// </summary>
		public Texture Forward { get; set; }

		/// <summary>
		/// Gets or sets the filter mode for the texture sampling
		/// </summary>
		public TextureFilter Filter { get; set; }

		// Used for indexing
		private Texture[] textures;

		/// <summary>
		/// Creates a new environment utilizing a skybox
		/// </summary>
		/// <param name="right">Right side texture</param>
		/// <param name="left">Left side texture</param>
		/// <param name="up">Above texture</param>
		/// <param name="down">Below texture</param>
		/// <param name="back">Backwards texture</param>
		/// <param name="forward">Forward texture</param>
		/// <param name="filter">The filter mode for the texture sampling</param>
		public EnvironmentSkybox(Texture right, Texture left, Texture up, Texture down, Texture back, Texture forward, TextureFilter filter = TextureFilter.Point)
		{
			Right = right;
			Left = left;
			Up = up;
			Down = down;
			Back = back;
			Forward = forward;
			Filter = filter;

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

		/// <summary>
		/// Returns a color from the given direction.
		/// Referenced: https://www.gamedev.net/forums/topic/687535-implementing-a-cube-map-lookup-function/5337472/
		/// </summary>
		/// <param name="direction">The direction in which we're looking for a color</param>
		/// <returns>The color as a vector3</returns>
		public override Vector3 GetColorFromDirection(Vector3 direction)
		{
			// Which texture?
			int faceIndex = -1;
			float mag;
			Vector3 absDir = Vector3.Abs(direction);
			Vector2 uv;

			if (absDir.X >= absDir.Y && absDir.X >= absDir.Z)
			{
				// Left or right (flipped due to LH/RH)
				if (direction.X >= 0)
				{
					faceIndex = 1; // Was 0
					uv = new Vector2(-direction.Z, -direction.Y);
				}
				else
				{
					faceIndex = 0; // Was 1
					uv = new Vector2(direction.Z, -direction.Y);
				}

				mag = 0.5f / absDir.X;
			}
			else if (absDir.Y >= absDir.X && absDir.Y >= absDir.Z)
			{
				// Up or down (flipped UV's due to LH vs RH)
				if (direction.Y >= 0)
				{
					faceIndex = 2;
					uv = new Vector2(-direction.X, -direction.Z); // Was (X, Z)
				}
				else
				{
					faceIndex = 3;
					uv = new Vector2(-direction.X, direction.Z); // Was (X, -Z)
				}

				mag = 0.5f / absDir.Y;
			}
			else
			{
				// Forward or back
				if (direction.Z >= 0)
				{
					faceIndex = 4;
					uv = new Vector2(direction.X, -direction.Y);
				}
				else
				{
					faceIndex = 5;
					uv = new Vector2(-direction.X, -direction.Y);
				}

				mag = 0.5f / absDir.Z;
			}

			// Now that we know the face, convert to a UV
			uv = uv * mag + new Vector2(0.5f, 0.5f);

			// Sample
			return textures[faceIndex].Sample(uv, TextureAddressMode.Clamp, Filter).ToVector3();
		}
	}
}
