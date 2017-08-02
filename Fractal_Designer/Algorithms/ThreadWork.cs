using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Fractal_Designer
{
    partial class MainWindow
    {
        Stack<CancellationTokenSource> tokens = new Stack<CancellationTokenSource>();
        ulong currentTaskID = 0;

        private void AsyncDraw(AlgorithmProcessor colourer, Complex center, double radius, int width, int height)
        {
            if (tokens.Count > 0 && !tokens.Peek().IsCancellationRequested)
            {
                tokens.Peek().Cancel();
                tokens.Peek().Dispose();
                ++currentTaskID;
            }

            var token = new CancellationTokenSource();
            tokens.Push(token);

            Task.Factory.StartNew(() =>
            {
                const double parallelThreshold = 9;

                foreach (var divisor in new double[] { 243, 81, 27, 9, 3, 1, .5 })
                    GetBitmapAndReport(divisor, divisor <= parallelThreshold);

            }, token.Token).ContinueWith(new Action<Task>(t => t.Dispose()), token.Token);

            void GetBitmapAndReport(double divisor, bool parallel)
            {
                if (token.IsCancellationRequested)
                    return;

                var result = colourer.GetBitmapSourceFromComplexGrid(center, radius, (int)(width / divisor), (int)(height / divisor), token.Token, parallel);

                if (token.IsCancellationRequested)
                    return;

                if (result.bitmap != null)
                {
                    result.taskID = currentTaskID;
                    result.timesSmaller = divisor;

                    Report(result);
                }
            }// end GetBitmapAndReport

            void Report(BitmapSourceResult result)
            {
                Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (!token.IsCancellationRequested && (Fractal.Tag == null || result.taskID != ((BitmapSourceResult)Fractal.Tag).taskID || result.timesSmaller < ((BitmapSourceResult)Fractal.Tag).timesSmaller))
                    {
                        lock (Fractal)
                        {
                            Fractal.Source = result.bitmap;
                            Fractal.Tag = result;

                            Status.Text = $"#{result.timesSmaller} times smaller ({result.taskID})";
                        }
                    }

                }));
            }// end Report
        }// end AsyncDraw

        //private TimeSpan Measure(Action a)
        //{
        //    var sw = new Stopwatch();
        //    sw.Start();
        //    a();
        //    sw.Stop();
        //    Title = $"Computed in {sw.Elapsed.Milliseconds}ms ({sw.Elapsed.Ticks} ts)";
        //    return sw.Elapsed;
        //}
    }
}
