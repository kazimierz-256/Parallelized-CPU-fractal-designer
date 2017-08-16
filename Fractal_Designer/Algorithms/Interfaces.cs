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
        public double eps
        {
            get => Math.Pow(2, (double)Settings.Instance.eps); private set { }
        }
        public double epseps
        {
            get => Math.Pow(2, (double)Settings.Instance.epseps); private set { }
        }
        public double deltaeps
        {
            get => Math.Pow(2, (double)Settings.Instance.delta); private set { }
        }

        //public double eps = Math.Pow(2, -11);
        //public double epseps = Math.Pow(2, -16);
        public abstract AlgorithmResult Compute(Complex z);
        public int MaximumIterationCount { get; set; }
        public ComplexFunction.ComplexFunction[] Derivatives { get; set; }
        protected bool ReachedConvergence(Complex delta, Complex z, Complex fz) =>
            delta.Magnitude < Math.Max(deltaeps, z.Magnitude * epseps) && ReachedZero(fz);
        protected bool ReachedZero(Complex fz) => fz.Magnitude < epseps;
    }

    public abstract class ParametrizedFractalAlgorithm : FractalAlgorithm
    {
        public double Parameter { get; set; }
    }

    public interface IFractalFactory
    {
        FractalAlgorithm GetAutoConfiguredAlgorithmByID(Algorithm algorithmID, params ComplexFunction.ComplexFunction[] Derivatives);
    }

    struct ArchivedSolution
    {
        Complex z;
        AlgorithmResult result;
    }

    public abstract class Colorer
    {
        public abstract (byte R, byte G, byte B) Color(AlgorithmResult result);
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
        public bool computed;
        public double orderOfConvergence;
        public (byte R, byte G, byte B) color;
    }
}
