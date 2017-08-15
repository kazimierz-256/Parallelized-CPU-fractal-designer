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

        private void AsyncDraw(AlgorithmProcessor algorithmProcessor, Complex center, double radius, int width, int height)
        {
            if (tokens.Count > 0 && !tokens.Peek().IsCancellationRequested)
            {
                tokens.Peek().Cancel();
                //tokens.Peek().Dispose();
                ++currentTaskID;
            }

            var token = new CancellationTokenSource();
            tokens.Push(token);

            Task.Factory.StartNew(() =>
            {
                var colorer = new AlgorithmProcessor.Colourful();

                algorithmProcessor.GetBitmapSourceFromComplexGrid(
                    colorer, center, radius, width * 4, height * 4, currentTaskID, Report, token.Token);

            }, token.Token).ContinueWith(new Action<Task>(t => t.Dispose()), token.Token);


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
