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
using System.Windows.Shapes;

namespace PathTracerDemo
{
	/// <summary>
	/// MaterialDetailWindow.xaml 的交互逻辑
	/// </summary>
	public partial class MaterialDetailWindow : Window
	{
		SphereItem sphere;
		public MaterialDetailWindow(SphereItem sphere)
		{
			InitializeComponent();

			this.sphere = sphere;
			initParams();
		}
		private void initParams()
		{
			if(sphere==null)
			{
#if DEBUG
				throw new Exception();
#else
				this.Close();
				return;
#endif
			}

			//设置反射率初始值
			albedoXTextBox.Text = sphere.material.reflectance.x.ToString();
			albedoYTextBox.Text = sphere.material.reflectance.y.ToString();
			albedoZTextBox.Text = sphere.material.reflectance.z.ToString();

			//设置发光度初始值
			emissiveXTextBox.Text = sphere.material.emissive.x.ToString();
			emissiveYTextBox.Text = sphere.material.emissive.y.ToString();
			emissiveZTextBox.Text = sphere.material.emissive.z.ToString();

			//设置粗糙度初始值
			roughnessTextBox.Text = sphere.material.roughness.ToString();

			//设置ri初始值
			riTextBox.Text = sphere.material.ri.ToString();
		}

		private bool checkParams()
		{
			float temp;

			if(!float.TryParse(albedoXTextBox.Text,out temp)||temp<0||temp>1)
			{
				MessageBox.Show("反光率设置有误","范围应为0 ~ 1");
				return false;
			}else if (!float.TryParse(albedoYTextBox.Text, out temp) || temp < 0 || temp > 1)
			{
				MessageBox.Show("反光率设置有误", "范围应为0 ~ 1");
				return false;
			}else if (!float.TryParse(albedoZTextBox.Text, out temp) || temp < 0 || temp > 1)
			{
				MessageBox.Show("反光率设置有误", "范围应为0 ~ 1");
				return false;
			}else if (!float.TryParse(emissiveXTextBox.Text, out temp) || temp < 0)
			{
				MessageBox.Show("发光率设置有误","应不小于0");
				return false;
			}else if (!float.TryParse(emissiveYTextBox.Text, out temp) || temp < 0)
			{
				MessageBox.Show("发光率设置有误", "应不小于0");
				return false;
			}else if (!float.TryParse(emissiveZTextBox.Text, out temp) || temp < 0)
			{
				MessageBox.Show("发光率设置有误", "应不小于0");
				return false;
			}else if (!float.TryParse(roughnessTextBox.Text, out temp) || temp < 0)
			{
				MessageBox.Show("粗糙度设置有误", "应不小于0");
				return false;
			}else if (!float.TryParse(riTextBox.Text, out temp) || temp < 0)
			{
				MessageBox.Show("ri设置有误", "应不小于0");
				return false;
			}

			return true;
		}
		private void OkButton_Click(object sender, RoutedEventArgs e)
		{
			if (!checkParams())
				return;

			sphere.setMaterialDetail(new float3(float.Parse(albedoXTextBox.Text), float.Parse(albedoYTextBox.Text), float.Parse(albedoZTextBox.Text)), new float3(float.Parse(emissiveXTextBox.Text), float.Parse(emissiveYTextBox.Text), float.Parse(emissiveZTextBox.Text)), float.Parse(roughnessTextBox.Text), float.Parse(riTextBox.Text));
			this.Close();
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}
	}
}
