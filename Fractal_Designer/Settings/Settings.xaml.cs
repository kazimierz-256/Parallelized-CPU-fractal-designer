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
        private Action<Settings> UpdateFractalDelegate;
        private Settings programSettings;

        public SettingsWindow()
        {
            InitializeComponent();
        }

        public SettingsWindow(Settings programSettings, Action<Settings> UpdateFractalDelegate)
        {
            this.programSettings = programSettings;
            this.UpdateFractalDelegate = UpdateFractalDelegate;

            InitializeComponent();

            Iterations.Value = programSettings.iterations;
            Parameter.Value = (double) programSettings.parameter;

            Iterations.ValueChanged += UpdateFractal;
            Parameter.ValueChanged += UpdateFractal;

        }

        private void UpdateFractal(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            programSettings.iterations = (ushort) Iterations.Value;

            programSettings.parameter = (decimal) Parameter.Value;

            UpdateFractalDelegate?.Invoke(programSettings);
        }
    }
}
