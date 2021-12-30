using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CSharpPathTracer
{
	/// <summary>
	/// Represents the basics of a surface material
	/// </summary>
	abstract class Material
	{
		public Vector3 Color { get; set; }
		public Texture Texture { get; set; }

		public float Roughness { get; set; }
		public Texture RoughnessMap { get; set; }

		public Vector2 UVScale { get; set; }

		public Material(Vector3 color, Texture texture = null, Texture roughnessMap = null, float roughness = 0.0f)
		{
			Color = color;
			Texture = texture;
			RoughnessMap = roughnessMap;
			Roughness = roughness;
			UVScale = Vector2.One;
		}

		public Vector3 GetColorAtUV(Vector2 uv)
		{
			return Texture == null ? Color : Texture.Sample(uv * UVScale).ToVector3() * Color;
		}

		public float GetRoughnessAtUV(Vector2 uv)
		{
			return RoughnessMap == null ? Roughness : RoughnessMap.Sample(uv * UVScale).X;
		}

		public abstract Ray GetNextBounce(Ray ray, RayHit hit);


		public static bool Refract(Vector3 incident, Vector3 normal, float indexOfRefraction, out Vector3 refraction)
		{
			float cos = Vector3.Dot(-1 * incident, normal);
			float sin = MathF.Sqrt(1.0f - cos * cos);
			if (indexOfRefraction * sin > 1.0f)
			{
				refraction = Vector3.Zero;
				return false;
			}

			Vector3 rPerp = indexOfRefraction * (incident + cos * normal);
			Vector3 rParallel = -MathF.Sqrt(MathF.Abs(1.0f - rPerp.LengthSquared())) * normal;
			refraction = (rPerp + rParallel).Normalized();
			return true;
		}


		public static double FresnelSchlick(float NdotV, float indexOfRefraction)
		{
			float r0 = MathF.Pow((1.0f - indexOfRefraction) / (1.0f + indexOfRefraction), 2.0f);
			return r0 + (1.0f - r0) * MathF.Pow(1 - NdotV, 5.0f);
		}
	}

	/// <summary>
	/// A perfectly diffuse surface
	/// </summary>
	class DiffuseMaterial : Material
	{
		public DiffuseMaterial(Vector3 color, Texture texture = null, Texture roughnessMap = null, float roughness = 1.0f)
			: base(color, texture, roughnessMap, roughness)
		{
		}

		public override Ray GetNextBounce(Ray ray, RayHit hit)
		{
			// Random bounce since we're assuming its perfectly diffuse
			Vector3 bounce = ThreadSafeRandom.Instance.NextVectorInHemisphere(hit.Normal);

			// TODO: Handle roughness?
			// This is difficult because it shouldn't tint the reflection like a metal

			return new Ray(
				hit.Position,
				bounce,
				ray.TMin,
				ray.TMax);
		}
	}

	/// <summary>
	/// A metallic surface
	/// </summary>
	class MetalMaterial : Material
	{
		public MetalMaterial(Vector3 color, Texture texture = null, Texture roughnessMap = null, float roughness = 0.0f)
			: base(color, texture, roughnessMap, roughness)
		{
		}

		public override Ray GetNextBounce(Ray ray, RayHit hit)
		{
			// Perfect reflection since we're metal
			Vector3 reflection = Vector3.Reflect(ray.Direction, hit.Normal);

			// Adjust based on roughness
			reflection += ThreadSafeRandom.Instance.NextVector3() * GetRoughnessAtUV(hit.UV);

			return new Ray(
				hit.Position,
				reflection,
				ray.TMin,
				ray.TMax);
		}
	}

	/// <summary>
	/// A transparent (refractive) surface
	/// </summary>
	class TransparentMaterial : Material
	{
		public float IndexOfRefraction { get; set; }

		public TransparentMaterial(Vector3 color, float indexOfRefraction, Texture texture = null, Texture roughnessMap = null, float roughness = 0.0f)
			: base(color, texture, roughnessMap, roughness)
		{
			IndexOfRefraction = indexOfRefraction;
		}

		public override Ray GetNextBounce(Ray ray, RayHit hit)
		{
			float ior = IndexOfRefraction;
			if (hit.Side == HitSide.Outside)
				ior = 1.0f / ior;

			// Random chance for reflection instead of refraction based on Fresnel
			float NdotV = Vector3.Dot(-1 * ray.Direction, hit.Normal);
			bool reflectFresnel = FresnelSchlick(NdotV, ior) > ThreadSafeRandom.Instance.NextFloat();

			// Test for refraction
			Vector3 newDir;
			if (reflectFresnel || !Refract(ray.Direction, hit.Normal, ior, out newDir))
				newDir = Vector3.Reflect(ray.Direction, hit.Normal);

			// Adjust based on roughness...
			newDir += ThreadSafeRandom.Instance.NextVector3() * GetRoughnessAtUV(hit.UV);

			// Create the new ray based on either reflection or refraction
			return new Ray(hit.Position, newDir, ray.TMin, ray.TMax);
		}
	}
}
