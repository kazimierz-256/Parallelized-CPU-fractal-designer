using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Fractal_Designer
{
    public interface IFractalAlgorithm
    {
        AlgorithmResult Compute(Complex z);
        int MaximumIterationCount { get; set; }
        ComplexFunction[] Derivatives { get; set; }
    }


    public interface IFractalFactory
    {
        IFractalAlgorithm GetAlgorithmByName(string algorithmName,int MaximumIterationCount, params ComplexFunction[] Derivatives);
    }

    public delegate Complex ComplexFunction(Complex z);
}
