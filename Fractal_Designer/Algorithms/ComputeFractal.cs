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

            double radiusReal = Settings.Instance.Radius;
            double radiusImaginary = Settings.Instance.Radius * lengthImaginary / lengthReal;

            if (lengthReal < lengthImaginary)
            {
                radiusReal = Settings.Instance.Radius * lengthReal / lengthImaginary;
                radiusImaginary = Settings.Instance.Radius;
            }

            if (center == null)
            {
                double realPosition = Settings.Instance.Center.Real + (re * 2d - lengthReal) / lengthReal * radiusReal;
                double imaginaryPosition = Settings.Instance.Center.Imaginary - (im * 2d - lengthImaginary) / lengthImaginary * radiusImaginary;
                return new Complex(realPosition, imaginaryPosition);
            }
            else
            {
                double realPosition = center.Value.Real + (re * 2d - lengthReal) / lengthReal * radiusReal;
                double imaginaryPosition = center.Value.Imaginary - (im * 2d - lengthImaginary) / lengthImaginary * radiusImaginary;
                return new Complex(realPosition, imaginaryPosition);
            }
        }
        private static Random r = new Random();
        private void ComputeFractal(Complex complexCoordinates)
        {
            switch ((DragEffect)Settings.Instance.drageffect)
            {
                case DragEffect.Move:
                    Settings.Instance.Center = CenterLastClicked - (GetComplexCoords(MouseLastMove, CenterLastClicked) - MouseLastClickedComplex);
                    break;
                case DragEffect.SingleRoot:
                    AlgorithmFunction = new ComplexFunction.ProductCF(Function,
                        new ComplexFunction.DifferenceCF(new ComplexFunction.ArgumentCF(), new ComplexFunction.ConstantCF(complexCoordinates)));
                    break;
                case DragEffect.DoubleRoot:
                    AlgorithmFunction = new ComplexFunction.ProductCF(Function,
                        new ComplexFunction.DifferenceCF(new ComplexFunction.ArgumentCF(), new ComplexFunction.ConstantCF(complexCoordinates)),
                        new ComplexFunction.DifferenceCF(new ComplexFunction.ArgumentCF(), new ComplexFunction.ConstantCF(complexCoordinates))
                        );
                    break;
                case DragEffect.Singularity:
                    AlgorithmFunction = new ComplexFunction.QuotientCF(Function,
                        new ComplexFunction.DifferenceCF(new ComplexFunction.ArgumentCF(), new ComplexFunction.ConstantCF(complexCoordinates)));
                    break;
                case DragEffect.Reset:
                    AlgorithmFunction = Function;
                    break;
                default:
                    break;
            }

            ComputeFractal();
        }

        private void ComputeFractal()
        {
            if (Settings.Instance.Center == null || double.IsNaN(Fractal.Width) || double.IsNaN(Fractal.Height) || Fractal.Width == 0 || Fractal.Height == 0)
                return;

            var fractalFactory = new ComplexFractalFactory();
            Colourer = new AlgorithmProcessor(fractalFactory.GetAutoConfiguredAlgorithmByID((Algorithm)Settings.Instance.algorithm, AlgorithmFunction ?? Function));

            AsyncDraw(Colourer, Settings.Instance.Center, Settings.Instance.Radius, (int)Fractal.Width, (int)Fractal.Height);

            Status.Text = $"Radius={Settings.Instance.Radius}";
        }

        private void RecomputeFractal() => ComputeFractal();

    }
}
