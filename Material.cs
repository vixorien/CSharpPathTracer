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
		/// Gets or sets the material's normal map texture.
		/// </summary>
		public Texture NormalMap { get; set; }

		/// <summary>
		/// Gets or sets the material's overall UV scale
		/// </summary>
		public Vector2 UVScale { get; set; }

		/// <summary>
		/// Creates a new material
		/// </summary>
		/// <param name="color">Base color</param>
		/// <param name="roughness">Uniform roughness value (superseded by roughness map)</param>
		/// <param name="texture">Texture (tinted by color)</param>
		/// <param name="roughnessMap">Roughness map texture (supersedes roughness value)</param>
		/// <param name="normalMap">Normal map texture</param>
		/// <param name="uvScale">UV scale to apply</param>
		/// <param name="addressMode">Texture address mode</param>
		/// <param name="filter">Texture filter mode</param>
		public Material(
			Vector3 color,
			float roughness = 0.0f,
			Texture texture = null, 
			Texture roughnessMap = null, 
			Texture normalMap = null,
			Vector2? uvScale = null,
			TextureAddressMode addressMode = DefaultAddressMode,
			TextureFilter filter = DefaultFilterMode)
		{
			Color = color;
			Roughness = roughness;
			Texture = texture;
			RoughnessMap = roughnessMap;
			NormalMap = normalMap;
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
		/// Gets the normal at the specified uv coord, taking into
		/// account the surface's normal map (if one exists)
		/// </summary>
		/// <param name="uv">The uv to sample</param>
		/// <param name="surfaceNormal">The surface's base normal</param>
		/// <param name="surfaceTangent">The surface's base tangent</param>
		/// <returns>A normal</returns>
		public Vector3 GetNormalAtUV(Vector2 uv, Vector3 surfaceNormal, Vector3 surfaceTangent)
		{
			// Is there a normal map?
			if (NormalMap == null)
				return surfaceNormal;

			// Sample and unpack normal map
			Vector3 normalFromMap = NormalMap.Sample(uv * UVScale, AddressMode, Filter).ToVector3();
			normalFromMap *= 2.0f;
			normalFromMap = Vector3.Subtract(normalFromMap, Vector3.One);

			// Create TBN matrix
			Vector3 N = surfaceNormal;
			Vector3 T = Vector3.Normalize(surfaceTangent - N * Vector3.Dot(surfaceTangent, N));
			Vector3 B = Vector3.Cross(N, T);  // Reverse due to right-handed

			Matrix4x4 TBN = new Matrix4x4(
				T.X, T.Y, T.Z, 0,
				B.X, B.Y, B.Z, 0,
				N.X, N.Y, N.Z, 0,
				0, 0, 0, 1);

			// Apply TBN and return result
			return Vector3.Normalize(Vector3.TransformNormal(normalFromMap, TBN));
		}

		/// <summary>
		/// Gets the emission color at the specified uv coord
		/// </summary>
		/// <param name="uv">The uv to sample</param>
		/// <returns>An emissive color</returns>
		public virtual Vector3 GetEmissionAtUV(Vector2 uv)
		{
			// No emission by default
			return Vector3.Zero;
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


		/// <summary>
		/// Schlick approx of Fresnel using a constant 0.04 for f0
		/// </summary>
		/// <param name="NdotV">Dot between normal and "view"</param>
		/// <returns>Fresnel approximation result</returns>
		public static double FresnelSchlick(float NdotV)
		{
			float f0 = 0.04f;
			return f0 + (1.0f - f0) * MathF.Pow(1 - NdotV, 5.0f);
		}

		// See: https://computergraphics.stackexchange.com/questions/4486/mimicking-blenders-roughness-in-my-path-tracer
		// Also look up paper: "A Simpler and Exact Sampling Routine for the GGX Distribution of Visible Normals"
		// http://hacksoflife.blogspot.com/2015/12/importance-sampling-look-mom-no-weights.html
		// https://agraphicsguy.wordpress.com/2015/11/01/sampling-microfacet-brdf/
		// https://schuttejoe.github.io/post/ggximportancesamplingpart1/
		// http://cwyman.org/code/dxrTutors/tutors/Tutor14/tutorial14.md.html <-----
		public static Vector3 GGX_NormalDistribution(float roughness, float rand1, float rand2)
		{
			// theta = arctan((a * sqrt(r1)) / sqrt(1-r1))
			// phi = 2*pi*r2
			// Where r1 and r2 are uniform random numbers [0,1]
			// Output is a set of polar coords (theta,phi) relative to surface normal

			// a = roughness^2 (and unreal does it again!)
			float a = roughness * roughness;

			float theta = MathF.Atan((a * MathF.Sqrt(rand1)) / MathF.Sqrt(1.0f - rand1));
			float phi = 2.0f * MathF.PI * rand2;

			// Note: This assumes x/y plane and "z up"
			float x = MathF.Cos(phi) * MathF.Sin(theta);
			float y = MathF.Sin(phi) * MathF.Sin(theta);
			float z = MathF.Cos(theta);

			return new Vector3(x, y, z);
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
		/// <param name="normalMap">Normal map texture</param>
		/// <param name="uvScale">UV scale to apply</param>
		/// <param name="addressMode">Texture address mode</param>
		/// <param name="filter">Texture filter mode</param>
		public DiffuseMaterial(
			Vector3 color,
			Texture texture = null,
			Texture normalMap = null,
			Vector2? uvScale = null,
			TextureAddressMode addressMode = DefaultAddressMode,
			TextureFilter filter = DefaultFilterMode)
			: 
			base(color, 1.0f, texture, null, normalMap, uvScale, addressMode, filter)
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
			// Handle optional normal map
			hit.Normal = GetNormalAtUV(hit.UV, hit.Normal, hit.Tangent);

			// Random bounce since we're assuming its perfectly diffuse
			// TODO: Handle bounce ray going through the object due to normal map
			Vector3 bounce = ThreadSafeRandom.Instance.NextVectorInHemisphere(hit.Normal);

			return new Ray(
				hit.Position,
				bounce,
				ray.TMin,
				ray.TMax,
				false); // Perfectly diffuse so there's never a specular ray
		}
	}

	class DiffuseAndSpecularMaterial : Material
	{
		/// <summary>
		/// Creates a new perfectly diffuse material
		/// </summary>
		/// <param name="color">Base color</param>
		/// <param name="roughness">Uniform roughness value (superseded by roughness map)</param>
		/// <param name="texture">Texture (tinted by color)</param>
		/// <param name="roughnessMap">Roughness map texture (supersedes roughness value)</param>
		/// <param name="normalMap">Normal map texture</param>
		/// <param name="uvScale">UV scale to apply</param>
		/// <param name="addressMode">Texture address mode</param>
		/// <param name="filter">Texture filter mode</param>
		public DiffuseAndSpecularMaterial(
			Vector3 color,
			float roughness = 1.0f,
			Texture texture = null,
			Texture roughnessMap = null,
			Texture normalMap = null,
			Vector2? uvScale = null,
			TextureAddressMode addressMode = DefaultAddressMode,
			TextureFilter filter = DefaultFilterMode)
			:
			base(color, roughness, texture, roughnessMap, normalMap, uvScale, addressMode, filter)
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
			// Handle optional normal map
			hit.Normal = GetNormalAtUV(hit.UV, hit.Normal, hit.Tangent);

			//// Adjust normal based on roughness using GGX Normal Dist Function
			//Vector3 ggxNormal = GGX_NormalDistribution(
			//	GetRoughnessAtUV(hit.UV),
			//	ThreadSafeRandom.Instance.NextFloat(),
			//	ThreadSafeRandom.Instance.NextFloat());

			//Vector3 N = hit.Normal;
			//Vector3 T = Vector3.Normalize(hit.Tangent - N * Vector3.Dot(hit.Tangent, N));
			//Vector3 B = Vector3.Cross(N, T);

			//Matrix4x4 TBN = new Matrix4x4(
			//	T.X, T.Y, T.Z, 0,
			//	B.X, B.Y, B.Z, 0,
			//	N.X, N.Y, N.Z, 0,
			//	0, 0, 0, 1);

			//Vector3 roughNormal = Vector3.Normalize(Vector3.TransformNormal(ggxNormal, TBN));

			//// Randomly choose whether this is a diffuse or specular ray
			//float NdotV = Vector3.Dot(-1 * ray.Direction, roughNormal);// hit.Normal);
			//bool reflectFresnel = FresnelSchlick(NdotV) > ThreadSafeRandom.Instance.NextFloat();

			//// Determine if the bounce is specular or diffuse
			//Vector3 bounce;
			//if (reflectFresnel)
			//{
			//	Vector3 refl = Vector3.Reflect(ray.Direction, roughNormal);// hit.Normal);

			//	// Adjust based on roughness
			//	Vector3 randVec = ThreadSafeRandom.Instance.NextVector3() * GetRoughnessAtUV(hit.UV);
			//	bounce = refl + randVec;

			//	// Verify this new reflection vector is on the correct side 
			//	// of the normal, and if not, reverse the random vector
			//	// Note: The probability isn't uniform, so this may need a PDF?
			//	if (Vector3.Dot(hit.Normal, bounce) < 0)
			//		bounce = refl - randVec;
			//}
			//else
			//{
			//	// Random bounce since we're assuming its perfectly diffuse
			//	// Shouldn't need the GGX normal, right?
			//	bounce = ThreadSafeRandom.Instance.NextVectorInHemisphere(hit.Normal);
			//}

			// Very basic diffuse/specular switch using a lerp
			// Randomly choose whether this is a diffuse or specular ray
			float NdotV = Vector3.Dot(-1 * ray.Direction, hit.Normal);
			bool reflectFresnel = FresnelSchlick(NdotV) > ThreadSafeRandom.Instance.NextFloat();

			// Calculate both bounces
			Vector3 diffuseBounce = ThreadSafeRandom.Instance.NextVectorInHemisphere(hit.Normal);
			Vector3 specularBounce = Vector3.Reflect(ray.Direction, hit.Normal);

			// Adjust specular based on roughness
			float roughness = GetRoughnessAtUV(hit.UV);
			float a = roughness * roughness;
			specularBounce = Vector3.Normalize(Vector3.Lerp(specularBounce, diffuseBounce, a));

			// Which ray are we actually using?
			Vector3 bounce = reflectFresnel ? specularBounce : diffuseBounce;

			// Note: This assumes 100% roughness (perfectly diffuse) - could handle roughness
			// but we'd need to split into diffuse & specular components
			return new Ray(
				hit.Position,
				bounce,
				ray.TMin,
				ray.TMax,
				reflectFresnel); // Fresnel = specular ray here
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
		/// <param name="roughness">Uniform roughness value (superseded by roughness map)</param>
		/// <param name="texture">Texture (tinted by color)</param>
		/// <param name="roughnessMap">Roughness map texture (supersedes roughness value)</param>
		/// <param name="normalMap">Normal map texture</param>
		/// <param name="uvScale">UV scale to apply</param>
		/// <param name="addressMode">Texture address mode</param>
		/// <param name="filter">Texture filter mode</param>
		public MetalMaterial(
			Vector3 color,
			float roughness = 0.0f,
			Texture texture = null,
			Texture roughnessMap = null,
			Texture normalMap = null,
			Vector2? uvScale = null,
			TextureAddressMode addressMode = DefaultAddressMode,
			TextureFilter filter = DefaultFilterMode)
			: 
			base(color, roughness, texture, roughnessMap, normalMap, uvScale, addressMode, filter)
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
			// Handle optional normal map
			hit.Normal = GetNormalAtUV(hit.UV, hit.Normal, hit.Tangent);

			// Perfect reflection since we're metal
			Vector3 reflection = Vector3.Reflect(ray.Direction, hit.Normal);

			// Adjust based on roughness
			Vector3 randVec = ThreadSafeRandom.Instance.NextVector3() * GetRoughnessAtUV(hit.UV);
			Vector3 roughReflection = reflection + randVec;

			// Verify this new reflection vector is on the correct side 
			// of the normal, and if not, reverse the random vector
			// Note: The probability isn't uniform, so this may need a PDF?
			if (Vector3.Dot(hit.Normal, roughReflection) < 0) 
				roughReflection = reflection - randVec;

			return new Ray(
				hit.Position,
				roughReflection,
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
		/// <param name="roughness">Uniform roughness value (superseded by roughness map)</param>
		/// <param name="texture">Texture (tinted by color)</param>
		/// <param name="roughnessMap">Roughness map texture (supersedes roughness value)</param>
		/// <param name="normalMap">Normal map texture</param>
		/// <param name="uvScale">UV scale to apply</param>
		/// <param name="addressMode">Texture address mode</param>
		/// <param name="filter">Texture filter mode</param>
		public TransparentMaterial(
			Vector3 color, 
			float indexOfRefraction,
			float roughness = 0.0f,
			Texture texture = null, 
			Texture roughnessMap = null,
			Texture normalMap = null,
			Vector2? uvScale = null,
			TextureAddressMode addressMode = DefaultAddressMode,
			TextureFilter filter = DefaultFilterMode)
			: 
			base(color, roughness, texture, roughnessMap, normalMap, uvScale, addressMode, filter)
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
			// Handle optional normal map
			hit.Normal = GetNormalAtUV(hit.UV, hit.Normal, hit.Tangent);

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

			// Adjust based on roughness
			Vector3 randVec = ThreadSafeRandom.Instance.NextVector3() * GetRoughnessAtUV(hit.UV);
			Vector3 roughNewDir = newDir + randVec;

			// Create the new ray based on either reflection or refraction
			return new Ray(hit.Position, roughNewDir, ray.TMin, ray.TMax);
		}
	}

	/// <summary>
	/// A material that emits light
	/// </summary>
	class EmissiveMaterial : DiffuseMaterial
	{
		/// <summary>
		/// Gets or sets the emissive texture for this material
		/// </summary>
		public Texture EmissiveTexture { get; set; }

		/// <summary>
		/// Gets or sets the emissive color for the material.  When used
		/// in conjunction with an emissive texture, this acts as a tint.
		/// </summary>
		public Vector3 EmissiveColor { get; set; }

		/// <summary>
		/// Gets or sets the intensity of emission.  This is a multiplier
		/// to the overall emission of this material.
		/// </summary>
		public float EmissiveIntensity { get; set; }

		/// <summary>
		/// Creates a new perfectly diffuse material
		/// </summary>
		/// <param name="emissiveColor">Color emitted by the surface</param>
		/// <param name="emissiveIntensity">Intensity of emission</param>
		/// <param name="emissiveTexture">Emissive map texture</param>
		/// <param name="color">Base color</param>
		/// <param name="roughness">Uniform roughness value (superseded by roughness map)</param>
		/// <param name="texture">Texture (tinted by color)</param>
		/// <param name="roughnessMap">Roughness map texture (supersedes roughness value)</param>
		/// <param name="normalMap">Normal map texture</param>
		/// <param name="uvScale">UV scale to apply</param>
		/// <param name="addressMode">Texture address mode</param>
		/// <param name="filter">Texture filter mode</param>
		public EmissiveMaterial(
			Vector3 emissiveColor,
			float emissiveIntensity = 1.0f,
			Texture emissiveTexture = null,
			Vector3 color = new Vector3(), // 0,0,0
			Texture texture = null,
			Texture normalMap = null,
			Vector2? uvScale = null,
			TextureAddressMode addressMode = DefaultAddressMode,
			TextureFilter filter = DefaultFilterMode)
			:
			base(color, texture, normalMap, uvScale, addressMode, filter)
		{
			EmissiveColor = emissiveColor;
			EmissiveIntensity = emissiveIntensity;
			EmissiveTexture = emissiveTexture;
		}


		/// <summary>
		/// Returns the material's color for emission
		/// </summary>
		/// <param name="uv">The uv for sampling</param>
		/// <returns>The emitted color</returns>
		public override Vector3 GetEmissionAtUV(Vector2 uv)
		{
			// Calculate overall color
			Vector3 baseEmission = EmissiveColor * EmissiveIntensity;
			
			// Determine if texture is in use
			return EmissiveTexture == null ? baseEmission : EmissiveTexture.Sample(uv * UVScale, AddressMode, Filter).ToVector3() * baseEmission;
		}
	}
}
