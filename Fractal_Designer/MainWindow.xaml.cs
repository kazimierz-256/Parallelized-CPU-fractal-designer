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
using System.Threading;

namespace Fractal_Designer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SettingsWindow SettingsWindow;
        Complex MouseLastClickComplex;
        Point MouseLastClicked;
        Point MouseLastMoved;
        Complex MouseLastMovedComplex;
        // odwrotna notacja polska zaimplementować

        public MainWindow()
        {
            InitializeComponent();

            Settings.Recompute += ComputeFractal;

            // lookup on the internet how should the two cooperate in a good way
        }

        private void Fractal_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                MouseLastClicked = Mouse.GetPosition(Fractal);
                MouseLastClickComplex = GetComplexCoords(MouseLastClicked);
                ComputeFractal(MouseLastClickComplex);
            }
        }

        private void Fractal_MouseMove(object sender, MouseEventArgs e)
        {
            Fractal.Cursor = Settings.Instance.drageffect == 0 ? Cursors.Hand : Cursors.Cross;

            MouseLastMoved = Mouse.GetPosition(Fractal);
            MouseLastMovedComplex = GetComplexCoords(MouseLastMoved);

            if (Mouse.LeftButton == MouseButtonState.Pressed)
                ComputeFractal(GetComplexCoords(MouseLastMoved));

            int re = (int) MouseLastMoved.X;
            int im = (int) MouseLastMoved.Y;
            var results = ((BitmapSourceResult) (sender as Image).Tag).results;
            if (results == null || re < 0 || im < 0 || re >= results.GetLength(0) || im >= results.GetLength(1))
                return;

            var result = results[re, im];

            if (result.succeeded)
                Status.Text = $"Radius={(double) Settings.Instance.radius}, Iterations={result.iterations}, Result={result.z}, f(Result)={Function(result.z)}, Position={MouseLastMovedComplex}";
            else
                Status.Text = $"Radius={(double) Settings.Instance.radius}, Iterations={result.iterations} (fail), Position={MouseLastMovedComplex}";
        }

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double multiplier = Math.Max(Math.Pow(.5, e.Delta / 100), 1d / (1 << 10));

            double radius = (double) Settings.Instance.radius;

            if (radius == 0 || double.IsInfinity(radius) || double.IsNaN(radius))
                radius = 1;

            radius *= multiplier;

            Settings.Instance.radius = (decimal) radius;

            // to allow proportional zooming to points of interest
            Settings.Instance.center += (MouseLastMovedComplex - Settings.Instance.center) * (1 - multiplier);

        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Fractal.Width = Width - 40;
            Fractal.Height = Height - 80;
            ComputeFractal();
        }

        private void OpenSettings(object sender, RoutedEventArgs e)
        {
            if (SettingsWindow == null || !SettingsWindow.IsLoaded)
            {
                SettingsWindow = new SettingsWindow()
                {
                    Owner = this
                };

                SettingsWindow.Show();
            }
            else
                SettingsWindow.Activate();
        }


        private void Fractal_MouseLeave(object sender, MouseEventArgs e) => Status.Text = "";

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) => SettingsWindow?.Close();

        private void Exit(object sender, RoutedEventArgs e) => Close();

    }
}
