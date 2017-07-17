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
        public IFractalAlgorithm GetAlgorithmByName(string algorithmName, int MaximumIterationCount, params ComplexFunction[] Derivatives)
        {
            IFractalAlgorithm algorithm;

            switch (algorithmName.ToLower())
            {
                case "newton":
                    algorithm = new NewtonFractal();
                    break;
                case "muller":
                    algorithm = new MullerFractal();
                    break;
                case "kazimierz":
                    algorithm = new KazimierzFractal();
                    break;
                default:
                    return null;
            }

            algorithm.Derivatives = Derivatives;
            algorithm.MaximumIterationCount = MaximumIterationCount;

            return algorithm;
        }
    }

    public class MullerFractal : IFractalAlgorithm
    {
        double eps44 = Math.Pow(2, -44);
        double eps11 = Math.Pow(2, -11);
        double eps20 = Math.Pow(2, -20);

        public ComplexFunction[] Derivatives { get; set; }
        public int MaximumIterationCount { get; set; } = 30;

        public AlgorithmResult Compute(Complex z)
        {
            int iterationsLeft = MaximumIterationCount;
            Complex delta;
            Complex zz, zzz, fz, fzz, fzzz;
            Complex q, A, B, C;

            zzz = z;
            fzzz = Derivatives[0](z);

            zz = z * (1 - eps11);
            fzz = Derivatives[0](zz);

            zz = z - z * eps11 * fzz / (fzzz - fzz);
            fzz = Derivatives[0](zz);

            z = zz - zz * (zzz - zz) * fzz / (fzzz - fzz);
            fz = Derivatives[0](z);

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
                fz = Derivatives[0](z);

            } while (--iterationsLeft >= 0 && delta.Magnitude > Math.Max(eps44, z.Magnitude * eps20));

            return new AlgorithmResult() { z = z, iterations = MaximumIterationCount - iterationsLeft, succeeded = true };//(z, , true);
        }

    }

    public class NewtonFractal : IFractalAlgorithm
    {
        double eps44 = Math.Pow(2, -44);
        double eps20 = Math.Pow(2, -20);
        double eps10 = Math.Pow(2, -10);

        public ComplexFunction[] Derivatives { get; set; }
        public int MaximumIterationCount { get; set; }

        public AlgorithmResult Compute(Complex z)
        {
            int iterationsLeft = MaximumIterationCount;
            Complex delta;

            do
            {
                delta = Derivatives[0](z) / Derivatives[1](z);
                if (double.IsNaN(delta.Real) || double.IsNaN(delta.Imaginary))
                    return new AlgorithmResult() { z = z, iterations = MaximumIterationCount - iterationsLeft, succeeded = false };

                z -= delta;
            } while (--iterationsLeft >= 0 && delta.Magnitude > Math.Max(eps44, z.Magnitude * eps20));

            return new AlgorithmResult() { z = z, iterations = MaximumIterationCount - iterationsLeft, succeeded = true };
        }

    }

    public class KazimierzFractal : IFractalAlgorithm
    {
        double eps44 = Math.Pow(2, -44);
        double eps11 = Math.Pow(2, -11);
        double eps20 = Math.Pow(2, -20);

        public ComplexFunction[] Derivatives { get; set; }
        public int MaximumIterationCount { get; set; }
        public double Parameter { get; set; } = 1;

        public AlgorithmResult Compute(Complex z)
        {
            int iterationsLeft = MaximumIterationCount;
            Complex delta;
            Complex zz, zzz, fz, fzz, fzzz;
            zzz = z;
            fzzz = Derivatives[0](z);

            zz = z * (1 - eps11);
            fzz = Derivatives[0](zz);

            z = z - z * eps11 * fzz / (fzzz - fzz);
            fz = Derivatives[0](z);

            iterationsLeft -= 2;

            if (double.IsNaN(z.Real) || double.IsNaN(z.Imaginary))
                return new AlgorithmResult() { z = z, iterations = MaximumIterationCount - iterationsLeft, succeeded = false };

            do
            {
                delta = fz / (Parameter * ((fzz - fz) * (zzz - z) * (zzz - z) - (fzzz - fz) * (zz - z) * (zz - z)) / ((zz - z) * (zzz - z) * (zzz - zz)) + (1 - Parameter) * (fzz - fz) / (zz - z));

                if (double.IsNaN(delta.Real) || double.IsNaN(delta.Imaginary))
                    return new AlgorithmResult() { z = z, iterations = MaximumIterationCount - iterationsLeft, succeeded = false };

                zzz = zz;
                fzzz = fzz;
                zz = z;
                fzz = fz;
                z -= delta;
                fz = Derivatives[0](z);

            } while (--iterationsLeft >= 0 && delta.Magnitude > Math.Max(eps44, z.Magnitude * eps20));

            return new AlgorithmResult() { z = z, iterations = MaximumIterationCount - iterationsLeft, succeeded = true };
        }
    }

}
