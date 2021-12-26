using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

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

		private Matrix worldMatrix;
		private Matrix worldInverseTransposeMatrix;
		private bool matricesDirty;

		private Transform parent;
		private List<Transform> children;

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
		public Matrix WorldMatrix { get { UpdateMatrices(); return worldMatrix; } }

		/// <summary>
		/// Gets the inverse transpose of the transform's world matrix
		/// </summary>
		public Matrix WorldInverseTransposeMatrix { get { UpdateMatrices(); return worldInverseTransposeMatrix; } }

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

			up = Vector3.Up;
			right = Vector3.Right;
			forward = Vector3.Forward;
			vectorsDirty = false;

			worldMatrix = Matrix.Identity;
			worldInverseTransposeMatrix = Matrix.Identity;
			matricesDirty = false;

			parent = null;
			children = new List<Transform>();
		}

		public void MoveAbsolute(float x, float y, float z)
		{
			position.X += x;
			position.Y += y;
			position.Z += z;
			matricesDirty = true;
		}

		public void MoveAbsolute(Vector3 offset)
		{
			position += offset;
			matricesDirty = true;
		}

		public void MoveRelative(float x, float y, float z)
		{
			MoveRelative(new Vector3(x, y, z));
		}

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

		public void Rotate(float p, float y, float r)
		{
			pitchYawRoll.X += p;
			pitchYawRoll.Y += y;
			pitchYawRoll.Z += r;
			matricesDirty = true;
			vectorsDirty = true;
		}

		public void Rotate(Vector3 pitchYawRoll)
		{
			pitchYawRoll += pitchYawRoll;
			matricesDirty = true;
			vectorsDirty = true;
		}

		public void ScaleRelative(float uniformScale)
		{
			scale *= uniformScale;
			matricesDirty = true;
		}

		public void ScaleRelative(float x, float y, float z)
		{
			scale.X *= x;
			scale.Y *= y;
			scale.Z *= z;
			matricesDirty = true;
		}

		public void ScaleRelative(Vector3 offset)
		{
			scale *= offset;
			matricesDirty = true;
		}

		public void SetPosition(float x, float y, float z)
		{
			position = new Vector3(x, y, z);
			matricesDirty = true;
		}

		public void SetPosition(Vector3 pos)
		{
			position = pos;
			matricesDirty = true;
		}

		public void SetRotation(float p, float y, float r)
		{
			pitchYawRoll = new Vector3(p, y, r);
			matricesDirty = true;
			vectorsDirty = true;
		}

		public void SetRotation(Vector3 pitchYawRoll)
		{
			this.pitchYawRoll = pitchYawRoll;
			matricesDirty = true;
			vectorsDirty = true;
		}

		public void SetScale(float uniformScale)
		{
			this.scale = new Vector3(uniformScale, uniformScale, uniformScale);
			matricesDirty = true;
		}

		public void SetScale(float x, float y, float z)
		{
			this.scale = new Vector3(x, y, z);
			matricesDirty = true;
		}

		public void SetScale(Vector3 scale)
		{
			this.scale = scale;
			matricesDirty = true;
		}

		public void SetTransformFromMatrix(Matrix world)
		{
			// Decompose the matrix
			Quaternion localRot;
			world.Decompose(out this.scale, out localRot, out this.position);

			// Get the euler angles from the quaternion
			pitchYawRoll = QuaternionToEuler(localRot);

			// Things have changed
			matricesDirty = true;
			vectorsDirty = true;
		}

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
				Matrix invParent = Matrix.Invert(worldMatrix);
				Matrix relChild = child.WorldMatrix * invParent;

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

		public Transform GetChild(int index)
		{
			if (index < 0 || index >= children.Count)
				return null;

			return children[index];
		}

		public int GetChildIndex(Transform child)
		{
			// Verify child
			if (child == null)
				return -1;

			// Search and return index
			return children.IndexOf(child);
		}

		private void UpdateMatrices()
		{
			if (!matricesDirty)
				return;

			// Create the three transforms
			Matrix trans = Matrix.CreateTranslation(position);
			Matrix rot = Matrix.CreateFromYawPitchRoll(pitchYawRoll.Y, pitchYawRoll.X, pitchYawRoll.Z);
			Matrix sc = Matrix.CreateScale(scale);

			// Combine for local world
			Matrix wm = sc * rot * trans;

			// Is there a parent?
			if (parent != null)
			{
				wm *= parent.WorldMatrix; 
			}

			// Store the result and create inverse transpose
			worldMatrix = wm;
			worldInverseTransposeMatrix = Matrix.Invert(Matrix.Transpose(wm));

			// Up to date!
			matricesDirty = false;
		}

		private void UpdateVectors()
		{
			if (!vectorsDirty)
				return;

			// Update all three vectors
			Matrix rotMat = Matrix.CreateFromYawPitchRoll(pitchYawRoll.Y, pitchYawRoll.X, pitchYawRoll.Z);
			up = rotMat.Up;
			right = rotMat.Right;
			forward = rotMat.Forward;

			// Vectors are now up to date
			vectorsDirty = false;
		}

		private void MarkChildTransformsDirty()
		{
			// Recursively set each child dirty
			foreach (Transform c in children)
			{
				c.matricesDirty = true;
				c.MarkChildTransformsDirty();
			}
		}

		private Vector3 QuaternionToEuler(Quaternion quat)
		{
			// Convert quaternion to euler angles
			// Note: This will give a set of euler angles, but not necessarily
			// the same angles that were used to create the quaternion

			// Step 1: Quaternion to rotation matrix
			Matrix rotMat = Matrix.CreateFromQuaternion(quat);

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
