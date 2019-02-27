using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;


namespace PathTracer
{
	/// <summary>
	/// reflectance：反射率
	/// emissive：发光率
	/// roughness：粗糙系数
	/// ri
	/// </summary>
	public struct Material
	{
		public enum Type { Lambert = 0, Metal = 1, Dielectric = 2 };
		public Type type;
		public float3 reflectance;
		public float3 emissive;
		public float roughness;
		public float ri;
		public Material(Type t, float3 a, float3 e, float r, float i)
		{
			type = t; reflectance = a; emissive = e; roughness = r; ri = i;
		}
		public bool HasEmission => emissive.x > 0 || emissive.y > 0 || emissive.z > 0;
	};

	public class Tracer
	{
		const int SAMPLE_NUM_PER_PIXEL = 4;
		const int SAMPLE_NUM_PER_REFL = 2;

		Sphere[] s_Spheres;

		Material[] s_SphereMats;

		Spheres spheres;

		public Tracer(Scene scene)
		{
			s_Spheres = scene.spheres;
			s_SphereMats = scene.materials;

			spheres = new Spheres(s_Spheres.Length);
		}

		const float kMinT = 0.001f;
		const float kMaxT = 1.0e7f;
		const int kMaxDepth = 10;


		bool HitWorld(ref Ray r, float tMin, float tMax, ref Hit outHit, ref int outID)
		{
			outID = spheres.HitSpheres(ref r, tMin, tMax, ref outHit);
			return outID != -1;
		}

		bool Scatter(ref Material mat, ref Ray r_in, Hit rec, out float3 attenuation, out List<Ray> scattered, out float3 outLightE, ref int inoutRayCount, ref uint state)
		{
			outLightE = new float3(0, 0, 0);
			if (mat.type == Material.Type.Lambert)
			{
				float3 target = rec.pos + rec.normal + MathUtil.RandomUnitVector(ref state);

				scattered = new List<Ray>();
				for (int i = 0; i < SAMPLE_NUM_PER_REFL; i++)
					scattered.Add(new Ray(rec.pos, float3.Normalize(target - rec.pos)));
				attenuation = mat.reflectance;

				return true;
			}
			else if (mat.type == Material.Type.Metal)
			{
				Debug.Assert(r_in.dir.IsNormalized); Debug.Assert(rec.normal.IsNormalized);
				float3 refl = float3.Reflect(r_in.dir, rec.normal);
				scattered = new List<Ray> { new Ray(rec.pos, float3.Normalize(refl + mat.roughness * MathUtil.RandomInUnitSphere(ref state))) };
				attenuation = mat.reflectance;
				
				return float3.Dot(scattered[0].dir, rec.normal) > 0;
			}
			else if (mat.type == Material.Type.Dielectric)
			{
				Debug.Assert(r_in.dir.IsNormalized); Debug.Assert(rec.normal.IsNormalized);
				float3 outwardN;
				float3 rdir = r_in.dir;
				float3 refl = float3.Reflect(rdir, rec.normal);
				float nint;
				attenuation = new float3(1, 1, 1);
				float3 refr;
				float reflProb;
				float cosine;
				if (float3.Dot(rdir, rec.normal) > 0)
				{
					outwardN = -rec.normal;
					nint = mat.ri;
					cosine = mat.ri * float3.Dot(rdir, rec.normal);
				}
				else
				{
					outwardN = rec.normal;
					nint = 1.0f / mat.ri;
					cosine = -float3.Dot(rdir, rec.normal);
				}
				if (float3.Refract(rdir, outwardN, nint, out refr))
				{
					reflProb = MathUtil.Schlick(cosine, mat.ri);
				}
				else
				{
					reflProb = 1;
				}
				if (MathUtil.RandomFloat01(ref state) < reflProb)
					scattered = new List<Ray> { new Ray(rec.pos, float3.Normalize(refl)) };
				else
					scattered = new List<Ray> { new Ray(rec.pos, float3.Normalize(refr)) };
			}
			else
			{
				attenuation = new float3(1, 0, 1);
				scattered = new List<Ray>();
				return false;
			}
			return true;
		}
		
		float3 Trace(Ray r, int depth, ref int inoutRayCount, ref uint state, bool doMaterialE = true)
		{
			Hit rec = default(Hit);
			int id = 0;
			++inoutRayCount;
			if (HitWorld(ref r, kMinT, kMaxT, ref rec, ref id))
			{
				List<Ray> scattered;
				float3 attenuation;//衰减
				float3 lightE;
				ref Material mat = ref s_SphereMats[id];
				var matE = mat.emissive;
				if (depth < kMaxDepth && Scatter(ref mat, ref r, rec, out attenuation, out scattered, out lightE, ref inoutRayCount, ref state))
				{
					float3 temp = new float3(0,0,0);
					foreach (Ray ray in scattered)
						temp += Trace(ray, depth + 1, ref inoutRayCount, ref state, doMaterialE);
					return matE + lightE + attenuation * temp / scattered.Count;
				}
				else
				{
					return matE;
				}
			}
			else
			{
				// sky
				float3 unitDir = r.dir;
				float t = 0.5f * (unitDir.y + 1.0f);
				return ((1.0f - t) * new float3(1.0f, 1.0f, 1.0f) + t * new float3(0.5f, 0.7f, 1.0f)) * 0.3f;
			}
		}

		void TraceRow(int y, int screenWidth, int screenHeight, int frameCount, float[] backbuffer, ref Camera cam)
		{
			int backbufferIdx = y * screenWidth * 3;
			float invWidth = 1.0f / screenWidth;
			float invHeight = 1.0f / screenHeight;
			float lerpFac = (float)frameCount / (float)(frameCount + 1);

			int rayCount = 0;
			//for (uint32_t y = start; y < end; ++y)
			{
				uint state = (uint)(y * 9781 + frameCount * 6271) | 1;
				for (int x = 0; x < screenWidth; ++x)
				{
					float3 col = new float3(0, 0, 0);
					for (int s = 0; s < SAMPLE_NUM_PER_PIXEL; s++)
					{
						float u = (x + MathUtil.RandomFloat01(ref state)) * invWidth;
						float v = (y + MathUtil.RandomFloat01(ref state)) * invHeight;
						Ray r = cam.GetRay(u, v, ref state);
						col += Trace(r, 0, ref rayCount, ref state);
					}
					col *= 1.0f / (float)SAMPLE_NUM_PER_PIXEL;

					ref float bb1 = ref backbuffer[backbufferIdx + 0];
					ref float bb2 = ref backbuffer[backbufferIdx + 1];
					ref float bb3 = ref backbuffer[backbufferIdx + 2];

					float3 prev = new float3(bb1, bb2, bb3);
					col = prev * lerpFac + col * (1 - lerpFac);
					bb1 = col.x;
					bb2 = col.y;
					bb3 = col.z;
					backbufferIdx += 3;
				}
			}
		}


		public void DrawTest(int frameCount, int screenWidth, int screenHeight, float[] backbuffer)
		{
			float3 lookfrom = new float3(0, 2, 3);
			float3 lookat = new float3(0, 0, 0);
			float distToFocus = 3;
			float aperture = 0.1f;

			spheres.Update(s_Spheres, s_SphereMats);

			Camera cam = new Camera(lookfrom, lookat, new float3(0, 1, 0), 60, (float)screenWidth / (float)screenHeight, aperture, distToFocus);

			for (int y = 0; y < screenHeight; ++y)
				TraceRow(y, screenWidth, screenHeight, frameCount, backbuffer, ref cam);
		}
	}

}
