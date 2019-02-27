using Microsoft.Win32;
using PathTracer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;

namespace PathTracerDemo
{
	/// <summary>
	/// MainWindow.xaml 的交互逻辑
	/// </summary>
	public partial class MainWindow : Window
	{
		public static Dispatcher dispatcher;

		public static int width;
		public static int height;
		public static int iterationTime;
		public static float[] buffer;
		public static Scene scene;

		public static Thread pathTracingThread;

		public MainWindow()
		{
			InitializeComponent();

			dispatcher = Dispatcher.CurrentDispatcher;

			Scene defaultScene = Scene.defaultScene();
			for (int i = 0; i < defaultScene.spheres.Length; i++)
			{
				SpheresGrid.Children.Add(new SphereItem(this,defaultScene.spheres[i],defaultScene.materials[i]));
			}
		}

		private void clearProgressBar(int max)
		{
			progressBar.Maximum = max;
			progressBar.Value = 0;
			progressText.Text = "计算中 - " + progressBar.Value.ToString() + "/" + progressBar.Maximum.ToString();
		}
		private void addProgressBar()
		{
			progressBar.Value++;
			if (progressBar.Value < progressBar.Maximum)
				progressText.Text = "计算中 - " + progressBar.Value.ToString() + "/" + progressBar.Maximum.ToString();
			else
				progressOK();

			loadResultImg();
		}
		private void progressOK()
		{
			progressText.Text = "计算完成";
			MessageBox.Show("计算完成");
		}
		private bool checkParams()
		{
			int temp;

			if (!int.TryParse(iterationTimeTextBox.Text, out temp) || temp < 0)
			{
				MessageBox.Show("迭代次数设置有误", "启动失败", MessageBoxButton.OK);
				return false;
			}

			return true;
		}
		private Scene getScene()
		{
			List<Sphere> spheres = new List<Sphere>();
			List<Material> materials = new List<Material>();

			foreach(SphereItem sphere in SpheresGrid.Children)
			{
				spheres.Add(sphere.getSphere());
				materials.Add(sphere.getMaterial());
			}

			return new Scene(spheres.ToArray(),materials.ToArray());
		}

		private void pathTracing()
		{
			//检查参数设置是否有误
			if (!checkParams())
				return;

			scene = getScene();
			iterationTime = int.Parse(iterationTimeTextBox.Text);

			clearProgressBar(iterationTime);

			width = (int)imageGrid.ActualWidth;
			height = (int)imageGrid.ActualHeight;

			if (width <= 0 || width > 10000 || height <= 0 || height > 10000)
			{
				width = 600;
				height = 400;
			}

			buffer = new float[width * height * 3];

			if (pathTracingThread != null && pathTracingThread.IsAlive)
				pathTracingThread.Abort();

			Thread thread = new Thread(pathTracing_Thread);
			thread.Start();

			pathTracingThread = thread;
		}
		private void pathTracing_Thread()
		{
			Tracer test = new Tracer(MainWindow.scene);

			for (int i = 0; i < iterationTime; ++i)
			{
				test.DrawTest(i, MainWindow.width, MainWindow.height, MainWindow.buffer);

				MainWindow.dispatcher.BeginInvoke(new Action(delegate
				{
					addProgressBar();
				}));

				Console.WriteLine($"iteration - {i + 1} OK");
			}
		}

		static int constraint(float x)
		{
			x *= 256;
			if (x < 0)
				x = 0;
			else if (x > 255)
				x = 255;

			return (int)x;
		}

		private void loadResultImg()
		{
			float[] tempBuffer = buffer.Clone() as float[];

			if(tempBuffer == null)
			{
#if DEBUG
				throw new Exception();
#else
				return;
#endif
			}

			Bitmap bitmap = new Bitmap(width, height);

			try
			{
				for (int x = 0; x < width; x++)
					for (int y = 0; y < height; y++)
						bitmap.SetPixel(x, (height - y - 1), System.Drawing.Color.FromArgb(255, constraint(tempBuffer[(x + y * width) * 3]), constraint(tempBuffer[(x + y * width) * 3 + 1]), constraint(tempBuffer[(x + y * width) * 3 + 2])));

				resultImage.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());//new BitmapImage(new Uri(Directory.GetCurrentDirectory().ToString() + "\\output.tga", UriKind.Absolute));
			}
			catch (Exception ex)
			{
				MessageBox.Show("显示出错");

#if DEBUG
				throw ex;
#else
				Console.WriteLine(ex);
#endif
			}
		}

		private void StartButton_Click(object sender, RoutedEventArgs e)
		{
			pathTracing();
		}

		public void deleteSphereItem(SphereItem item)
		{
			//UIElement element = null;

			//foreach(var child in SpheresGrid.Children)
			//	if(child is SphereItem||(child as SphereItem).id == id)
			//	{
			//		element = child as UIElement;
			//		break;
			//	}

			//if (element != null)
			//	SpheresGrid.Children.Remove(element);
			SpheresGrid.Children.Remove(item);
		}

		private void AddSphereButton_Click(object sender, RoutedEventArgs e)
		{
			SpheresGrid.Children.Add(new SphereItem(this));
		}
	}
}
