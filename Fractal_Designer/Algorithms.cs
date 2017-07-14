using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Fractal_Designer
{
    partial class Algorithms
    {
        public interface IFractalAlgorithm
        {
            (Complex, int, bool) Compute(Complex z);
        }

        public class KFractal : IFractalAlgorithm
        {
            int MaximumIterationCount;
            double parameter;
            double eps44 = Math.Pow(2, -44);
            double eps11 = Math.Pow(2, -11);
            double eps20 = Math.Pow(2, -20);
            Func<Complex, Complex>[] Derivatives;

            public (Complex, int, bool) Compute(Complex z)
            {
                int iterationsLeft = MaximumIterationCount;
                Complex delta;
                Complex zz, zzz, fz, fzz, fzzz;
                zzz = z;
                fzzz = Derivatives[0](z);

                zz = z * (1 - eps11);
                fzz = Derivatives[0](zz);

                z = z - z * eps11 * fzz / (fzzz - fzz);
                fz = Derivatives[0](z);

                if (double.IsNaN(z.Real) || double.IsNaN(z.Imaginary))
                    return (z, MaximumIterationCount - iterationsLeft, false);

                do
                {
                    delta = fz / (parameter * ((fzz - fz) * (zzz - z) * (zzz - z) - (fzzz - fz) * (zz - z) * (zz - z)) / ((zz - z) * (zzz - z) * (zzz - zz)) + (1 - parameter) * (fzz - fz) / (zz - z));

                    if (double.IsNaN(delta.Real) || double.IsNaN(delta.Imaginary))
                        return (z, MaximumIterationCount - iterationsLeft, false);

                    zzz = zz;
                    fzzz = fzz;
                    zz = z;
                    fzz = fz;
                    z -= delta;
                    fz = Derivatives[0](z);

                } while (--iterationsLeft >= 0 && Complex.Abs(delta) > Math.Max(eps44, Complex.Abs(z) * eps20));

                return (z, MaximumIterationCount - iterationsLeft, true);
            }

            public KFractal(int MaximumIterationCount, double parameter = 1, params Func<Complex, Complex>[] Derivatives)
            {
                this.MaximumIterationCount = MaximumIterationCount;
                this.parameter = parameter;
                this.Derivatives = Derivatives;
            }
        }

        public class NewtonFractal : IFractalAlgorithm
        {
            int MaximumIterationCount;
            double eps44 = Math.Pow(2, -44);
            double eps20 = Math.Pow(2, -20);
            double eps10 = Math.Pow(2, -10);
            Func<Complex, Complex>[] Derivatives;

            public (Complex, int, bool) Compute(Complex z)
            {
                int iterationsLeft = MaximumIterationCount;
                Complex delta;

                do
                {
                    delta = Derivatives[0](z) / Derivatives[1](z);
                    if (double.IsNaN(delta.Real) || double.IsNaN(delta.Imaginary))
                        return (z, MaximumIterationCount - iterationsLeft, false);
                    z -= delta;
                } while (--iterationsLeft >= 0 && Complex.Abs(delta) > Math.Max(eps44, Complex.Abs(z) * eps20));

                return (z, MaximumIterationCount - iterationsLeft, true);
            }

            public NewtonFractal(int MaximumIterationCount, params Func<Complex, Complex>[] Derivatives)
            {
                this.MaximumIterationCount = MaximumIterationCount;
                this.Derivatives = Derivatives;
            }
        }

        public class FractalColourer
        {
            IFractalAlgorithm fractalAlgorithm;

            public BitmapSource CreateBitmapSource(Complex center, double radius, int lengthReal, int lengthImaginary)
            {
                double eps10 = Math.Pow(2, -10);

                PixelFormat pixelFormat = PixelFormats.Bgr32;
                int stride = (lengthReal * pixelFormat.BitsPerPixel + 7) / 8;
                var fractal = new byte[stride * lengthImaginary];

                double radiusReal = radius;
                double radiusImaginary = radius * lengthImaginary / lengthReal;

                // best-fit window, if in portrait mode
                if (lengthReal < lengthImaginary)
                {
                    radiusReal = radius * lengthReal / lengthImaginary;
                    radiusImaginary = radius;
                }

                Parallel.For(0, lengthReal, re =>
                {
                    for (int im = 0; im < lengthImaginary; ++im)
                    {
                        // compute the location like in a grid (could be image-based but who wants it?)
                        double realPosition = center.Real + (re * 2d - lengthReal) / lengthReal * radiusReal;
                        double imaginaryPosition = center.Imaginary + (im * 2d - lengthImaginary) / lengthImaginary * radiusImaginary;

                        // compute the end result
                        (Complex z, int iterations, bool succeeded) = fractalAlgorithm.Compute(new Complex(realPosition, imaginaryPosition));

                        if (succeeded)
                        {
                            double abs = Complex.Abs(z);
                            double hue = 180d * (z.Phase + Math.PI) / Math.PI;//(360/2) * ..., don't worry, overflow is allowed
                            int iterationsSquared = iterations * iterations;
                            double saturation = 700d / (800d + iterationsSquared) * (1 - eps10 / (eps10 + abs * abs));
                            double value = 180d / (200d + iterationsSquared);

                            (byte R, byte G, byte B) color = ColorFromHSV(hue, saturation, value);

                            fractal[4 * (lengthReal * im + re)] = color.B;
                            fractal[4 * (lengthReal * im + re) + 1] = color.G;
                            fractal[4 * (lengthReal * im + re) + 2] = color.R;
                        }
                        // else zeroes

                    }
                });

                return BitmapSource.Create(lengthReal, lengthImaginary, 96, 96, pixelFormat, null, fractal, stride);
            }

            public FractalColourer(IFractalAlgorithm fractalAlgorithm)
            {
                if (fractalAlgorithm == null)
                {
                    throw new ArgumentNullException("Brak algorytmu.");
                }

                this.fractalAlgorithm = fractalAlgorithm;
            }

        }

        public static (byte R, byte G, byte B) ColorFromHSV(double hue, double saturation, double value)
        {
            var hi = (byte) ((int) (hue / 60) % 6);
            double f = hue / 60 - (int) (hue / 60);

            value *= 255;
            var v = (byte) value;
            var p = (byte) (value * (1 - saturation));
            var q = (byte) (value * (1 - f * saturation));
            var t = (byte) (value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return (v, t, p);
            else if (hi == 1)
                return (q, v, p);
            else if (hi == 2)
                return (p, v, t);
            else if (hi == 3)
                return (p, q, v);
            else if (hi == 4)
                return (t, p, v);
            else
                return (v, p, q);
        }
    }
}
