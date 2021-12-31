using System;
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

	struct Triangle : IBoundable, IRayIntersectable
	{
		/// <summary>
		/// Gets or sets whether triangle backfaces should be culled
		/// </summary>
		public static bool CullBackfaces { get; set; }

		// Fields
		private BoundingBox aabb;

		/// <summary>
		/// Gets the AABB bounding box of this geometry
		/// </summary>
		public BoundingBox AABB => aabb;

		public Vertex V0 { get; set; }
		public Vertex V1 { get; set; }
		public Vertex V2 { get; set; }

		public Triangle(Vertex v0, Vertex v1, Vertex v2)
		{
			V0 = v0;
			V1 = v1;
			V2 = v2;

			aabb = new BoundingBox(v0.Position, v0.Position);
			aabb.Encompass(v1.Position);
			aabb.Encompass(v2.Position);
		}

		public Vector3 CalcNormalBarycentric(Vector3 barycentrics)
		{
			return
				(barycentrics.X * V1.Normal +
				barycentrics.Y * V2.Normal +
				barycentrics.Z * V0.Normal).Normalized();
		}

		public Vector2 CalcUVBarycentric(Vector3 barycentrics)
		{
			return
				barycentrics.X * V1.UV +
				barycentrics.Y * V2.UV +
				barycentrics.Z * V0.UV;
		}

		public bool RayIntersection(Ray ray, out RayHit hit)
		{
			// Assume no hit
			hit = RayHit.None;

			// From: https://graphicscodex.com/Sample2-RayTriangleIntersection.pdf
			Vector3 edge1 = V1.Position - V0.Position;
			Vector3 edge2 = V2.Position - V0.Position;

			Vector3 normal = Vector3.Cross(edge1, edge2);
			normal.Normalize();

			Vector3 q = Vector3.Cross(ray.Direction, edge2);
			float a = Vector3.Dot(edge1, q);

			// Parallel?
			if (MathF.Abs(a) < 0.000001f)
				return false;

			// Is this a backface?
			HitSide side = HitSide.Outside;
			if (Vector3.Dot(normal, ray.Direction) >= 0)
			{
				// Get out early if we're culling
				if (CullBackfaces) return false;

				// Not culling; denote that we're inside
				side = HitSide.Inside;
			}

			// Get barycentrics
			Vector3 s = (ray.Origin - V0.Position) / a;
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
			if (t < ray.TMin || t > ray.TMax)
				return false;

			// If we're inside, flip the normal
			Vector3 hitNormal = CalcNormalBarycentric(bary);
			if (side == HitSide.Inside)
				hitNormal *= -1;

			// Success, so fill out the hit
			hit = new RayHit(
				ray.Origin + ray.Direction * t,
				hitNormal,
				CalcUVBarycentric(bary),
				t,
				side,
				this);
			return true;
		}
	}


	/// <summary>
	/// Geometry represented by a mesh of indexed triangles
	/// </summary>
	class Mesh : Geometry
	{
		// Mesh data
		//private List<int> indices;
		//private List<Vertex> vertices;
		private List<Triangle> triangles;
		private Octree<Triangle> octree;

		public Mesh(string file) : base()
		{
			//indices = new List<int>();
			//vertices = new List<Vertex>();
			triangles = new List<Triangle>();

			LoadMesh(file);

			// Now that the mesh is loaded, build the octree
			octree = new Octree<Triangle>(AABB);
			foreach (Triangle t in triangles)
				octree.AddObject(t);

			// Shrink the octree to speed up ray intersections
			octree.ShrinkAndPrune();
		}


		public override bool RayIntersection(Ray ray, out RayHit hit)
		{
			return octree.RayIntersection(ray, out hit);
		}


		private void LoadMesh(string file)
		{
			List<Vector3> positions = new List<Vector3>();
			List<Vector3> normals = new List<Vector3>();
			List<Vector2> uvs = new List<Vector2>();

			//int vertCounter = 0;

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
						Vector3 pos = new Vector3(float.Parse(p[1]), float.Parse(p[2]), float.Parse(p[3]));

						// Add to the positions list as well as the AABB
						positions.Add(pos);
						aabb.Encompass(pos);
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
						//////////////////vertices.Add(v1);
						//////////////////vertices.Add(v3);
						//////////////////vertices.Add(v2);
						
						//vertices.Add(v1);
						//vertices.Add(v2);
						//vertices.Add(v3);

						// Add three more indices
						//indices.Add(vertCounter); vertCounter += 1;
						//indices.Add(vertCounter); vertCounter += 1;
						//indices.Add(vertCounter); vertCounter += 1;

						// Add the triangle
						triangles.Add(new Triangle(v1, v2, v3));

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
							///////////////////////vertices.Add(v1);
							///////////////////////vertices.Add(v4);
							///////////////////////vertices.Add(v3);
							//vertices.Add(v1);
							//vertices.Add(v3);
							//vertices.Add(v4);

							// Add three more indices
							//indices.Add(vertCounter); vertCounter += 1;
							//indices.Add(vertCounter); vertCounter += 1;
							//indices.Add(vertCounter); vertCounter += 1;

							// Add the triangle
							triangles.Add(new Triangle(v1, v3, v4));
						}

					}
				}
			}
		}
	}
}
