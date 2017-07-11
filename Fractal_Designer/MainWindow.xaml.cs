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
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var sw = new Stopwatch();
            sw.Start();
            var solver = new Algorithms.IteratorSolver(new Algorithms.NewtonFractal());
            Fractal.Source = solver.CreateBitmapSource(0, 20, 20, (int) Fractal.Width, (int) Fractal.Height);
            Title = $"{sw.ElapsedMilliseconds} ms";
        }
    }
}
