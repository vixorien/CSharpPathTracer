using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace CSharpPathTracer
{
	class Environment
	{
		public Vector3 ColorUp { get; }
		public Vector3 ColorForward { get; }
		public Vector3 ColorDown { get; }

		public Environment(Vector3 solidColor)
		{
			ColorUp = solidColor;
			ColorForward = solidColor;
			ColorDown = solidColor;
		}

		public Environment(Vector3 colorUp, Vector3 colorForward, Vector3 colorDown)
		{
			ColorUp = colorUp;
			ColorForward = colorForward;
			ColorDown = colorDown;
		}

		public Vector3 GetColorFromDirection(Vector3 direction)
		{
			// Normalize and grab the dot product
			direction.Normalize();
			float dot = Vector3.Dot(Vector3.Up, direction);

			// Interpolate and return the gradient result
			if (dot > 0.0f)
			{
				return Vector3.Lerp(ColorForward, ColorUp, dot);
			}
			else
			{
				return Vector3.Lerp(ColorForward, ColorDown, dot * -1);
			}
		}
	}
}
