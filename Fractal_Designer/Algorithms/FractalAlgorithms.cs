using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Fractal_Designer
{
    class ComplexFractalFactory : IFractalFactory
    {
        public IFractalAlgorithm GetAutoConfiguredAlgorithmByID(Algorithm algorithmID, params IComplexFunction[] Derivatives)
        {
            IFractalAlgorithm algorithm;

            switch (algorithmID)
            {
                // actually we really need the derivative so really (... >= 2)
                case Algorithm.Newton when Derivatives.Length >= 1:
                    algorithm = new NewtonFractal();
                    break;
                case Algorithm.Kazimierz when Derivatives.Length >= 1:
                    algorithm = new KazimierzFractal();
                    break;
                case Algorithm.Muller when Derivatives.Length >= 1:
                    algorithm = new MullerFractal();
                    break;
                default:
                    algorithm = new NullAlgorithm();
                    break;
            }

            algorithm.Derivatives = Derivatives;
            algorithm.MaximumIterationCount = Settings.Instance.iterations;

            if (algorithm is IParametrizedAlgorithm)
                (algorithm as IParametrizedAlgorithm).Parameter = (double)Settings.Instance.parameter;

            return algorithm;
        }
    }

    internal class NullAlgorithm : IFractalAlgorithm
    {
        public int MaximumIterationCount { get; set; }
        public IComplexFunction[] Derivatives { get; set; }

        public AlgorithmResult Compute(Complex z) => new AlgorithmResult() { succeeded = false };
    }

    public class MullerFractal : IFractalAlgorithm
    {
        double eps44 = Math.Pow(2, -44);
        double eps11 = Math.Pow(2, -11);
        double eps20 = Math.Pow(2, -20);

        public IComplexFunction[] Derivatives { get; set; }
        public int MaximumIterationCount { get; set; } = 30;

        public AlgorithmResult Compute(Complex z)
        {
            int iterationsLeft = MaximumIterationCount;
            Complex delta;
            Complex zz, zzz, fz, fzz, fzzz;
            Complex q, A, B, C;

            zzz = z;
            fzzz = Derivatives[0].Compute(z);

            zz = z * (1 - eps11);
            fzz = Derivatives[0].Compute(zz);

            zz = zzz - zzz * (zzz - zz) * fzz / (fzzz - fzz);
            fzz = Derivatives[0].Compute(zz);

            z = zz - zz * (zzz - zz) * fzz / (fzzz - fzz);
            fz = Derivatives[0].Compute(z);

            iterationsLeft -= 2;

            if (double.IsNaN(z.Real) || double.IsNaN(z.Imaginary))
                new AlgorithmResult() { z = z, iterations = MaximumIterationCount - iterationsLeft, succeeded = false };

            do
            {
                q = (z - zz) / (zz - zzz);
                A = q * fz - q * (1 + q) * fzz + q * q * fzzz;
                B = (2 * q + 1) * fz - (1 + q) * (1 + q) * fzz + q * q * fzzz;
                C = (1 + q) * fz;
                Complex sqrt = Complex.Sqrt(B * B - 4 * A * C);
                Complex best = B + sqrt;
                Complex secondary = B - sqrt;

                if (best.Magnitude < secondary.Magnitude)
                    best = secondary;

                delta = (z - zz) * 2 * C / best;

                if (double.IsNaN(delta.Real) || double.IsNaN(delta.Imaginary))
                    return new AlgorithmResult() { z = z, iterations = MaximumIterationCount - iterationsLeft, succeeded = false };

                zzz = zz;
                fzzz = fzz;
                zz = z;
                fzz = fz;
                z -= delta;
                fz = Derivatives[0].Compute(z);

            } while (--iterationsLeft >= 0 && delta.Magnitude > Math.Max(eps44, z.Magnitude * eps20));

            return new AlgorithmResult() { z = z, iterations = MaximumIterationCount - iterationsLeft, succeeded = fz.Magnitude < eps11 && iterationsLeft >= 0 };//(z, , true);
        }

    }

    public class NewtonFractal : IFractalAlgorithm
    {
        // REQUIRES knowing the DERIVATIVE

        //double eps44 = Math.Pow(2, -44);
        //double eps20 = Math.Pow(2, -20);
        //double eps11 = Math.Pow(2, -11);

        //public IComplexFunction[] Derivatives { get; set; }
        //public int MaximumIterationCount { get; set; }

        //public AlgorithmResult Compute(Complex z)
        //{
        //    int iterationsLeft = MaximumIterationCount;
        //    Complex delta, fz;

        //    do
        //    {
        //        fz = Derivatives[0].Compute(z);
        //        delta = fz / Derivatives[1].Compute(z);
        //        if (double.IsNaN(delta.Real) || double.IsNaN(delta.Imaginary))
        //            return new AlgorithmResult() { z = z, iterations = MaximumIterationCount - iterationsLeft, succeeded = false };

        //        z -= delta;
        //    } while (--iterationsLeft >= 0 && delta.Magnitude > Math.Max(eps44, z.Magnitude * eps20));

        //    return new AlgorithmResult() { z = z, iterations = MaximumIterationCount - iterationsLeft, succeeded = fz.Magnitude < eps11 && iterationsLeft >= 0 };
        //}

        double eps44 = Math.Pow(2, -44);
        double eps11 = Math.Pow(2, -11);
        double eps20 = Math.Pow(2, -20);

        public IComplexFunction[] Derivatives { get; set; }
        public int MaximumIterationCount { get; set; }

        public AlgorithmResult Compute(Complex z)
        {
            int iterationsLeft = MaximumIterationCount;
            Complex delta;
            Complex zz, zzz, fz, fzz, fzzz;
            zzz = z;
            fzzz = Derivatives[0].Compute(z);

            zz = z * (1 - eps11);
            fzz = Derivatives[0].Compute(zz);

            z = zzz - (zzz - zz) * fzz / (fzzz - fzz);
            fz = Derivatives[0].Compute(z);

            z = zz;
            fz = fzz;
            zz = zzz;
            fzz = fzzz;

            iterationsLeft -= 2;

            if (double.IsNaN(z.Real) || double.IsNaN(z.Imaginary))
                return new AlgorithmResult() { z = z, iterations = MaximumIterationCount - iterationsLeft, succeeded = false };

            do
            {
                // newton-alike
                delta = z * eps11 * fz / (fz - Derivatives[0].Compute(z * (1 - eps11)));

                if (double.IsNaN(delta.Real) || double.IsNaN(delta.Imaginary))
                    return new AlgorithmResult() { z = z, iterations = MaximumIterationCount - iterationsLeft, succeeded = false };

                zzz = zz;
                fzzz = fzz;
                zz = z;
                fzz = fz;
                z -= delta;
                fz = Derivatives[0].Compute(z);
                
            } while (--iterationsLeft >= 0 && delta.Magnitude > Math.Max(eps44, z.Magnitude * eps20));

            return new AlgorithmResult() { z = z, iterations = MaximumIterationCount - iterationsLeft, succeeded = fz.Magnitude < eps11 && iterationsLeft >= 0 };
        }
    }

    public class KazimierzFractal : IFractalAlgorithm, IParametrizedAlgorithm
    {
        double eps44 = Math.Pow(2, -44);
        double eps11 = Math.Pow(2, -11);
        double eps20 = Math.Pow(2, -20);

        public IComplexFunction[] Derivatives { get; set; }
        public int MaximumIterationCount { get; set; }
        public double Parameter { get; set; }

        public AlgorithmResult Compute(Complex z)
        {
            //var orderOfConvergence = new Stack<double>();

            int iterationsLeft = MaximumIterationCount;
            Complex delta;
            Complex zz, zzz, fz, fzz, fzzz;
            zzz = z;
            fzzz = Derivatives[0].Compute(z);

            zz = z * (1 - eps11);
            fzz = Derivatives[0].Compute(zz);

            z = zzz - (zzz - zz) * fzz / (fzzz - fzz);
            fz = Derivatives[0].Compute(z);

            iterationsLeft -= 2;

            if (double.IsNaN(z.Real) || double.IsNaN(z.Imaginary))
                return new AlgorithmResult()
                {
                    z = z,
                    iterations = MaximumIterationCount - iterationsLeft,
                    succeeded = false,
                };

            do
            {
                delta = fz / (Parameter * ((fzz - fz) * (zzz - z) * (zzz - z) - (fzzz - fz) * (zz - z) * (zz - z)) / ((zz - z) * (zzz - z) * (zzz - zz)) + (1 - Parameter) * (fzz - fz) / (zz - z));

                if (double.IsNaN(delta.Real) || double.IsNaN(delta.Imaginary))
                    return new AlgorithmResult()
                    {
                        z = z,
                        iterations = MaximumIterationCount - iterationsLeft,
                        succeeded = false,
                    };

                zzz = zz;
                fzzz = fzz;
                zz = z;
                fzz = fz;
                z -= delta;
                fz = Derivatives[0].Compute(z);

                //Complex ideal = -9;// new Complex(Math.Round(z.Real), Math.Round(z.Imaginary));
                //orderOfConvergence.Push((Complex.Log((ideal - z).Magnitude / (ideal - zz).Magnitude) / Complex.Log((ideal - zz).Magnitude / (ideal - zzz).Magnitude)).Magnitude);
            } while (--iterationsLeft >= 0 && delta.Magnitude > Math.Max(eps44, z.Magnitude * eps20));

            return new AlgorithmResult()
            {
                z = z,
                iterations = MaximumIterationCount - iterationsLeft,
                succeeded = fz.Magnitude < eps11 && iterationsLeft >= 0,
                //orderOfConvergence = orderOfConvergence.Peek(),
            };
        }
    }

}
