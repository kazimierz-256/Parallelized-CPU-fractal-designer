using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        IProgress<(BitmapSource, AlgorithmResult[,])> progress;
        Task t;

        private void AsyncDraw(FractalColourer colourer, Complex center, double radius, int width, int height)
        {
            if (tokens.Count > 0 && !tokens.Peek().IsCancellationRequested)
                tokens.Peek().Cancel();

            var currentToken = new CancellationTokenSource();

            tokens.Push(currentToken);

            progress = new Progress<(BitmapSource bitmap, AlgorithmResult[,] information)>(result =>
            {
                if (result.bitmap != null)
                {
                    Fractal.Source = result.bitmap;
                    Fractal.Tag = result.information;
                }
            });

            t = Task.Factory.StartNew(() =>
            {
                DoWork(currentToken.Token, colourer, center, radius, width, height);
            }, currentToken.Token);
        }

        private void DoWork(CancellationToken token, FractalColourer colourer, Complex center, double radius, int width, int height)
        {
            var divisors = new List<double> { 100, 20, 4, 2, 1, .5 };
            foreach (var divisor in divisors)
            {
                int newWindth = (int) (width / divisor);
                int newHeight = (int) (height / divisor);

                if (newWindth == 0 || newHeight == 0)
                    continue;

                var bs = colourer.GetBitmapSourceFromComplexGrid(center, radius, newWindth, newHeight, token);

                if (token.IsCancellationRequested)
                    return;

                progress.Report(bs);
            }
        }
    }
}
