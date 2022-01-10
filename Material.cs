using System;
using System.Numerics;

namespace CSharpPathTracer
{
	/// <summary>
	/// Represents the basics of a surface material
	/// </summary>
	abstract class Material
	{
		/// <summary>
		/// Default mode for texture addressing
		/// </summary>
		protected const TextureAddressMode DefaultAddressMode = TextureAddressMode.Wrap;

		/// <summary>
		/// Default texture filter mode
		/// </summary>
		protected const TextureFilter DefaultFilterMode = TextureFilter.Point;

		/// <summary>
		/// Gets or sets the material's base color
		/// </summary>
		public Vector3 Color { get; set; }

		/// <summary>
		/// Gets or sets the material's texture.  Note this is tinted by the base color.
		/// </summary>
		public Texture Texture { get; set; }

		/// <summary>
		/// Gets or sets the material's texture address mode
		/// </summary>
		public TextureAddressMode AddressMode { get; set; }

		/// <summary>
		/// Gets or sets the texture's filter mode
		/// </summary>
		public TextureFilter Filter { get; set; }

		/// <summary>
		/// Gets or sets the material's roughness.  Note this is superseded by a roughness map, if present.
		/// </summary>
		public float Roughness { get; set; }

		/// <summary>
		/// Gets or sets the material's roughness map texture.  Note this supersedes the basic roughness value.
		/// </summary>
		public Texture RoughnessMap { get; set; }

		/// <summary>
		/// Gets or sets the material's overall UV scale
		/// </summary>
		public Vector2 UVScale { get; set; }

		/// <summary>
		/// Creates a new material
		/// </summary>
		/// <param name="color">Base color</param>
		/// <param name="texture">Texture (tinted by color)</param>
		/// <param name="roughnessMap">Roughness map texture (supersedes roughness value)</param>
		/// <param name="roughness">Uniform roughness value (superseded by roughness map)</param>
		/// <param name="uvScale">UV scale to apply</param>
		/// <param name="addressMode">Texture address mode</param>
		/// <param name="filter">Texture filter mode</param>
		public Material(
			Vector3 color, 
			Texture texture = null, 
			Texture roughnessMap = null, 
			float roughness = 0.0f, 
			Vector2? uvScale = null, 
			TextureAddressMode addressMode = DefaultAddressMode,
			TextureFilter filter = DefaultFilterMode)
		{
			Color = color;
			Texture = texture;
			RoughnessMap = roughnessMap;
			Roughness = roughness;
			UVScale = uvScale ?? Vector2.One;
			AddressMode = addressMode;
			Filter = filter;
		}

		/// <summary>
		/// Gets the color at the specified uv coord
		/// </summary>
		/// <param name="uv">The uv to sample</param>
		/// <returns>A color as a vector3</returns>
		public Vector3 GetColorAtUV(Vector2 uv)
		{
			return Texture == null ? Color : Texture.Sample(uv * UVScale, AddressMode, Filter).ToVector3() * Color;
		}

		/// <summary>
		/// Gets the roughness at the specified uv coord
		/// </summary>
		/// <param name="uv">The uv to sample</param>
		/// <returns>A roughness value</returns>
		public float GetRoughnessAtUV(Vector2 uv)
		{
			return RoughnessMap == null ? Roughness : RoughnessMap.Sample(uv * UVScale, AddressMode, Filter).X;
		}

		/// <summary>
		/// Generates a new ray after the given ray has hit this material
		/// </summary>
		/// <param name="ray">The ray to further bounce</param>
		/// <param name="hit">Hit information about the intersection</param>
		/// <returns>A new ray</returns>
		public abstract Ray GetNextBounce(Ray ray, RayHit hit);

		/// <summary>
		/// Performs a refraction and reports whether or not it succeeds
		/// </summary>
		/// <param name="incident">Incoming vector</param>
		/// <param name="normal">Normal of surface</param>
		/// <param name="indexOfRefraction">Ratio of values of the two media</param>
		/// <param name="refraction">Refraction vector (if refraction works)</param>
		/// <returns>True if refraction is valid, false otherwise</returns>
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
			refraction = Vector3.Normalize(rPerp + rParallel);
			return true;
		}

		/// <summary>
		/// Schlick approx of Fresnel
		/// </summary>
		/// <param name="NdotV">Dot between normal and "view"</param>
		/// <param name="indexOfRefraction">Index of refraction for the surface</param>
		/// <returns>Fresnel approximation result</returns>
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
		/// <summary>
		/// Creates a new perfectly diffuse material
		/// </summary>
		/// <param name="color">Base color</param>
		/// <param name="texture">Texture (tinted by color)</param>
		/// <param name="roughnessMap">Roughness map texture (supersedes roughness value)</param>
		/// <param name="roughness">Uniform roughness value (superseded by roughness map)</param>
		/// <param name="uvScale">UV scale to apply</param>
		/// <param name="addressMode">Texture address mode</param>
		/// <param name="filter">Texture filter mode</param>
		public DiffuseMaterial(
			Vector3 color, 
			Texture texture = null, 
			Texture roughnessMap = null, 
			float roughness = 1.0f, 
			Vector2? uvScale = null,
			TextureAddressMode addressMode = DefaultAddressMode,
			TextureFilter filter = DefaultFilterMode)
			: 
			base(color, texture, roughnessMap, roughness, uvScale, addressMode, filter)
		{
		}

		/// <summary>
		/// Completely random bounce in the hemisphere centered on this hit's normal
		/// </summary>
		/// <param name="ray">The ray to further bounce</param>
		/// <param name="hit">Hit information about the intersection</param>
		/// <returns>A new ray</returns>
		public override Ray GetNextBounce(Ray ray, RayHit hit)
		{
			// Random bounce since we're assuming its perfectly diffuse
			Vector3 bounce = ThreadSafeRandom.Instance.NextVectorInHemisphere(hit.Normal);

			// Note: This assumes 100% roughness (perfectly diffuse) - could handle roughness
			// but we'd need to split into diffuse & specular components
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
		/// <summary>
		/// Creates a new metallic material
		/// </summary>
		/// <param name="color">Base color</param>
		/// <param name="texture">Texture (tinted by color)</param>
		/// <param name="roughnessMap">Roughness map texture (supersedes roughness value)</param>
		/// <param name="roughness">Uniform roughness value (superseded by roughness map)</param>
		/// <param name="uvScale">UV scale to apply</param>
		/// <param name="addressMode">Texture address mode</param>
		/// <param name="filter">Texture filter mode</param>
		public MetalMaterial(
			Vector3 color, 
			Texture texture = null,
			Texture roughnessMap = null, 
			float roughness = 0.0f, 
			Vector2? uvScale = null,
			TextureAddressMode addressMode = DefaultAddressMode,
			TextureFilter filter = DefaultFilterMode)
			: 
			base(color, texture, roughnessMap, roughness, uvScale, addressMode, filter)
		{
		}

		/// <summary>
		/// Perfect reflection, adjusted based on the roughness of the surface
		/// </summary>
		/// <param name="ray">The ray to further bounce</param>
		/// <param name="hit">Hit information about the intersection</param>
		/// <returns>A new ray</returns>
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
		/// <summary>
		/// Gets or sets the index of refraction of this material
		/// </summary>
		public float IndexOfRefraction { get; set; }

		/// <summary>
		/// Creates a new transparent material
		/// </summary>
		/// <param name="color">Base color</param>
		/// <param name="indexOfRefraction">Ratio of values of the two media</param>
		/// <param name="texture">Texture (tinted by color)</param>
		/// <param name="roughnessMap">Roughness map texture (supersedes roughness value)</param>
		/// <param name="roughness">Uniform roughness value (superseded by roughness map)</param>
		/// <param name="uvScale">UV scale to apply</param>
		/// <param name="addressMode">Texture address mode</param>
		/// <param name="filter">Texture filter mode</param>
		public TransparentMaterial(
			Vector3 color, 
			float indexOfRefraction, 
			Texture texture = null, 
			Texture roughnessMap = null, 
			float roughness = 0.0f, 
			Vector2? uvScale = null,
			TextureAddressMode addressMode = DefaultAddressMode,
			TextureFilter filter = DefaultFilterMode)
			: 
			base(color, texture, roughnessMap, roughness, uvScale, addressMode, filter)
		{
			IndexOfRefraction = indexOfRefraction;
		}

		/// <summary>
		/// Either refraction or reflection based on the fresnel result and roughness
		/// </summary>
		/// <param name="ray">The ray to further bounce</param>
		/// <param name="hit">Hit information about the intersection</param>
		/// <returns>A new ray</returns>
		public override Ray GetNextBounce(Ray ray, RayHit hit)
		{
			// Invert the index depending on the side
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

			// Adjust based on roughness (TODO: test this more)
			newDir += ThreadSafeRandom.Instance.NextVector3() * GetRoughnessAtUV(hit.UV);

			// Create the new ray based on either reflection or refraction
			return new Ray(hit.Position, newDir, ray.TMin, ray.TMax);
		}
	}
}
