﻿using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;


namespace CSharpPathTracer
{
	/// <summary>
	/// A single vertex in a triangle mesh
	/// </summary>
	public struct Vertex
	{
		public Vector3 Position { get; set; }
		public Vector3 Normal { get; set; }
		public Vector3 Tangent { get; set; }
		public Vector2 UV { get; set; }

		public Vertex(Vector3 position, Vector3 normal, Vector2 uv)
		{
			Position = position;
			Normal = normal;
			UV = uv;
			Tangent = Vector3.Zero;
		}
	}

	/// <summary>
	/// The point at which a ray hits a triangle
	/// </summary>
	struct TriangleHit
	{
		public Vertex V0 { get; set; }
		public Vertex V1 { get; set; }
		public Vertex V2 { get; set; }
		public float Distance { get; set; }
		public Vector3 Position { get; set; }
		public Vector3 Barycentrics { get; set; }

		public Vector3 CalcHitNormal()
		{
			return
				Barycentrics.X * V1.Normal +
				Barycentrics.Y * V2.Normal +
				Barycentrics.Z * V0.Normal;
		}

		public Vector2 CalcHitUV()
		{
			return
				Barycentrics.X * V1.UV +
				Barycentrics.Y * V2.UV +
				Barycentrics.Z * V0.UV;
		}
	}

	/// <summary>
	/// Geometry represented by a mesh of indexed triangles
	/// </summary>
	class Mesh : Geometry
	{
		// Mesh data
		private List<int> indices;
		private List<Vertex> vertices;

		public Mesh(string file) : base()
		{
			indices = new List<int>();
			vertices = new List<Vertex>();

			LoadMesh(file);
		}


		public override bool RayIntersection(Ray ray, out RayHit[] hits)
		{
			// Closest hit, if any
			bool anyHit = false;
			TriangleHit closestHit = new TriangleHit();
			closestHit.Distance = float.PositiveInfinity;

			// Loop through indices/triangles, looking for hits
			for (int i = 0; i < indices.Count;)
			{
				// Get the three vertices for this triangle
				Vertex v0 = vertices[indices[i++]];
				Vertex v1 = vertices[indices[i++]];
				Vertex v2 = vertices[indices[i++]];

				// Test for intersection
				TriangleHit triHit;
				if (RayTriangleIntersection(ray, v0, v1, v2, out triHit))
				{
					// We hit a triangle, test for closest
					if (triHit.Distance < closestHit.Distance)
					{
						closestHit = triHit;
					}

					anyHit = true;
				}
			}

			// No hits at all
			if (!anyHit)
			{
				hits = new RayHit[0];
				return false;
			}

			// Got a hit
			hits = new RayHit[1];
			hits[0] = new RayHit(
				closestHit.Position,
				closestHit.CalcHitNormal(),
				closestHit.Distance,
				null);
			return true;
		}

		public static bool RayTriangleIntersection(Ray ray, Vertex v0, Vertex v1, Vertex v2, out TriangleHit triHit, bool cullBackfaces = true)
		{
			// Assume no hit
			triHit = new TriangleHit();

			// From: https://graphicscodex.com/Sample2-RayTriangleIntersection.pdf
			Vector3 edge1 = v1.Position - v0.Position;
			Vector3 edge2 = v2.Position - v0.Position;

			Vector3 normal = Vector3.Cross(edge1, edge2);
			normal.Normalize();

			Vector3 q = Vector3.Cross(ray.Direction, edge2);
			float a = Vector3.Dot(edge1, q);

			// Parallel?
			if (MathF.Abs(a) < 0.000001f)
				return false;

			// Check backfaces?
			if (cullBackfaces && Vector3.Dot(normal, ray.Direction) >= 0)
				return false;

			// Get barycentrics
			Vector3 s = (ray.Origin - v0.Position) / a;
			Vector3 r = Vector3.Cross(s, edge1);

			Vector3 bary = Vector3.Zero;
			bary.X = Vector3.Dot(s, q);
			bary.Y = Vector3.Dot(r, ray.Direction);
			bary.Z = 1.0f - bary.X - bary.Y;

			// Outside triangle?
			if (bary.X < 0.0f || bary.Y < 0.0f || bary.Z < 0.0f)
				return false;

			// Calculate hit distance
			float t = Vector3.Dot(edge2, r);
			if (t < 0.0f)
				return false;

			triHit.V0 = v0;
			triHit.V1 = v1;
			triHit.V2 = v2;
			triHit.Distance = t;
			triHit.Position = ray.Origin + ray.Direction * t;
			triHit.Barycentrics = bary;
			return true;
		}

		private void LoadMesh(string file)
		{
			List<Vector3> positions = new List<Vector3>();
			List<Vector3> normals = new List<Vector3>();
			List<Vector2> uvs = new List<Vector2>();

			int vertCounter = 0;

			using (StreamReader input = new StreamReader(file))
			{
				// Read a single line and process
				String line = "";
				while ((line = input.ReadLine()) != null)
				{
					// Valid line?
					if (line.Length < 2)
						continue;

					// Check the type of line
					if (line[0] == 'v' && line[1] == 'n') // Vertex Normal
					{
						// Split and verify length
						string[] n = line.Split(' ');
						if (n.Length != 4) continue;

						// Valid length, so parse the elements (skip 0)
						normals.Add(new Vector3(float.Parse(n[1]), float.Parse(n[2]), float.Parse(n[3])));
					}
					else if (line[0] == 'v' && line[1] == 't') // Vertex Texture Coord
					{
						// Split and verify length
						string[] t = line.Split(' ');
						if (t.Length != 3) continue;

						// Valid length, so parse the elements (skip 0)
						uvs.Add(new Vector2(float.Parse(t[1]), float.Parse(t[2])));
					}
					else if (line[0] == 'v') // Vertex Position
					{
						// Split and verify length
						string[] p = line.Split(' ');
						if (p.Length != 4) continue;

						// Valid length, so parse the elements (skip 0)
						positions.Add(new Vector3(float.Parse(p[1]), float.Parse(p[2]), float.Parse(p[3])));
					}
					else if (line[0] == 'f') // Face
					{
						List<int> i = new List<int>();

						// Split the line into vertex sets, then loop and
						// split those into element indices, adding to the 
						// list above 
						string[] vertSets = line.Split(' ');
						for (int v = 1; v < vertSets.Length; v++)
						{
							string[] elementIndices = vertSets[v].Split('/');
							foreach (string elementIndex in elementIndices)
							{
								i.Add(int.Parse(elementIndex));
							}
						}

						// We now have a list of indices from the line - verify
						// that we have either 9 or 12 numbers (for a triangle or quad)
						if (!(i.Count == 9 || i.Count == 12))
							continue;


						// - Create the verts by looking up
						//    corresponding data from vectors
						// - OBJ File indices are 1-based, so
						//    they need to be adusted
						Vertex v1 = new Vertex(
							positions[i[0] - 1],
							normals[i[2] - 1],
							uvs[i[1] - 1]);

						Vertex v2 = new Vertex(
							positions[i[3] - 1],
							normals[i[5] - 1],
							uvs[i[4] - 1]);

						Vertex v3 = new Vertex(
							positions[i[6] - 1],
							normals[i[8] - 1],
							uvs[i[7] - 1]);

						// The model is most likely in a right-handed space,
						// especially if it came from Maya.  We want to convert
						// to a left-handed space for DirectX.  This means we 
						// need to:
						//  - Invert the Z position
						//  - Invert the normal's Z
						//  - Flip the winding order
						// We also need to flip the UV coordinate since DirectX
						// defines (0,0) as the top left of the texture, and many
						// 3D modeling packages use the bottom left as (0,0)

						// Flip the UV's since they're probably "upside down"
						//v1.UV.Y = 1.0f - v1.UV.Y;
						//v2.UV.Y = 1.0f - v2.UV.Y;
						//v3.UV.Y = 1.0f - v3.UV.Y;

						//// Flip Z (LH vs. RH)
						//v1.Position.Z *= -1.0f;
						//v2.Position.Z *= -1.0f;
						//v3.Position.Z *= -1.0f;

						//// Flip normal Z
						//v1.Normal.Z *= -1.0f;
						//v2.Normal.Z *= -1.0f;
						//v3.Normal.Z *= -1.0f;

						// Add the verts to the vector (flipping the winding order)
						//vertices.Add(v1);
						//vertices.Add(v3);
						//vertices.Add(v2);
						vertices.Add(v1);
						vertices.Add(v2);
						vertices.Add(v3);

						// Add three more indices
						indices.Add(vertCounter); vertCounter += 1;
						indices.Add(vertCounter); vertCounter += 1;
						indices.Add(vertCounter); vertCounter += 1;

						// Was there a 4th face?
						if (i.Count == 12)
						{
							// Make the last vertex
							Vertex v4 = new Vertex(
								positions[i[9] - 1],
								normals[i[11] - 1],
								uvs[i[10] - 1]);

							// Flip the UV, Z pos and normal
							//v4.UV.Y = 1.0f - v4.UV.Y;
							//v4.Position.Z *= -1.0f;
							//v4.Normal.Z *= -1.0f;

							// Add a whole triangle (flipping the winding order)
							//vertices.Add(v1);
							//vertices.Add(v4);
							//vertices.Add(v3);
							vertices.Add(v1);
							vertices.Add(v3);
							vertices.Add(v4);

							// Add three more indices
							indices.Add(vertCounter); vertCounter += 1;
							indices.Add(vertCounter); vertCounter += 1;
							indices.Add(vertCounter); vertCounter += 1;
						}

					}
				}
			}
		}
	}
}