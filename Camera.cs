using System;
using System.Collections.Generic;
using System.Text;

using System.Numerics;
//using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CSharpPathTracer
{
	class Camera
	{
		private Transform transform;
		private float nearClip;
		private float farClip;
		private float aspectRatio;
		private float fieldOfView;

		private Matrix4x4 viewMatrix;
		private Matrix4x4 projMatrix;
		private bool projDirty;

		public Transform Transform { get { return transform; } } // Not a great way to dirty the view, but it works
		public float NearClip { get { return nearClip; } set { nearClip = value; projDirty = true; } }
		public float FarClip { get { return farClip; } set { farClip = value; projDirty = true; } }
		public float AspectRatio { get { return aspectRatio; } set { aspectRatio = value; projDirty = true; } }
		public float FieldOfView { get { return fieldOfView; } set { fieldOfView = value; projDirty = true; } }
		public Matrix4x4 View { get { if (transform.Dirty) UpdateViewMatrix(); return viewMatrix; } }
		public Matrix4x4 Projection { get { if (projDirty) UpdateProjectionMatrix(); return projMatrix; } }
		public Matrix4x4 InverseViewProjection 
		{ 
			get 
			{
				if (transform.Dirty) UpdateViewMatrix();
				if (projDirty) UpdateProjectionMatrix();

				Matrix4x4 inv;
				if (Matrix4x4.Invert(View * Projection, out inv))
					return inv;
				else
					return Matrix4x4.Identity;
			} 
		}

		public Camera(
			Vector3 position, 
			float aspectRatio,
			float fieldOfView,
			float nearClip,
			float farClip)
		{
			transform = new Transform();
			transform.SetPosition(position);
			NearClip = nearClip;
			FarClip = farClip;
			AspectRatio = aspectRatio;
			FieldOfView = fieldOfView;

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
			return new Ray(transform.Position, Vector3.Normalize(worldPos - transform.Position), NearClip, FarClip);
		}

		private void UpdateViewMatrix()
		{
			viewMatrix = Matrix4x4.CreateLookAt(
				transform.Position,
				transform.Position + transform.Forward,
				Vector3.UnitY);
		}

		private void UpdateProjectionMatrix()
		{
			projMatrix = Matrix4x4.CreatePerspectiveFieldOfView(
				MathF.PI / 4,
				AspectRatio,
				NearClip,
				FarClip);
			projDirty = false;
		}
	
	}
}
