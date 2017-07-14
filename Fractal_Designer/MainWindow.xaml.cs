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
        Settings programSettings = null;

        Algorithms.FractalColourer solver;

        public MainWindow()
        {
            InitializeComponent();
            Settings.Load(out programSettings);
            settingsWindow = new SettingsWindow(programSettings, (newSettings) =>
            {
                programSettings = newSettings;
                UpdateFractal();
                Settings.Save(programSettings);
            });
            settingsWindow.Show();
            // lookup on the internet how should the two cooperate in a good way
        }

        Complex center = 0;
        double radius = 20;


        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Fractal.Width = Width - 20;
            Fractal.Height = Height - 20;
            UpdateFractal();
        }

        private void UpdateFractal()
        {
            if (double.IsNaN(Fractal.Width) || double.IsNaN(Fractal.Height))
                return;

            var sw = new Stopwatch();
            sw.Start();

            Func<Complex, Complex> function = z => z * (z - 1) * (z + 1);

            Algorithms.IFractalAlgorithm algorithm = new Algorithms.KFractal(programSettings.iterations, (double) programSettings.parameter, function);

            solver = new Algorithms.FractalColourer(algorithm);

            Fractal.Source = solver.CreateBitmapSource(center, radius, (int) Fractal.Width, (int) Fractal.Height);

            sw.Stop();
            Title = $"{sw.ElapsedMilliseconds} ms";
        }

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double multiplier = Math.Pow(.5, e.Delta / 100);
            radius *= Math.Max(multiplier, 1d / (1 << 10));
            UpdateFractal();

            Title += radius.ToString();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            settingsWindow?.Close();
        }
    }
}
