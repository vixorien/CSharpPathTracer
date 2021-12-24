using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CSharpPathTracer
{
	class Camera
	{
		private Vector3 position;
		private Vector3 direction;
		private float nearClip;
		private float farClip;
		private float aspectRatio;
		private float fieldOfView;

		private Matrix viewMatrix;
		private Matrix projMatrix;
		private bool viewDirty;
		private bool projDirty;

		public Vector3 Position { get { return position; } set { position = value; viewDirty = true; } }
		public Vector3 Direction { get { return direction; } set { direction = value; viewDirty = true; } }
		public float NearClip { get { return nearClip; } set { nearClip = value; projDirty = true; } }
		public float FarClip { get { return farClip; } set { farClip = value; projDirty = true; } }
		public float AspectRatio { get { return aspectRatio; } set { aspectRatio = value; projDirty = true; } }
		public float FieldOfView { get { return fieldOfView; } set { fieldOfView = value; projDirty = true; } }
		public Matrix View { get { if (viewDirty) UpdateViewMatrix(); return viewMatrix; } }
		public Matrix Projection { get { if (projDirty) UpdateProjectionMatrix(); return projMatrix; } }
		public Matrix InverseViewProjection 
		{ 
			get 
			{
				if (viewDirty) UpdateViewMatrix();
				if (projDirty) UpdateProjectionMatrix();
				return Matrix.Invert(View * Projection);
			} 
		}

		public Camera(
			Vector3 position, 
			Vector3 direction,
			float aspectRatio,
			float fieldOfView,
			float nearClip,
			float farClip)
		{
			Position = position;
			Direction = direction;
			NearClip = nearClip;
			FarClip = farClip;
			AspectRatio = aspectRatio;
			FieldOfView = fieldOfView;

			viewDirty = true;
			projDirty = true;
		}

		public Ray GetRayThroughPixel(float x, float y, int screenWidth, int screenHeight)
		{
			// Calculate NDCs
			Vector4 ndc = new Vector4(x, y, 0, 1);
			ndc.X = ndc.X / screenWidth * 2.0f - 1.0f;
			ndc.Y = ndc.Y / screenHeight * 2.0f - 1.0f;
			ndc.Y = -ndc.Y;

			// Unproject coordinates
			Vector4 unprojPos = Vector4.Transform(ndc, InverseViewProjection);
			unprojPos /= unprojPos.W;

			// Create a vector3 from the unprojected vec4
			Vector3 worldPos = new Vector3(
				unprojPos.X,
				unprojPos.Y,
				unprojPos.Z);

			// Create the ray
			return new Ray(Position, Vector3.Normalize(worldPos - Position));
		}

		private void UpdateViewMatrix()
		{
			viewMatrix = Matrix.CreateLookAt(
				Position,
				Position + Direction,
				Vector3.Up);
			viewDirty = false;
		}

		private void UpdateProjectionMatrix()
		{
			projMatrix = Matrix.CreatePerspectiveFieldOfView(
				MathHelper.Pi / 4,
				AspectRatio,
				NearClip,
				FarClip);
			projDirty = false;
		}
	
	}
}
