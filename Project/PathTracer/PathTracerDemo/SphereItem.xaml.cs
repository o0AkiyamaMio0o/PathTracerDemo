using PathTracer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PathTracerDemo
{
	/// <summary>
	/// Sphere.xaml 的交互逻辑
	/// </summary>
	public partial class SphereItem : UserControl
	{
		private MainWindow mainWindow;

		public Sphere sphere;
		public Material material;
		
		public SphereItem(MainWindow mainWindow)
		{
			InitializeComponent();

			this.mainWindow = mainWindow;
			material = new Material(Material.Type.Lambert,new float3(0.8f,0.8f,0.8f),new float3(0,0,0),0,0);
			sphere = new Sphere(new float3(0,0,0),0.5f);

			initParams();
		}
		public SphereItem(MainWindow mainWindow,Sphere sphere,Material material)
		{
			InitializeComponent();

			this.mainWindow = mainWindow;
			this.material = material;
			this.sphere = sphere;

			initParams();
		}
		private void initParams()
		{
			xTextBox.Text = sphere.center.x.ToString();
			yTextBox.Text = sphere.center.y.ToString();
			zTextBox.Text = sphere.center.z.ToString();

			radiusTextBox.Text = sphere.radius.ToString();

			string[] types = new string[] { "兰伯特","金属","玻璃" };
			materialComboBox.ItemsSource = types;
			materialComboBox.SelectedIndex = (int)material.type;
		}
		public void setMaterialDetail(float3 reflectance,float3 emissive,float roughness,float ri)
		{
			material.reflectance = reflectance;
			material.emissive = emissive;
			material.roughness = roughness;
			material.ri = ri;
		}
		public Material getMaterial()
		{
			switch(materialComboBox.SelectedIndex)
			{
				case 0:
					material.type = Material.Type.Lambert;
					break;
				case 1:
					material.type = Material.Type.Metal;
					break;
				case 2:
					material.type = Material.Type.Dielectric;
					break;
			}

			return material;
		}
		public Sphere getSphere()
		{
			try
			{
				sphere.center.x = float.Parse(xTextBox.Text);
				sphere.center.y = float.Parse(yTextBox.Text);
				sphere.center.z = float.Parse(zTextBox.Text);
				sphere.radius = float.Parse(radiusTextBox.Text);
			}
			catch
			{
				sphere.center.x = 0;
				sphere.center.y = 0;
				sphere.center.z = 0;
				sphere.radius = 0.5f;
			}

			return sphere;
		}

		private void DeleteButton_Click(object sender, RoutedEventArgs e)
		{
			mainWindow.deleteSphereItem(this);
		}
		private void MaterialButton_Click(object sender, RoutedEventArgs e)
		{
			MaterialDetailWindow window = new MaterialDetailWindow(this);
			window.ShowDialog();
		}
	}
}
