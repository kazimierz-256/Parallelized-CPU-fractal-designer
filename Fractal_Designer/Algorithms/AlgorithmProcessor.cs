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
        public FractalAlgorithm FractalAlgorithm { get; set; }

        public AlgorithmProcessor(FractalAlgorithm fractalAlgorithm) =>
            FractalAlgorithm = fractalAlgorithm ?? throw new ArgumentNullException("Brak algorytmu.");

        public void GetBitmapSourceFromComplexGrid(Complex center, double radius, int width, int height, ulong taskID, Action<BitmapSourceResult> performUpdate, CancellationToken token = new CancellationToken())
        {
            Colorer colorer = GetColorer(Settings.Instance.colorer);
            PixelFormat pixelFormat = PixelFormats.Bgr32;
            var results = new AlgorithmResult[width, height];

            double radiusReal = radius;
            double radiusImaginary = radius * height / width;

            // best-fit window, if in portrait mode
            if (width < height)
            {
                radiusReal = radius * width / height;
                radiusImaginary = radius;
            }

            const int parallelThreshold = 64;

            for (int delta = 2048; delta >= 1; delta >>= 1)
            {
                var localWidth = width / delta;
                var localHeight = height / delta;

                if (localWidth == 0 || localHeight == 0)
                    continue;

                var bytesPerPixel = (pixelFormat.BitsPerPixel + 7) / 8;
                var stride = localWidth * bytesPerPixel;
                var fractal = new byte[stride * localHeight];

                if (delta <= parallelThreshold)
                {
                    //parallel
                    //minus one because of the UI thread
                    int completedRows = 0;

                    ShowProgress(0, 0);

                    if (delta == parallelThreshold)
                        System.Windows.Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            if (System.Windows.Application.Current.MainWindow.TaskbarItemInfo != null)
                                System.Windows.Application.Current.MainWindow.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Normal;
                        }));

                    Parallel.For(0, localWidth, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount - 1 }, (re, loopstate) =>
                        {
                            if (token.IsCancellationRequested)
                                loopstate.Break();
                            else
                            {
                                re = re * delta;
                                for (int im = 0; im < delta * localHeight; im += delta)
                                {
                                    if (!results[re, im].computed)
                                    {
                                        // compute the location like in a grid (could be image-based but who wants it?)
                                        results[re, im] = FractalAlgorithm.Compute(new Complex(
                                            real: center.Real + ((re + .5) * 2d - width) / width * radiusReal,
                                            imaginary: center.Imaginary - ((im + .5) * 2d - height) / height * radiusImaginary
                                        ));

                                        results[re, im].computed = true;

                                        if (results[re, im].succeeded)
                                            results[re, im].color = colorer.Color(results[re, im]);

                                    }

                                    var position = bytesPerPixel * (localWidth * (im / delta) + re / delta);

                                    fractal[position] = results[re, im].color.B;
                                    fractal[position + 1] = results[re, im].color.G;
                                    fractal[position + 2] = results[re, im].color.R;
                                    //fractal[position + 3] = results[re, im].color.A;
                                }

                                Interlocked.Increment(ref completedRows);
                                ShowProgress(completedRows, 1.0 * completedRows / localWidth);
                            }
                        });

                    ShowProgress(0, 1);

                    if (delta == 1)
                        System.Windows.Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            if (System.Windows.Application.Current?.MainWindow?.TaskbarItemInfo != null)
                                System.Windows.Application.Current.MainWindow.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                        }));
                }
                else
                {
                    for (int re = delta / 2; re < delta * localWidth; re += delta)
                    {
                        if (token.IsCancellationRequested)
                            return;

                        for (int im = delta / 2; im < delta * localHeight; im += delta)
                        {
                            if (!results[re, im].computed)
                            {
                                // compute the location like in a grid (could be image-based but who wants it?)
                                results[re, im] = FractalAlgorithm.Compute(new Complex(
                                    real: center.Real + ((re + .5) * 2d - width) / width * radiusReal,
                                    imaginary: center.Imaginary - ((im + .5) * 2d - height) / height * radiusImaginary
                                ));

                                results[re, im].computed = true;

                                if (results[re, im].succeeded)
                                    results[re, im].color = colorer.Color(results[re, im]);
                            }

                            var position = bytesPerPixel * (localWidth * (im / delta) + re / delta);

                            fractal[position] = results[re, im].color.B;
                            fractal[position + 1] = results[re, im].color.G;
                            fractal[position + 2] = results[re, im].color.R;
                            //fractal[position + 3] = results[re, im].color.A;
                        }

                    }
                }

                if (token.IsCancellationRequested)
                    return;

                // problematic black stripe
                var bitmap = BitmapSource.Create(localWidth, localHeight, 96, 96, pixelFormat, null, fractal, stride);
                bitmap.Freeze();
                performUpdate(new BitmapSourceResult
                {
                    bitmap = bitmap,
                    results = results,
                    timesSmaller = delta,
                    taskID = taskID,
                });
            }

        }

        private Colorer GetColorer(ushort colorer)
        {
            switch ((Colorer) colorer)
            {
                case Colorer.Root_phase:
                    return new RootPhaseColorer();
                case Colorer.Iterations:
                    return new IterationsColorer();
                default:
                    return null;
            }
        }

        void ShowProgress(int completedRows, double ratio)
        {
            System.Windows.Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
            {
                var bar = ((System.Windows.Controls.ProgressBar)System.Windows.Application.Current?.MainWindow?.FindName("Progress"));
                if (bar == null)
                    return;

                bar.Value = bar.Maximum * ratio;

                if (System.Windows.Application.Current.MainWindow.TaskbarItemInfo != null)
                    System.Windows.Application.Current.MainWindow.TaskbarItemInfo.ProgressValue = ratio;
            }));
        }


        public class RootPhaseColorer : Colorer
        {
            static double eps10 = Math.Pow(2, -10);
            public override (byte R, byte G, byte B) Color(AlgorithmResult result)
            {
                double abs = result.z.Magnitude;
                double hue = 180d * (result.z.Phase + Math.PI) / Math.PI;//(360/2) * ..., don't worry, overflow is allowed
                int iterationsSquared = result.iterations * result.iterations;
                double saturation = 700d / (800d + Math.Log(result.iterations)) * (1 - eps10 / (eps10 + abs * abs));
                double value = 10d / (10d + result.iterations);

                return ColorFromHSV(hue, saturation, value);
            }
        }

        public class IterationsColorer : Colorer
        {
            public override (byte R, byte G, byte B) Color(AlgorithmResult result)
            {
                double iter = Math.Pow(result.iterations, 1.4);
                return ((byte)(255d * 10d / (10d + iter)), (byte)(255d * 70d / (70d + iter)), (byte)(255d * 300d / (300d + iter)));
            }
        }

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
