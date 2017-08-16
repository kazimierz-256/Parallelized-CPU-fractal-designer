using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
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
    public class PowerOfTwoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => $"2 ^ {value.ToString()}";
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
    }

    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            DataContext = Settings.Instance;
            InitializeComponent();

            foreach (var DragEffectName in Enum.GetNames(typeof(DragEffect)))
                DragEffectComboBox.Items.Add(new ComboBoxItem() { Content = DragEffectName.Replace('_', ' ') });

            foreach (var AlgorithmName in Enum.GetNames(typeof(Algorithm)))
                AlgorithmComboBox.Items.Add(new ComboBoxItem() { Content = AlgorithmName.Replace('_', ' ') });
        }

        private void Reset(object sender, RoutedEventArgs e)
        {
            Settings.Reset();
            DataContext = Settings.Instance;
        }
    }
}
