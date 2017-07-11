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
    class Algorithms
    {
        public interface IFractalAlgorithm
        {
            int maximumIterationCount { get; set; }
            (Complex, int) Compute(Complex z);
        }

        public class NewtonFractal : IFractalAlgorithm
        {
            public int maximumIterationCount { get; set; } = 50;
            static double eps44 = Math.Pow(2, -44);
            static double eps10 = Math.Pow(2, -10);

            public (Complex, int) Compute(Complex z)
            {
                int iterationsLeft = maximumIterationCount;
                Complex delta;

                do
                {
                    delta = (z * z * z - 1) / (3 * z * z);
                    z = z - delta;
                } while (--iterationsLeft >= 0 && !double.IsNaN(z.Real) && !double.IsNaN(z.Imaginary) && Complex.Abs(delta) > Math.Max(eps44, Complex.Abs(z) * eps10));
                return (z, maximumIterationCount - iterationsLeft);
            }
        }

        public class IteratorSolver
        {
            IFractalAlgorithm fractalAlgorithm;

            public BitmapSource CreateBitmapSource(Complex center, double radiusReal, double radiusImaginary, int lengthReal, int lengthImaginary)
            {
                PixelFormat pf = PixelFormats.Bgr32;
                int rawStride = (lengthReal * pf.BitsPerPixel + 7) / 8;
                var rawImage = new byte[rawStride * lengthImaginary];

                Parallel.For(0, lengthReal, re =>
                {
                    for (int im = 0; im < lengthImaginary; ++im)
                    {
                        double realPosition = center.Real + (re * 2 - lengthReal) * radiusReal / lengthReal;
                        double imaginaryPosition = center.Imaginary + (im * 2 - lengthImaginary) * radiusImaginary / lengthImaginary;
                        (Complex z, int iterations) = fractalAlgorithm.Compute(new Complex(realPosition, imaginaryPosition));

                        (byte R, byte G, byte B) color;

                        if (double.IsNaN(z.Real) || double.IsNaN(z.Imaginary) || double.IsInfinity(z.Real) || double.IsInfinity(z.Imaginary))
                        {
                            color = (0, 0, 0);
                        }
                        else
                        {
                            double abs = Complex.Abs(z);
                            double hue = 180 * (z.Phase + Math.PI) / Math.PI;//(360/2) * ...
                            double saturation = 500d / (500d + iterations * iterations) * (1 - Math.Pow(2, -10) / (Math.Pow(2, -10) + abs * abs));
                            double value = 300d / (300d + iterations * iterations);
                            color = ColorFromHSV(hue, saturation, value);
                        }

                        //*(p1 + (4 * (lengthReal * im + re) + 0)) = color.B;
                        //*(p1 + (4 * (lengthReal * im + re) + 1)) = color.G;
                        //*(p1 + (4 * (lengthReal * im + re) + 2)) = color.R;
                        rawImage[4 * (lengthReal * im + re)] = color.B;
                        rawImage[4 * (lengthReal * im + re) + 1] = color.G;
                        rawImage[4 * (lengthReal * im + re) + 2] = color.R;
                    }
                });

                return BitmapSource.Create(lengthReal, lengthImaginary, 96, 96, pf, null, rawImage, rawStride);
            }

            //public Complex[,] Solve(Complex center, double radiusReal, double radiusImaginary, int lengthReal, int lengthImaginary)
            //{
            //    var grid = new Complex[lengthReal, lengthImaginary];
            //    double realPosition, imaginaryPosition;

            //    Parallel.For(0, lengthReal, re =>
            //    {
            //        for (int im = 0; im < lengthImaginary; im++)
            //        {
            //            realPosition = center.Real + (re * 2 - lengthReal) * radiusReal / lengthReal;
            //            imaginaryPosition = center.Imaginary + (im * 2 - lengthImaginary) * radiusImaginary / lengthImaginary;
            //            grid[re, im] = new Complex(realPosition, imaginaryPosition);
            //        }
            //    });

            //    return Solve(grid);
            //}

            //public Complex[,] Solve(Complex[,] grid)
            //{
            //    var outGrid = new Complex[grid.GetLength(0), grid.GetLength(1)];

            //    Parallel.For(0, grid.GetLength(0), re =>
            //    {
            //        for (int im = 0; im < grid.GetLength(1); im++)
            //            outGrid[re, im] = function(grid[re, im]);

            //    });

            //    return outGrid;
            //}

            public IteratorSolver(IFractalAlgorithm fractalAlgorithm)
            {
                this.fractalAlgorithm = fractalAlgorithm;
            }

        }

        //public static BitmapSource ComplexToImage(Complex[,] grid)
        //{
        //    PixelFormat pf = PixelFormats.Bgr32;
        //    int rawStride = (grid.GetLength(0) * pf.BitsPerPixel + 7) / 8;
        //    var rawImage = new byte[rawStride * grid.GetLength(1)];

        //    System.Drawing.Color color;

        //    Parallel.For(0, grid.GetLength(0), re =>
        //    {
        //        for (int im = 0; im < grid.GetLength(1); ++im)
        //        {
        //            if (double.IsNaN(grid[re, im].Real) || double.IsNaN(grid[re, im].Imaginary) || double.IsInfinity(grid[re, im].Real) || double.IsInfinity(grid[re, im].Imaginary))
        //            {
        //                color = System.Drawing.Color.DarkOrchid;
        //            }
        //            else
        //            {
        //                double hue = 180 * (grid[re, im].Phase + Math.PI) / Math.PI;//(360/2) * ...
        //                double saturation = 500d / (500d + 0) * (1 - Math.Pow(2, -10) / (Math.Pow(2, -10) + Complex.Abs(grid[re, im]) * Complex.Abs(grid[re, im])));
        //                double value = 300d / (300d + 0);
        //                color = ColorFromHSV(hue, saturation, value);
        //            }

        //            rawImage[4 * (grid.GetLength(0) * im + re) + 0] = color.B;
        //            rawImage[4 * (grid.GetLength(0) * im + re) + 1] = color.G;
        //            rawImage[4 * (grid.GetLength(0) * im + re) + 2] = color.R;

        //        }
        //    });

        //    return BitmapSource.Create(grid.GetLength(0), grid.GetLength(1), 96, 96, pf, null, rawImage, rawStride);
        //}

        public static (byte R, byte G, byte B) ColorFromHSV(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value *= 255;
            byte v = (byte) value;
            byte p = (byte) (value * (1 - saturation));
            byte q = (byte) (value * (1 - f * saturation));
            byte t = (byte) (value * (1 - (1 - f) * saturation));

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
