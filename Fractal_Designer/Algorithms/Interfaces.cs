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
    public interface IFractalAlgorithm
    {
        AlgorithmResult Compute(Complex z);
        int MaximumIterationCount { get; set; }
        ComplexFunction.ComplexFunction[] Derivatives { get; set; }
    }

    public interface IParametrizedAlgorithm
    {
        double Parameter { get; set; }
    }

    public interface IFractalFactory
    {
        IFractalAlgorithm GetAutoConfiguredAlgorithmByID(Algorithm algorithmID, params ComplexFunction.ComplexFunction[] Derivatives);
    }

    public interface IAlgorithmProcessor
    {
        IFractalAlgorithm FractalAlgorithm { get; set; }
        BitmapSourceResult GetBitmapSourceFromComplexGrid(Complex center, double radius, int lengthReal, int lengthImaginary, CancellationToken token, bool parallel);
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
