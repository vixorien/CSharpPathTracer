using System;
using System.Collections.Generic;
using System.Numerics;

namespace CSharpPathTracer
{
	/// <summary>
	/// Represents a set of translation, rotation and scale transformations
	/// </summary>
	public class Transform
	{
		private Vector3 position;
		private Vector3 pitchYawRoll;
		private Vector3 scale;

		private Vector3 up;
		private Vector3 right;
		private Vector3 forward;
		private bool vectorsDirty;

		private Matrix4x4 worldMatrix;
		private Matrix4x4 worldInverseTransposeMatrix;
		private bool matricesDirty;

		private Transform parent;
		private List<Transform> children;

		/// <summary>
		/// Gets whether the transform's matrices are currently out of date
		/// </summary>
		public bool Dirty { get { return matricesDirty || vectorsDirty; } }

		/// <summary>
		/// Gets or sets the transform's position
		/// </summary>
		public Vector3 Position
		{
			get { return position; }
			set { position = value; matricesDirty = true; }
		}

		/// <summary>
		/// Gets or sets the transform's pitch/yaw/roll rotation values
		/// </summary>
		public Vector3 PitchYawRoll
		{
			get { return pitchYawRoll; }
			set { pitchYawRoll = value; matricesDirty = true; vectorsDirty = true; }
		}

		/// <summary>
		/// Gets or sets the transform's scale
		/// </summary>
		public Vector3 Scale
		{
			get { return scale; }
			set { scale = value; matricesDirty = true; }
		}

		/// <summary>
		/// Gets the transform's local up vector
		/// </summary>
		public Vector3 Up { get { UpdateVectors(); return up; } }

		/// <summary>
		/// Gets the transform's local right vector
		/// </summary>
		public Vector3 Right { get { UpdateVectors(); return right; } }

		/// <summary>
		/// Gets the transform's local forward vector
		/// </summary>
		public Vector3 Forward { get { UpdateVectors(); return forward; } }

		/// <summary>
		/// Gets the transform's overall world matrix
		/// </summary>
		public Matrix4x4 WorldMatrix { get { UpdateMatrices(); return worldMatrix; } }

		/// <summary>
		/// Gets the inverse transpose of the transform's world matrix
		/// </summary>
		public Matrix4x4 WorldInverseTransposeMatrix { get { UpdateMatrices(); return worldInverseTransposeMatrix; } }

		/// <summary>
		/// Gets or sets this transform's parent
		/// </summary>
		public Transform Parent
		{
			get { return parent; }
			set	{ SetParent(value);	}
		}

		/// <summary>
		/// Gets the count of children in this transform
		/// </summary>
		public int ChildCount { get { return children.Count; } }

		/// <summary>
		/// Creates a new, default transform
		/// </summary>
		public Transform()
		{
			position = Vector3.Zero;
			pitchYawRoll = Vector3.Zero;
			scale = Vector3.One;

			up = Vector3.UnitY;
			right = Vector3.UnitX;
			forward = Vector3.UnitZ;
			vectorsDirty = false;

			worldMatrix = Matrix4x4.Identity;
			worldInverseTransposeMatrix = Matrix4x4.Identity;
			matricesDirty = false;

			parent = null;
			children = new List<Transform>();
		}

		/// <summary>
		/// Moves the position along the global XYZ axes by the specified amounts
		/// </summary>
		/// <param name="x">Amount to move in x</param>
		/// <param name="y">Amount to move in y</param>
		/// <param name="z">Amount to move in z</param>
		public void MoveAbsolute(float x, float y, float z)
		{
			position.X += x;
			position.Y += y;
			position.Z += z;
			matricesDirty = true;
		}

		/// <summary>
		/// Moves the position along the global XYZ axes by the specified amounts
		/// </summary>
		/// <param name="offset">The axis-aligned offset</param>
		public void MoveAbsolute(Vector3 offset)
		{
			position += offset;
			matricesDirty = true;
		}

		/// <summary>
		/// Moves the position along its local XYZ axes by the specified amounts		
		/// </summary>
		/// <param name="x">Amount to move in x</param>
		/// <param name="y">Amount to move in y</param>
		/// <param name="z">Amount to move in z</param>
		public void MoveRelative(float x, float y, float z)
		{
			MoveRelative(new Vector3(x, y, z));
		}

		/// <summary>
		/// Moves the position along its local XYZ axes by the specified amounts		
		/// </summary>
		/// <param name="offset">The offsets to move</param>
		public void MoveRelative(Vector3 offset)
		{
			// Create the rotation quaternion
			Quaternion rot = Quaternion.CreateFromYawPitchRoll(pitchYawRoll.Y, pitchYawRoll.X, pitchYawRoll.Z);

			// Transform the desired offset by the transform's rotation
			Vector3 rotatedOffset = Vector3.Transform(offset, rot);

			// Apply the rotated direction
			position += rotatedOffset;
			matricesDirty = true;
		}

		/// <summary>
		/// Rotates by the given pitch, yaw and roll values
		/// </summary>
		/// <param name="p">Pitch rotation</param>
		/// <param name="y">Yaw rotation</param>
		/// <param name="r">Roll rotation</param>
		public void Rotate(float p, float y, float r)
		{
			pitchYawRoll.X += p;
			pitchYawRoll.Y += y;
			pitchYawRoll.Z += r;
			matricesDirty = true;
			vectorsDirty = true;
		}

		/// <summary>
		/// Rotates by the given pitch, yaw and roll values
		/// </summary>
		/// <param name="pitchYawRoll">Rotation values</param>
		public void Rotate(Vector3 pitchYawRoll)
		{
			pitchYawRoll += pitchYawRoll;
			matricesDirty = true;
			vectorsDirty = true;
		}

		/// <summary>
		/// Scales the transform relatively (i.e., 2 is twice as large)
		/// </summary>
		/// <param name="uniformScale">The single value to apply to all axes</param>
		public void ScaleRelative(float uniformScale)
		{
			scale *= uniformScale;
			matricesDirty = true;
		}

		/// <summary>
		/// Scales the transform relatively (i.e., 2 is twice as large)
		/// </summary>
		/// <param name="x">Amount to scale in x</param>
		/// <param name="y">Amount to scale in y</param>
		/// <param name="z">Amount to scale in z</param>
		public void ScaleRelative(float x, float y, float z)
		{
			scale.X *= x;
			scale.Y *= y;
			scale.Z *= z;
			matricesDirty = true;
		}

		/// <summary>
		/// Scales the transform relatively (i.e., 2 is twice as large)
		/// </summary>
		/// <param name="offset">Amounts to scale</param>
		public void ScaleRelative(Vector3 offset)
		{
			scale *= offset;
			matricesDirty = true;
		}

		/// <summary>
		/// Overwrites the current position
		/// </summary>
		/// <param name="x">New x position</param>
		/// <param name="y">New y position</param>
		/// <param name="z">New z position</param>
		public void SetPosition(float x, float y, float z)
		{
			position = new Vector3(x, y, z);
			matricesDirty = true;
		}

		/// <summary>
		/// Overwrites the current position
		/// </summary>
		/// <param name="pos">The new position</param>
		public void SetPosition(Vector3 pos)
		{
			position = pos;
			matricesDirty = true;
		}

		/// <summary>
		/// Overwrites the current rotation
		/// </summary>
		/// <param name="p">The new pitch value</param>
		/// <param name="y">The new yaw value</param>
		/// <param name="r">The new roll value</param>
		public void SetRotation(float p, float y, float r)
		{
			pitchYawRoll = new Vector3(p, y, r);
			matricesDirty = true;
			vectorsDirty = true;
		}

		/// <summary>
		/// Overwrites the current rotation
		/// </summary>
		/// <param name="pitchYawRoll">New rotation values</param>
		public void SetRotation(Vector3 pitchYawRoll)
		{
			this.pitchYawRoll = pitchYawRoll;
			matricesDirty = true;
			vectorsDirty = true;
		}

		/// <summary>
		/// Overwrites the current scale
		/// </summary>
		/// <param name="uniformScale">The single value for all three axes</param>
		public void SetScale(float uniformScale)
		{
			this.scale = new Vector3(uniformScale, uniformScale, uniformScale);
			matricesDirty = true;
		}

		/// <summary>
		/// Overwrites the current scale
		/// </summary>
		/// <param name="x">New x scale</param>
		/// <param name="y">New y scale</param>
		/// <param name="z">New z scale</param>
		public void SetScale(float x, float y, float z)
		{
			this.scale = new Vector3(x, y, z);
			matricesDirty = true;
		}

		/// <summary>
		/// Overwrites the current scale
		/// </summary>
		/// <param name="scale">New scale values</param>
		public void SetScale(Vector3 scale)
		{
			this.scale = scale;
			matricesDirty = true;
		}

		/// <summary>
		/// Sets the position, rotation and scale from a single world matrix
		/// </summary>
		/// <param name="world">The world matrix to decompose</param>
		/// <returns>True if it works, false if the world matrix cannot be decomposed</returns>
		public bool SetTransformFromMatrix(Matrix4x4 world)
		{
			// Decompose the matrix
			Quaternion localRot;
			bool success = Matrix4x4.Decompose(world, out this.scale, out localRot, out this.position);
			if(success)
			{
				// Get the euler angles from the quaternion
				pitchYawRoll = QuaternionToEuler(localRot);

				// Things have changed
				matricesDirty = true;
				vectorsDirty = true;
			}

			return success;
		}

		/// <summary>
		/// Adds a transform as a child
		/// </summary>
		/// <param name="child">The new child transform</param>
		/// <param name="makeChildRelative">Should the child's transform change to keep it in the same spot relative to the parent?</param>
		public void AddChild(Transform child, bool makeChildRelative = true)
		{
			// Verify child is valid
			if (child == null)
				return;

			// Already a child?
			if (GetChildIndex(child) >= 0)
				return;

			// Are we making the child relative?
			if (makeChildRelative)
			{
				// Invert the parent's world matrix and
				// apply to the child's world
				Matrix4x4 invParent = worldMatrix.Invert();
				Matrix4x4 relChild = child.WorldMatrix * invParent;

				// Set the child's overall transform
				child.SetTransformFromMatrix(relChild);	
			}

			// Reciprocal set
			children.Add(child);
			child.parent = this; // Skip property!

			// Transform is out of date
			child.matricesDirty = true;
			child.MarkChildTransformsDirty();
		}

		/// <summary>
		/// Removes a child from this transform if possible
		/// </summary>
		/// <param name="child">The child to remove</param>
		/// <param name="applyParentTransform">Should the parent transform be applied so it stays put?</param>
		public void RemoveChild(Transform child, bool applyParentTransform = true)
		{
			// Is the child valid?
			if (child == null)
				return;

			// Is the child actually in the list?
			int childIndex = children.IndexOf(child);
			if (childIndex == -1)
				return;

			// We have a valid child - do we need to apply the parent transform?
			if (applyParentTransform)
			{
				// Apply the child's final matrix as its full transform
				child.SetTransformFromMatrix(child.worldMatrix);
			}

			// Reciprocal removal
			children.RemoveAt(childIndex);
			child.parent = null; // Skip property!

			// The child's transform is out of date
			child.matricesDirty = true;
			child.MarkChildTransformsDirty();
		}

		/// <summary>
		/// Sets the parent of this transform
		/// </summary>
		/// <param name="newParent">The new parent</param>
		/// <param name="makeChildRelative">Should this transform be adjust so it stays put?</param>
		public void SetParent(Transform newParent, bool makeChildRelative = true)
		{
			// Unparent if necessary
			if (parent != null)
			{
				parent.RemoveChild(this);
			}

			// Is the new parent something other than null?
			if (newParent != null)
			{
				newParent.AddChild(this, makeChildRelative);
			}
		}

		/// <summary>
		/// Gets the specified child
		/// </summary>
		/// <param name="index">The index of the child</param>
		/// <returns>The child transform, or null if the index is invalid</returns>
		public Transform GetChild(int index)
		{
			if (index < 0 || index >= children.Count)
				return null;

			return children[index];
		}

		/// <summary>
		/// Gets the index of a child transform
		/// </summary>
		/// <param name="child">The child to check for</param>
		/// <returns>The index, or -1 if child isn't found</returns>
		public int GetChildIndex(Transform child)
		{
			// Verify child
			if (child == null)
				return -1;

			// Search and return index
			return children.IndexOf(child);
		}

		/// <summary>
		/// Updates the matrices of the transform if they're dirty
		/// </summary>
		private void UpdateMatrices()
		{
			if (!matricesDirty)
				return;

			// Create the three transforms
			Matrix4x4 trans = Matrix4x4.CreateTranslation(position);
			Matrix4x4 rot = Matrix4x4.CreateFromYawPitchRoll(pitchYawRoll.Y, pitchYawRoll.X, pitchYawRoll.Z);
			Matrix4x4 sc = Matrix4x4.CreateScale(scale);

			// Combine for local world
			Matrix4x4 wm = sc * rot * trans;

			// Is there a parent?
			if (parent != null)
			{
				wm *= parent.WorldMatrix; 
			}

			// Store the result and create inverse transpose
			worldMatrix = wm;
			worldInverseTransposeMatrix = Matrix4x4.Transpose(wm).Invert();

			// Up to date!
			matricesDirty = false;
		}

		/// <summary>
		/// Updates the local vectors if they're dirty
		/// </summary>
		private void UpdateVectors()
		{
			if (!vectorsDirty)
				return;

			// Update all three vectors
			Matrix4x4 rotMat = Matrix4x4.CreateFromYawPitchRoll(pitchYawRoll.Y, pitchYawRoll.X, pitchYawRoll.Z);
			up = rotMat.Up();
			right = rotMat.Right();
			forward = rotMat.Forward();

			// Vectors are now up to date
			vectorsDirty = false;
		}

		/// <summary>
		/// Recursively marks child transforms dirty
		/// </summary>
		private void MarkChildTransformsDirty()
		{
			// Recursively set each child dirty
			foreach (Transform c in children)
			{
				c.matricesDirty = true;
				c.MarkChildTransformsDirty();
			}
		}

		/// <summary>
		/// Converts a quaternion to Euler angles
		/// </summary>
		/// <param name="quat">The quaternion to convert</param>
		/// <returns>The pitch, yaw and roll values</returns>
		private Vector3 QuaternionToEuler(Quaternion quat)
		{
			// Convert quaternion to euler angles
			// Note: This will give a set of euler angles, but not necessarily
			// the same angles that were used to create the quaternion

			// Step 1: Quaternion to rotation matrix
			Matrix4x4 rotMat = Matrix4x4.CreateFromQuaternion(quat);

			// Step 2: Extract each piece
			// NOTE: May need to transpose these for MonoGame!
			// From: https://stackoverflow.com/questions/60350349/directx-get-pitch-yaw-roll-from-xmmatrix
			float pitch = MathF.Asin(-rotMat.M32);
			float yaw = MathF.Atan2(rotMat.M31, rotMat.M33);
			float roll = MathF.Atan2(rotMat.M12, rotMat.M22); 

			return new Vector3(pitch, yaw, roll);
		}
	}
}
