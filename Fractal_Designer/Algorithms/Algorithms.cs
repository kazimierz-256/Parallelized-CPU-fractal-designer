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
    public struct AlgorithmResult
    {
        public Complex z;
        public int iterations;
        public bool succeeded;
    }

    public class FractalColourer
    {
        IFractalAlgorithm fractalAlgorithm;

        public AlgorithmResult[,] results;

        public BitmapSource CreateBitmapSource(Complex center, double radius, int lengthReal, int lengthImaginary)
        {
            double eps10 = Math.Pow(2, -10);

            PixelFormat pixelFormat = PixelFormats.Bgr32;
            int stride = (lengthReal * pixelFormat.BitsPerPixel + 7) / 8;
            var fractal = new byte[stride * lengthImaginary];
            results = new AlgorithmResult[lengthReal, lengthImaginary];

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
                        results[re, im] = fractalAlgorithm.Compute(new Complex(realPosition, imaginaryPosition));

                    if (results[re, im].succeeded)
                    {
                        double abs = results[re, im].z.Magnitude;
                        double hue = 180d * (results[re, im].z.Phase + Math.PI) / Math.PI;//(360/2) * ..., don't worry, overflow is allowed
                            int iterationsSquared = results[re, im].iterations * results[re, im].iterations;
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

        public FractalColourer(IFractalAlgorithm fractalAlgorithm) => this.fractalAlgorithm = fractalAlgorithm ?? throw new ArgumentNullException("Brak algorytmu.");

        private static (byte R, byte G, byte B) ColorFromHSV(double hue, double saturation, double value)
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
