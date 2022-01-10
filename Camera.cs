using System;
using System.Numerics;

namespace CSharpPathTracer
{
	/// <summary>
	/// A 3D camera
	/// </summary>
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

		/// <summary>
		/// Gets the camera's transform
		/// </summary>
		public Transform Transform { get { return transform; } }

		/// <summary>
		/// Gets or sets the camera's near clip plane distance
		/// </summary>
		public float NearClip { get { return nearClip; } set { nearClip = value; projDirty = true; } }

		/// <summary>
		/// Gets or sets the camera's far clip plane distance
		/// </summary>
		public float FarClip { get { return farClip; } set { farClip = value; projDirty = true; } }

		/// <summary>
		/// Gets or sets the camera's aspect ratio
		/// </summary>
		public float AspectRatio { get { return aspectRatio; } set { aspectRatio = value; projDirty = true; } }

		/// <summary>
		/// Gets or sets the camera's field of view (in Y)
		/// </summary>
		public float FieldOfView { get { return fieldOfView; } set { fieldOfView = value; projDirty = true; } }

		/// <summary>
		/// Gets the camera's view matrix
		/// </summary>
		public Matrix4x4 View { get { if (transform.Dirty) UpdateViewMatrix(); return viewMatrix; } }

		/// <summary>
		/// Gets the camera's projection matrix
		/// </summary>
		public Matrix4x4 Projection { get { if (projDirty) UpdateProjectionMatrix(); return projMatrix; } }

		/// <summary>
		/// Gets the inverse of the combined view and projection matrices
		/// </summary>
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

		/// <summary>
		/// Creates a new camera
		/// </summary>
		/// <param name="position">The starting position of the camera</param>
		/// <param name="aspectRatio">The aspect ratio (w/h)</param>
		/// <param name="fieldOfView">The field of view (in Y)</param>
		/// <param name="nearClip">The near clip plane distance</param>
		/// <param name="farClip">The far clip plane distance</param>
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

		/// <summary>
		/// Gets a ray that goes through the given pixel
		/// </summary>
		/// <param name="x">The x coord of the pixel</param>
		/// <param name="y">The y coord of the pixel</param>
		/// <param name="screenWidth">The overall screen width</param>
		/// <param name="screenHeight">The overall screen height</param>
		/// <returns>A ray whose origin is the camera that travels through the given pixel</returns>
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

		/// <summary>
		/// Updates the view matrix
		/// </summary>
		private void UpdateViewMatrix()
		{
			viewMatrix = Matrix4x4.CreateLookAt(
				transform.Position,
				transform.Position + transform.Forward,
				Vector3.UnitY);
		}

		/// <summary>
		/// Updates the projection matrix
		/// </summary>
		private void UpdateProjectionMatrix()
		{
			if (!projDirty)
				return;

			projMatrix = Matrix4x4.CreatePerspectiveFieldOfView(
				MathF.PI / 4,
				AspectRatio,
				NearClip,
				FarClip);
			projDirty = false;
		}
	
	}
}
