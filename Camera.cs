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
		private float aperture;
		private float lensRadius;
		private float focalDistance;

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
		/// Gets or sets the camera's aperture
		/// </summary>
		public float Aperture { get { return aperture; } set { aperture = value; lensRadius = aperture / 2.0f; } }

		/// <summary>
		/// Gets or sets the focal plane distance of the camera
		/// </summary>
		public float FocalDistance { get { return focalDistance; } set { focalDistance = value; projDirty = true; } }

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
		/// <param name="aperture">The aperture of the camera lens</param>
		/// <param name="focalDistance">The focal plane distance of the camera</param>
		public Camera(
			Vector3 position, 
			float aspectRatio,
			float fieldOfView,
			float nearClip,
			float farClip,
			float aperture,
			float focalDistance)
		{
			transform = new Transform();
			transform.SetPosition(position);
			NearClip = nearClip;
			FarClip = farClip;
			AspectRatio = aspectRatio;
			FieldOfView = fieldOfView;
			Aperture = aperture;
			FocalDistance = focalDistance;

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
			#region Matrix version (Replaced below)
			//// Calculate NDCs
			//Vector4 ndc = new Vector4(x, y, 0, 1);
			//ndc.X = ndc.X / screenWidth * 2.0f - 1.0f;
			//ndc.Y = ndc.Y / screenHeight * 2.0f - 1.0f;
			//ndc.Y = -ndc.Y;

			//// Unproject coordinates
			//Vector4 unprojPos = Vector4.Transform(ndc, InverseViewProjection);
			//unprojPos /= unprojPos.W;

			//// Create a vector3 from the unprojected vec4
			//Vector3 worldPos = new Vector3(
			//	unprojPos.X,
			//	unprojPos.Y,
			//	unprojPos.Z);
			#endregion

			// New (non-matrix) math for position - slightly faster, especially in release
			float w = 2 * MathF.Tan(fieldOfView / 2.0f) * nearClip;
			Vector2 ndc = new Vector2(x / screenWidth, y / screenHeight);
			ndc = Vector2.Add(Vector2.Multiply(2.0f, ndc), new Vector2(-1, -1));

			ndc.X *= w;
			ndc.Y *= w / aspectRatio;

			Vector3 screenCenter = transform.Position + transform.Forward * nearClip;
			Vector3 worldPos = screenCenter - transform.Up * ndc.Y + transform.Right * ndc.X;


			// Calculate the depth of field offset
			Vector3 randCircle = ThreadSafeRandom.Instance.NextVectorInCircle(lensRadius);
			Vector3 depthOfFieldOffset = transform.Right * randCircle.X + transform.Up * randCircle.Y;

			// Create the ray
			return new Ray(transform.Position + depthOfFieldOffset, Vector3.Normalize(worldPos - transform.Position - depthOfFieldOffset), NearClip, FarClip);
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
				FieldOfView,
				AspectRatio,
				NearClip + FocalDistance,
				FarClip);
			projDirty = false;
		}
	
	}
}
