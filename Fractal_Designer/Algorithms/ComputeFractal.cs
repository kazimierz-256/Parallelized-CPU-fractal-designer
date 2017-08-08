using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Fractal_Designer
{
    partial class MainWindow
    {
        ComplexFunction.ComplexFunction Function = ComplexFunction.Generator.Generate("z");
        ComplexFunction.ComplexFunction AlgorithmFunction;
        AlgorithmProcessor Colourer;

        private Complex GetComplexCoords(Point point, Complex? center = null)
        {
            var re = point.X;
            var im = point.Y;

            int lengthReal = (int)Fractal.Width;
            int lengthImaginary = (int)Fractal.Height;

            double radiusReal = (double)Settings.Instance.radius;
            double radiusImaginary = ((double)Settings.Instance.radius) * lengthImaginary / lengthReal;

            if (lengthReal < lengthImaginary)
            {
                radiusReal = ((double)Settings.Instance.radius) * lengthReal / lengthImaginary;
                radiusImaginary = (double)Settings.Instance.radius;
            }

            if (center == null)
            {
                double realPosition = Settings.Instance.center.Real + (re * 2d - lengthReal) / lengthReal * radiusReal;
                double imaginaryPosition = Settings.Instance.center.Imaginary + (im * 2d - lengthImaginary) / lengthImaginary * radiusImaginary;
                return new Complex(realPosition, imaginaryPosition);
            }
            else
            {
                double realPosition = center.Value.Real + (re * 2d - lengthReal) / lengthReal * radiusReal;
                double imaginaryPosition = center.Value.Imaginary + (im * 2d - lengthImaginary) / lengthImaginary * radiusImaginary;
                return new Complex(realPosition, imaginaryPosition);
            }
        }
        private static Random r = new Random();
        private void ComputeFractal(Complex complexCoordinates)
        {
            // consider reverting AlgorithmFunction to the original sometime
            switch ((DragEffect)Settings.Instance.drageffect)
            {
                case DragEffect.Move:
                    Settings.Instance.center = CenterLastClicked - (GetComplexCoords(MouseLastMove, CenterLastClicked) - MouseLastClickedComplex);
                    break;
                //case DragEffect.SingleRoot:
                //    AlgorithmFunction = new ArbitraryComplexFunction(z => Function.Compute(z) * (z - complexCoordinates));
                //    break;
                //case DragEffect.DoubleRoot:
                //    AlgorithmFunction = new ArbitraryComplexFunction(z => Function.Compute(z) * (z - complexCoordinates) * (z - complexCoordinates));
                //    break;
                //case DragEffect.CircularRoot:
                //    AlgorithmFunction = new ArbitraryComplexFunction(z => Function.Compute(z) * (z.Magnitude - complexCoordinates.Magnitude));
                //    break;
                //case DragEffect.Singularity:
                //    AlgorithmFunction = new ArbitraryComplexFunction(z => Function.Compute(z) / (z - complexCoordinates));
                //    break;
                default:
                    break;
            }

            ComputeFractal();
        }

        private void ComputeFractal()/* => ComputeFractal(GetComplexCoords(Mouse.GetPosition(Fractal)));*/
        {
            if (Settings.Instance.center == null || double.IsNaN(Fractal.Width) || double.IsNaN(Fractal.Height) || Fractal.Width == 0 || Fractal.Height == 0)
                return;

            var fractalFactory = new ComplexFractalFactory();
            Colourer = new AlgorithmProcessor(fractalFactory.GetAutoConfiguredAlgorithmByID((Algorithm)Settings.Instance.algorithm, AlgorithmFunction ?? Function));

            AsyncDraw(Colourer, Settings.Instance.center, (double)Settings.Instance.radius, (int)Fractal.Width, (int)Fractal.Height);

            Status.Text = $"Radius={(double)Settings.Instance.radius}";
        }

        private void RecomputeFractal() => ComputeFractal();

    }
}
