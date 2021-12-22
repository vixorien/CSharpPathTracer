using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.Xna.Framework;

namespace CSharpRaytracing
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
	/// Base class for all geometry
	/// </summary>
	abstract class Geometry
	{
		public Material Material { get; set; }

		public Geometry(Material material)
		{
			Material = material;
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

		public Mesh(string file, Material material) : base(material)
		{
			indices = new List<int>();
			vertices = new List<Vertex>();

			LoadMesh(file);
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

	/// <summary>
	/// A perfect sphere represented by a center point and a radius
	/// </summary>
	class Sphere : Geometry
	{
		public Vector3 Center { get; set; }
		public float Radius { get; set; }

		public Sphere(Vector3 center, float radius, Material material) : base(material)
		{
			Center = center;
			Radius = radius;
		}
	}

	struct SphereOld
	{
		public static SphereOld Null { get { return new SphereOld(Vector3.Zero, 0, null); } }

		public Vector3 Center { get; set; }
		public float Radius { get; set; }

		public Material Material { get; set; }

		public SphereOld(Vector3 center, float radius, Material mat)
		{
			Center = center;
			Radius = radius;
			Material = mat;
		}
	}

	struct SphereIntersection
	{
		public Vector3 Position { get; }
		public Vector3 Normal { get; }
		public float Distance { get; }
		public SphereOld Sphere { get; }

		public SphereIntersection(Vector3 position, Vector3 normal, float distance, SphereOld sphere)
		{
			Position = position;
			Normal = normal;
			Distance = distance;
			Sphere = sphere;
		}
	}

	struct Ray
	{
		public Vector3 Origin { get; set; }
		public Vector3 Direction { get; set; }
		public float TMin { get; set; }
		public float TMax { get; set; }

		public Ray(Vector3 origin, Vector3 direction) 
			: this(origin, direction, 0.0f, 1000.0f)
		{ }

		public Ray(Vector3 origin, Vector3 direction, float tmin, float tmax)
		{
			Origin = origin;
			Direction = direction;
			Direction.Normalize();
			TMin = tmin;
			TMax = tmax;
		}

		public bool Intersect(SphereOld sphere, out SphereIntersection[] hits)
		{
			// How far along ray to closest point to sphere center
			Vector3 originToCenter = sphere.Center - this.Origin;
			float tCenter = Vector3.Dot(originToCenter, this.Direction);

			// If tCenter is negative, we point away from sphere
			if (tCenter < 0)
			{
				// No intersection points
				hits = new SphereIntersection[0];
				return false;
			}

			// Distance from closest point to sphere's center
			float d = MathF.Sqrt(originToCenter.LengthSquared() - tCenter * tCenter);
			
			// If distance is greater than radius, we don't hit the sphere
			if (d > sphere.Radius)
			{
				// No intersection points
				hits = new SphereIntersection[0];
				return false;
			}

			// Offset from tCenter to an intersection point
			float offset = MathF.Sqrt(sphere.Radius * sphere.Radius - d * d);

			// Distances to the two hit points
			float t1 = tCenter - offset;
			float t2 = tCenter + offset;

			// Points of intersection
			Vector3 p1 = this.Origin + this.Direction * t1;
			Vector3 p2 = this.Origin + this.Direction * t2;

			// Normals
			Vector3 n1 = p1 - sphere.Center; n1.Normalize();
			Vector3 n2 = p2 - sphere.Center; n2.Normalize();

			// Set up return values
			hits = new SphereIntersection[2];
			hits[0] = new SphereIntersection(p1, n1, t1, sphere);
			hits[1] = new SphereIntersection(p2, n2, t2, sphere);
			return true;
		}
	}
}
