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
        Complex MouseLastClickedComplex;
        Point MouseLastClicked;
        Point MouseLastMove;
        Complex MouseLastMovedComplex;
        Complex CenterLastClicked;

        public MainWindow()
        {
            InitializeComponent();

            Settings.Recompute += RecomputeFractal;
        }

        private void Fractal_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                CenterLastClicked = Settings.Instance.Center;
                MouseLastMove = MouseLastClicked = Mouse.GetPosition(Fractal);
                MouseLastMovedComplex = MouseLastClickedComplex = GetComplexCoords(MouseLastClicked);

                if ((DragEffect)Settings.Instance.drageffect != DragEffect.Move)
                    ComputeFractal(MouseLastClickedComplex);
            }
        }

        private void Fractal_MouseMove(object sender, MouseEventArgs e)
        {
            Fractal.Cursor = Settings.Instance.drageffect == 0 ? Cursors.Hand : Cursors.Cross;

            MouseLastMove = Mouse.GetPosition(Fractal);
            MouseLastMovedComplex = GetComplexCoords(MouseLastMove);

            // recompute not so often...
            if (Mouse.LeftButton == MouseButtonState.Pressed)
                ComputeFractal(MouseLastMovedComplex);

            var bitmapSourceResult = (BitmapSourceResult)(sender as Image).Tag;
            int re = (int)(MouseLastMove.X * (bitmapSourceResult.bitmap.PixelWidth) / Fractal.Width);
            int im = (int)(MouseLastMove.Y * (bitmapSourceResult.bitmap.PixelHeight) / Fractal.Height);

            if (bitmapSourceResult.results == null || re < 0 || im < 0 || re >= bitmapSourceResult.results.GetLength(0) || im >= bitmapSourceResult.results.GetLength(1))
                return;

            var result = bitmapSourceResult.results[re, im];

            if (result.succeeded)
                Status.Text = $"Radius={Settings.Instance.Radius}{Environment.NewLine}Iterations={result.iterations}{Environment.NewLine}Result={result.z}{Environment.NewLine}f(Result)={Function.Compute(result.z)}{Environment.NewLine}Position={MouseLastMovedComplex}";
            else
                Status.Text = $"Radius={Settings.Instance.Radius}{Environment.NewLine}Iterations={result.iterations} (fail){Environment.NewLine}Position={MouseLastMovedComplex}";
        }

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double multiplier = Math.Max(Math.Pow(.5, e.Delta / 100), 1d / (1 << 10));

            double radius = Settings.Instance.Radius;

            if (radius == 0 || double.IsInfinity(radius) || double.IsNaN(radius))
                radius = 1;

            radius *= multiplier;

            Settings.Instance.Radius = radius;

            // to allow proportional zooming to points of interest but only if zooming in
            if (multiplier < 1)
                Settings.Instance.Center += (MouseLastMovedComplex - Settings.Instance.Center) * (1 - multiplier);

        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Fractal.Width = Hope.ActualWidth;
            Fractal.Height = Hope.ActualHeight;
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


        private void Fractal_MouseLeave(object sender, MouseEventArgs e) => Status.Text = string.Empty;

        private void Exit(object sender, RoutedEventArgs e) => Application.Current?.Shutdown();

        private void Interpret(object sender = null, TextChangedEventArgs e = null)
        {
            if (!IsLoaded)
                return;

            var succeeded = Interpreter.TryParse(Formula.Text, out ComplexFunction.ComplexFunction result);

            if (succeeded)
            {
                Function = result;
                RecomputeFractal();
                Formula.Foreground = Brushes.Black;
            }
            else
            {
                Formula.Foreground = Brushes.Red;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) => Interpret();
    }
}
