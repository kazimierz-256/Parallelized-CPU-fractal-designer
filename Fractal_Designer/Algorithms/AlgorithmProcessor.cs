using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Fractal_Designer
{
    public class AlgorithmProcessor
    {
        public IFractalAlgorithm FractalAlgorithm { get; set; }

        public AlgorithmProcessor(IFractalAlgorithm fractalAlgorithm) =>
            FractalAlgorithm = fractalAlgorithm ?? throw new ArgumentNullException("Brak algorytmu.");

        public BitmapSourceResult GetBitmapSourceFromComplexGrid(IColorer colorer, Complex center, double radius, int lengthReal, int lengthImaginary, CancellationToken token = new CancellationToken(), bool parallel = false)
        {
            if (lengthReal == 0 || lengthImaginary == 0)
                return new BitmapSourceResult { bitmap = null, results = null };

            double eps10 = Math.Pow(2, -10);

            PixelFormat pixelFormat = PixelFormats.Bgr32;
            int stride = (lengthReal * pixelFormat.BitsPerPixel + 7) / 8;
            var fractal = new byte[stride * lengthImaginary];
            var results = new AlgorithmResult[lengthReal, lengthImaginary];

            double radiusReal = radius;
            double radiusImaginary = radius * lengthImaginary / lengthReal;

            // best-fit window, if in portrait mode
            if (lengthReal < lengthImaginary)
            {
                radiusReal = radius * lengthReal / lengthImaginary;
                radiusImaginary = radius;
            }

            if (parallel)
            {
                // minus one because of the UI thread
                int completedRows = 0;

                ShowProgress(0);

                System.Windows.Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (System.Windows.Application.Current.MainWindow.TaskbarItemInfo != null)
                        System.Windows.Application.Current.MainWindow.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Normal;
                }));

                Parallel.For(0, lengthReal, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount - 1 }, (re, loopstate) =>
                    {
                        if (token.IsCancellationRequested)
                            loopstate.Break();
                        else
                        {
                            ComputeRow(re);
                            Interlocked.Increment(ref completedRows);
                            ShowProgress(completedRows);
                        }
                    });

                ShowProgress(0);

                System.Windows.Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (System.Windows.Application.Current?.MainWindow?.TaskbarItemInfo != null)
                        System.Windows.Application.Current.MainWindow.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                }));

                if (token.IsCancellationRequested)
                    return new BitmapSourceResult { bitmap = null, results = null };
            }
            else
            {
                for (int re = 0; re < lengthReal; ++re)
                {
                    if (token.IsCancellationRequested)
                        return new BitmapSourceResult { bitmap = null, results = null };

                    ComputeRow(re);
                }
            }

            var bitmap = BitmapSource.Create(lengthReal, lengthImaginary, 96, 96, pixelFormat, null, fractal, stride);
            bitmap.Freeze();
            return new BitmapSourceResult { bitmap = bitmap, results = results };

            void ComputeRow(int re)
            {
                for (int im = 0; im < lengthImaginary; ++im)
                {
                    // compute the location like in a grid (could be image-based but who wants it?)
                    double realPosition = center.Real + ((re + .5) * 2d - lengthReal) / lengthReal * radiusReal;
                    double imaginaryPosition = center.Imaginary + ((im + .5) * 2d - lengthImaginary) / lengthImaginary * radiusImaginary;

                    // compute the end result
                    results[re, im] = FractalAlgorithm.Compute(new Complex(realPosition, imaginaryPosition));

                    if (results[re, im].succeeded)
                    {
                        (byte R, byte G, byte B) color = colorer.Colour(results[re, im]);

                        fractal[4 * (lengthReal * im + re)] = color.B;
                        fractal[4 * (lengthReal * im + re) + 1] = color.G;
                        fractal[4 * (lengthReal * im + re) + 2] = color.R;
                    }
                    // else zeroes

                }
            }

            void ShowProgress(int completedRows)
            {
                System.Windows.Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
                {
                    var bar = ((System.Windows.Controls.ProgressBar)System.Windows.Application.Current?.MainWindow?.FindName("Progress"));
                    if (bar == null)
                        return;

                    bar.Value = bar.Maximum * completedRows / lengthReal;

                    if (System.Windows.Application.Current.MainWindow.TaskbarItemInfo != null)
                        System.Windows.Application.Current.MainWindow.TaskbarItemInfo.ProgressValue = 1d * completedRows / lengthReal;
                }));
            }
        }

        public class Colourful : IColorer
        {
            static double eps10 = Math.Pow(2, -10);
            public (byte R, byte G, byte B) Colour(AlgorithmResult result)
            {
                double abs = result.z.Magnitude;
                double hue = 180d * (result.z.Phase + Math.PI) / Math.PI;//(360/2) * ..., don't worry, overflow is allowed
                int iterationsSquared = result.iterations * result.iterations;
                double saturation = 700d / (800d + Math.Log(result.iterations)) * (1 - eps10 / (eps10 + abs * abs));
                double value = 180d / (200d + iterationsSquared);

                return ColorFromHSV(hue, saturation, value);
            }
        }

        // TODO
        //public class Bluish : IColorer
        //{
        //    static double eps10 = Math.Pow(2, -10);
        //    public (byte R, byte G, byte B) Colour(AlgorithmResult result)
        //    {
        //        double abs = result.z.Magnitude;
        //        double hue = 180d * (result.z.Phase + Math.PI) / Math.PI;//(360/2) * ..., don't worry, overflow is allowed
        //        int iterationsSquared = result.iterations * result.iterations;
        //        double saturation = 700d / (800d + Math.Log(result.iterations)) * (1 - eps10 / (eps10 + abs * abs));
        //        double value = 180d / (200d + iterationsSquared);

        //        return ColorFromHSV(hue, saturation, value);
        //    }
        //}

        private static (byte R, byte G, byte B) ColorFromHSV(double hue, double saturation, double value)
        {
            var hi = (byte)((int)(hue / 60) % 6);
            double f = hue / 60 - (int)(hue / 60);

            value *= 255;
            var v = (byte)value;
            var p = (byte)(value * (1 - saturation));
            var q = (byte)(value * (1 - f * saturation));
            var t = (byte)(value * (1 - (1 - f) * saturation));

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
