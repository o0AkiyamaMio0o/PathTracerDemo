using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathTracer
{
	public class Scene
	{
		public Sphere[] spheres;
		public Material[] materials;

		public Scene() { }
		public Scene(Sphere[] spheres,Material[] materials)
		{
			this.spheres = spheres;
			this.materials = materials;
		}
		public static Scene defaultScene()
		{
			Scene scene = new Scene();

			scene.spheres = new Sphere[]{
				new Sphere(new float3(0, -100.5f, -1), 100),
				new Sphere(new float3(2, 0, -1), 0.5f),
				new Sphere(new float3(0, 0, -1), 0.5f),
				new Sphere(new float3(-2, 0, -1), 0.5f),
				new Sphere(new float3(2, 0, 1), 0.5f),
				new Sphere(new float3(0, 0, 1), 0.5f),
				new Sphere(new float3(-2, 0, 1), 0.5f),
				new Sphere(new float3(0.5f, 1, 0.5f), 0.5f),
				new Sphere(new float3(-1.5f, 1.5f, 0f), 0.3f),
			};

			scene.materials = new Material[]{
				new Material(Material.Type.Lambert,     new float3(0.8f, 0.8f, 0.8f), new float3(0,0,0), 0, 0),
				new Material(Material.Type.Lambert,     new float3(0.8f, 0.4f, 0.4f), new float3(0,0,0), 0, 0),
				new Material(Material.Type.Lambert,     new float3(0.4f, 0.8f, 0.4f), new float3(0,0,0), 0, 0),
				new Material(Material.Type.Metal,       new float3(0.4f, 0.4f, 0.8f), new float3(0,0,0), 0, 0),
				new Material(Material.Type.Metal,       new float3(0.4f, 0.8f, 0.4f), new float3(0,0,0), 0, 0),
				new Material(Material.Type.Metal,       new float3(0.4f, 0.8f, 0.4f), new float3(0,0,0), 0.2f, 0),
				new Material(Material.Type.Metal,       new float3(0.4f, 0.8f, 0.4f), new float3(0,0,0), 0.6f, 0),
				new Material(Material.Type.Dielectric,  new float3(0.4f, 0.4f, 0.4f), new float3(0,0,0), 0, 1.5f),
				new Material(Material.Type.Lambert,     new float3(0.8f, 0.6f, 0.2f), new float3(30,25,15), 0, 0),
			};

			return scene;
		}
	}
}
