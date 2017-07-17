using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
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

namespace Fractal_Designer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SettingsWindow settingsWindow = null;
        FractalColourer colourer;

        public MainWindow()
        {
            InitializeComponent();
            // lookup on the internet how should the two cooperate in a good way
        }

        // change the size of the image appropriately!
        // or make it a canvas!
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e) => RecomputeFractal();

        private void RecomputeFractal()
        {
            if (Settings.Instance == null || double.IsNaN(Fractal.ActualWidth) || double.IsNaN(Fractal.ActualHeight) || Fractal.Width == 0 || Fractal.Height == 0)
                return;

            ComplexFunction function = z => z * (z - 1) * (z + 1) * (z - 2) * (z + 2) * (z - 4) * (z + 4);

            var fractalFactory = new ComplexFractalFactory();
            colourer = new FractalColourer(fractalFactory.GetAlgorithmByName(Settings.Instance.algorithm, (int) Settings.Instance.iterations, function));

            var sw = new Stopwatch();
            sw.Start();

            Fractal.Source = colourer.CreateBitmapSource(new Complex((double) Settings.Instance.centerreal, (double) Settings.Instance.centerimaginary), (double) Settings.Instance.radius, (int) Fractal.Width, (int) Fractal.Height);

            sw.Stop();
            Title = $"{sw.ElapsedMilliseconds} ms {(int) Fractal.ActualWidth}";
            Status.Text = $"Radius={(double) Settings.Instance.radius}";
        }

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double multiplier = Math.Pow(.5, e.Delta / 100);

            Settings.Instance.radius = (decimal) (((double) Settings.Instance.radius) * Math.Max(multiplier, 1d / (1 << 10)));

            if (Settings.Instance.radius == 0)
            {
                Settings.Instance.radius = 1;
            }

            RecomputeFractal();
            Settings.Save(Settings.Instance);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) => settingsWindow?.Close();

        private void Fractal_MouseMove(object sender, MouseEventArgs e)
        {
            var point = Mouse.GetPosition(Fractal);
            int re = (int) point.X;
            int im = (int) point.Y;
            if (re < 0 || im < 0 || re >= colourer.results.GetLength(0) || im >= colourer.results.GetLength(1))
                return;

            var result = colourer.results[re, im];

            int lengthReal = (int) Fractal.ActualWidth;
            int lengthImaginary = (int) Fractal.ActualHeight;

            double radiusReal = (double) Settings.Instance.radius;
            double radiusImaginary = ((double) Settings.Instance.radius) * lengthImaginary / lengthReal;
            if (lengthReal < lengthImaginary)
            {
                radiusReal = ((double) Settings.Instance.radius) * lengthReal / lengthImaginary;
                radiusImaginary = (double) Settings.Instance.radius;
            }
            double realPosition = ((double) Settings.Instance.centerreal) + (re * 2d - lengthReal) / lengthReal * radiusReal;
            double imaginaryPosition = ((double) Settings.Instance.centerimaginary) + (im * 2d - lengthImaginary) / lengthImaginary * radiusImaginary;

            Status.Text = $"Radius={(double) Settings.Instance.radius}, Iterations={result.iterations}, Result={result.z.Real} + i{result.z.Imaginary}, Mouse=[{point.X}, {point.Y}], Position={realPosition} + i{imaginaryPosition}";
        }

        private void OpenSettings(object sender, RoutedEventArgs e)
        {
            if (settingsWindow == null || !settingsWindow.IsLoaded)
            {
                settingsWindow = new SettingsWindow(() =>
                {
                    RecomputeFractal();
                    Settings.Save(Settings.Instance);
                });
                settingsWindow.Show();
            }
            else
                settingsWindow.Activate();
        }

        private void Exit(object sender, RoutedEventArgs e) => Close();
    }
}
