using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Fractal_Designer
{
    public abstract class FractalAlgorithm
    {
        public abstract AlgorithmResult Compute(Complex z);
        public int MaximumIterationCount { get; set; }
        public ComplexFunction.ComplexFunction[] Derivatives { get; set; }
    }

    public abstract class ParametrizedFractalAlgorithm : FractalAlgorithm
    {
        public double Parameter { get; set; }
    }

    public interface IFractalFactory
    {
        FractalAlgorithm GetAutoConfiguredAlgorithmByID(Algorithm algorithmID, params ComplexFunction.ComplexFunction[] Derivatives);
    }

    public interface IColorer
    {
        (byte R, byte G, byte B) Colour(AlgorithmResult result);
    }

    public struct BitmapSourceResult
    {
        public BitmapSource bitmap;
        public AlgorithmResult[,] results;
        public ulong taskID;
        public double timesSmaller;
    }

    public struct AlgorithmResult
    {
        public Complex z;
        public int iterations;
        public bool succeeded;
        public double orderOfConvergence;
    }
}
