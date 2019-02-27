using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PathTracer
{

//数学计算集
public static class MathF
{
    public static float Sqrt(float v) { return (float)Math.Sqrt(v); }
    public static float Abs(float v) { return (float)Math.Abs(v); }
    public static float Pow(float a, float b) { return (float)Math.Pow(a, b); }
    public static float Sin(float v) { return (float)Math.Sin(v); }
    public static float Cos(float v) { return (float)Math.Cos(v); }
    public static float Tan(float v) { return (float)Math.Tan(v); }
    public static float Max(float a, float b) { return (float) Math.Max(a, b); }
}

    
    //定义三维向量和相应操作
	public struct float3
	{
		public float x, y, z;

		public float3(float x_, float y_, float z_) { x = x_; y = y_; z = z_; }

		public float SqLength => x * x + y * y + z * z;
		public float Length => MathF.Sqrt(x * x + y * y + z * z);
		public void Normalize() { float k = 1.0f / Length; x *= k; y *= k; z *= k; }

		public static float3 operator +(float3 a, float3 b) { return new float3(a.x + b.x, a.y + b.y, a.z + b.z); }
		public static float3 operator -(float3 a, float3 b) { return new float3(a.x - b.x, a.y - b.y, a.z - b.z); }
		public static float3 operator *(float3 a, float3 b) { return new float3(a.x * b.x, a.y * b.y, a.z * b.z); }
		public static float3 operator *(float3 a, float b) { return new float3(a.x * b, a.y * b, a.z * b); }
		public static float3 operator *(float a, float3 b) { return new float3(a * b.x, a * b.y, a * b.z); }
		public static float3 operator -(float3 a) { return new float3(-a.x, -a.y, -a.z); }
		public static float3 operator /(float3 a,float b) { return new float3(a.x / b, a.y / b, a.z / b); }

        //向量点乘
		public static float Dot(float3 a, float3 b) { return a.x * b.x + a.y * b.y + a.z * b.z; }
        //向量叉乘
		public static float3 Cross(float3 a, float3 b) { return new float3(a.y * b.z - a.z * b.y, -(a.x * b.z - a.z * b.x), a.x * b.y - a.y * b.x); }
        //向量正规化
        public static float3 Normalize(float3 v) { float k = 1.0f / v.Length; return new float3(v.x * k, v.y * k, v.z * k); }

		public bool IsNormalized => MathF.Abs(SqLength - 1.0f) < 0.01f;

        /// <summary>
        /// 光线的反射：
        /// 入射向量和反射向量的向量和为入射向量在法向量上投影的两倍
        /// </summary>
        /// <param name="v"></param>
        /// <param name="n"></param>
        /// <returns></returns>
		public static float3 Reflect(float3 v, float3 n)
		{
           
			Debug.Assert(v.IsNormalized);
			return v - 2 * Dot(v, n) * n;
		}
		
		public static bool Refract(float3 v, float3 n, float nint, out float3 outRefracted)
		{
			Debug.Assert(v.IsNormalized);
			float dt = Dot(v, n);
			float discr = 1.0f - nint * nint * (1 - dt * dt);
			if (discr > 0)
			{
				outRefracted = nint * (v - n * dt) - n * MathF.Sqrt(discr);
				Debug.Assert(outRefracted.IsNormalized);
				return true;
			}
			outRefracted = new float3(0, 0, 0);
			return false;
		}
	}

	public class MathUtil
	{
		public static float PI => 3.1415926f;

        /// <summary>
        /// Fresnel-Schlick算法：定义在不同观察方向上，表面上被反射的光除以被折射的光的比例。
        /// 在一束光击中了表面的一瞬间，菲涅尔根据表面与观察方向之间的夹角，计算得到光被反射的百分比。
        /// FSchlick(n,v,F0)=F0+(1−F0)(1−(n⋅v))5
        /// F0表示表面基础反射率，可利用Indices of refraction(IOR)求得。
        /// </summary>
        /// <param name="cosine"></param>
        /// <param name="ri"></param>
        /// <returns></returns>
		public static float Schlick(float cosine, float ri)
		{
			float r0 = (1 - ri) / (1 + ri);
			r0 = r0 * r0;
			return r0 + (1 - r0) * MathF.Pow(1 - cosine, 5);
		}

		static uint XorShift32(ref uint state)
		{
			uint x = state;
			x ^= x << 13;
			x ^= x >> 17;
			x ^= x << 15;
			state = x;
			return x;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //生成随机数
		public static float RandomFloat01(ref uint state)
		{
			return (XorShift32(ref state) & 0xFFFFFF) / 16777216.0f;
		}

		public static float3 RandomInUnitDisk(ref uint state)
		{
			float3 p;
			do
			{
				p = 2.0f * new float3(RandomFloat01(ref state), RandomFloat01(ref state), 0) - new float3(1, 1, 0);
			} while (p.SqLength >= 1.0);
			return p;
		}

		public static float3 RandomInUnitSphere(ref uint state)
		{
			float3 p;
			do
			{
				p = 2.0f * new float3(RandomFloat01(ref state), RandomFloat01(ref state), RandomFloat01(ref state)) - new float3(1, 1, 1);
			} while (p.SqLength >= 1.0);
			return p;
		}

		public static float3 RandomUnitVector(ref uint state)
		{
			float z = RandomFloat01(ref state) * 2.0f - 1.0f;
			float a = RandomFloat01(ref state) * 2.0f * PI;
			float r = MathF.Sqrt(1.0f - z * z);
			float x = MathF.Sin(a);
			float y = MathF.Cos(a);
			return new float3(r * x, r * y, z);
		}

	}

    //光线
	public struct Ray
	{
        //光线原点
		public float3 orig;
        //光线方向
		public float3 dir;

		public Ray(float3 orig_, float3 dir_)
		{
			Debug.Assert(dir_.IsNormalized);
			orig = orig_;
			dir = dir_;
		}

        //传播尺度
		public float3 PointAt(float t) { return orig + dir * t; }
	}

    //照射
	public struct Hit
	{
		public float3 pos;
		public float3 normal;
		public float t;
	}

	public struct Sphere
	{
		public float3 center;
		public float radius;
		public Sphere(float3 center_, float radius_) { center = center_; radius = radius_; }
	}

	struct Spheres
	{
		public float[] centerX;
		public float[] centerY;
		public float[] centerZ;
		public float[] sqRadius;
		public float[] invRadius;
		public int[] emissives;
		public int emissiveCount;

		public Spheres(int len)
		{
			centerX = new float[len];
			centerY = new float[len];
			centerZ = new float[len];
			sqRadius = new float[len];
			invRadius = new float[len];
			emissives = new int[len];
			emissiveCount = 0;
		}

		public void Update(Sphere[] src, Material[] mat)
		{
			emissiveCount = 0;
			for (var i = 0; i < src.Length; ++i)
			{
				ref Sphere s = ref src[i];
				centerX[i] = s.center.x;
				centerY[i] = s.center.y;
				centerZ[i] = s.center.z;
				sqRadius[i] = s.radius * s.radius;
				invRadius[i] = 1.0f / s.radius;
				if (mat[i].HasEmission)
				{
					emissives[emissiveCount++] = i;
				}
			}
		}

		public int HitSpheres(ref Ray r, float tMin, float tMax, ref Hit outHit)
		{
			float hitT = tMax;
			int id = -1;
			for (int i = 0; i < centerX.Length; ++i)
			{
				float coX = centerX[i] - r.orig.x;
				float coY = centerY[i] - r.orig.y;
				float coZ = centerZ[i] - r.orig.z;
				float nb = coX * r.dir.x + coY * r.dir.y + coZ * r.dir.z;
				float c = coX * coX + coY * coY + coZ * coZ - sqRadius[i];
				float discr = nb * nb - c;
				if (discr > 0)
				{
					float discrSq = MathF.Sqrt(discr);
					
					float t = nb - discrSq;
					if (t <= tMin)
						t = nb + discrSq;

					if (t > tMin && t < hitT)
					{
						id = i;
						hitT = t;
					}
				}
			}
			if (id != -1)
			{
				outHit.pos = r.PointAt(hitT);
				outHit.normal = (outHit.pos - new float3(centerX[id], centerY[id], centerZ[id])) * invRadius[id];
				outHit.t = hitT;
				return id;
			}
			else
				return -1;
		}
	}

	struct Camera
	{
        public Camera(float3 lookFrom, float3 lookAt, float3 vup, float vfov, float aspect, float aperture, float focusDist)
		{
			lensRadius = aperture / 2;
			float theta = vfov * MathUtil.PI / 180;
			float halfHeight = MathF.Tan(theta / 2);
			float halfWidth = aspect * halfHeight;
			origin = lookFrom;
			direction = float3.Normalize(lookFrom - lookAt);
			u = float3.Normalize(float3.Cross(vup, direction));
			v = float3.Cross(direction, u);
			lowerLeftCorner = origin - halfWidth * focusDist * u - halfHeight * focusDist * v - focusDist * direction;
			horizontal = 2 * halfWidth * focusDist * u;
			vertical = 2 * halfHeight * focusDist * v;
		}

		/// <summary>
		/// 生成追踪路径
		/// </summary>
		/// <param name="s"></param>
		/// <param name="t"></param>
		/// <param name="state"></param>
		/// <returns></returns>
		public Ray GetRay(float s, float t, ref uint state)
		{
			float3 rd = lensRadius * MathUtil.RandomInUnitDisk(ref state);
			float3 offset = u * rd.x + v * rd.y;
			return new Ray(origin + offset, float3.Normalize(lowerLeftCorner + s * horizontal + t * vertical - origin - offset));
		}

		float3 origin;
		float3 lowerLeftCorner;
		float3 horizontal;
		float3 vertical;
		float3 u, v, direction;
		float lensRadius;
	}

}
