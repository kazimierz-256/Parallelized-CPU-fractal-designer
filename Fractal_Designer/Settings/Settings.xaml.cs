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

namespace Fractal_Designer
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private event Action UpdateFractalDelegate;

        public SettingsWindow()
        {
            InitializeComponent();
        }

        public SettingsWindow(Action UpdateFractalDelegate)
        {
            this.UpdateFractalDelegate += UpdateFractalDelegate;

            InitializeComponent();

            Iterations.Value = Settings.Instance.iterations;
            Parameter.Value = (double) Settings.Instance.parameter;

            Iterations.ValueChanged += UpdateFractal;
            Parameter.ValueChanged += UpdateFractal;

        }

        private void UpdateFractal(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Settings.Instance.iterations = (ushort) Iterations.Value;

            Settings.Instance.parameter = (decimal) Parameter.Value;

            // enumerate all settings

            UpdateFractalDelegate();
        }
    }
}
