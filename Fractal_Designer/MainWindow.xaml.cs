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
        bool effect = false;
        FractalColourer colourer;
        SettingsWindow settingsWindow = null;
        ComplexFunction function = z => Complex.Cos(z) - 7;
        // odwrotna notacja polska zaimplementować


        public MainWindow()
        {
            InitializeComponent();

            Settings.Recompute += RecomputeFractal;

            // lookup on the internet how should the two cooperate in a good way
        }

        // change the size of the image appropriately!
        // or make it a canvas!
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e) => RecomputeFractal();

        private void RecomputeFractal() => RecomputeFractal(GetComplexCoords(Mouse.GetPosition(Fractal)));

        private void RecomputeFractal(Complex complexCoordinates)
        {
            if (Settings.Instance == null || double.IsNaN(Fractal.ActualWidth) || double.IsNaN(Fractal.ActualHeight) || Fractal.Width == 0 || Fractal.Height == 0)
                return;
            // also move the center!
            var algorithmFunction = function;

            if (effect && Settings.Instance.drageffect > 0)
            {
                switch (Settings.Instance.drageffect)
                {
                    case 1:
                        algorithmFunction = z => function(z) * (z - complexCoordinates);
                        break;
                    case 2:
                        algorithmFunction = z => function(z) * (z - complexCoordinates) * (z - complexCoordinates);
                        break;
                    case 3:
                        algorithmFunction = z => function(z) * (z.Magnitude - complexCoordinates.Magnitude);
                        break;
                    case 4:
                        algorithmFunction = z => function(z) / (z - complexCoordinates);
                        break;
                    default:
                        break;
                }
            }

            var fractalFactory = new ComplexFractalFactory();

            colourer = new FractalColourer(fractalFactory.GetAutoConfiguredAlgorithmByID(Settings.Instance.algorithm, algorithmFunction));

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

            var radius = (decimal) (((double) Settings.Instance.radius) * Math.Max(multiplier, 1d / (1 << 10)));

            if (radius == 0)
            {
                radius = 1;
            }

            Settings.Instance.radius = radius;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) => settingsWindow?.Close();

        private Complex GetComplexCoords(Point point)
        {
            int re = (int) point.X;
            int im = (int) point.Y;

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
            return new Complex(realPosition, imaginaryPosition);
        }

        private void Fractal_MouseMove(object sender, MouseEventArgs e)
        {
            var point = Mouse.GetPosition(Fractal);
            var complexCoordinates = GetComplexCoords(point);
            int re = (int) point.X;
            int im = (int) point.Y;
            if (re < 0 || im < 0 || re >= colourer.results.GetLength(0) || im >= colourer.results.GetLength(1))
                return;

            if (effect)
                RecomputeFractal(complexCoordinates);

            var result = colourer.results[re, im];

            if (result.succeeded)
                Status.Text = $"Radius={(double) Settings.Instance.radius}, Iterations={result.iterations}, Result={result.z}, f(Result)={function(result.z)}, Position={complexCoordinates}";
            else
                Status.Text = $"Radius={(double) Settings.Instance.radius}, Iterations={result.iterations} (fail), Position={complexCoordinates}";
        }

        private void OpenSettings(object sender, RoutedEventArgs e)
        {
            if (settingsWindow == null || !settingsWindow.IsLoaded)
            {
                settingsWindow = new SettingsWindow()
                {
                    Owner = this
                };

                settingsWindow.Show();
            }
            else
                settingsWindow.Activate();
        }

        private void Exit(object sender, RoutedEventArgs e) => Close();

        private void Fractal_MouseLeave(object sender, MouseEventArgs e)
        {
            effect = false;
            Status.Text = "";
        }

        private void Fractal_MouseUp(object sender, MouseButtonEventArgs e) => effect = false;

        private void Fractal_MouseDown(object sender, MouseButtonEventArgs e) => effect = true;
    }
}
