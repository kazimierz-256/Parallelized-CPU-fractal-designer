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
        public FractalAlgorithm GetAutoConfiguredAlgorithmByID(Algorithm algorithmID, params ComplexFunction.ComplexFunction[] Derivatives)
        {
            if (Derivatives.Length == 0)
                return new NullAlgorithm();

            FractalAlgorithm algorithm;

            switch (algorithmID)
            {
                // actually we really need the derivative so really (... >= 2)
                case Algorithm.Newton:
                    if (Derivatives.Length < 2)
                    {
                        Derivatives = new ComplexFunction.ComplexFunction[] { Derivatives[0], Derivatives[0].GetDerivative() };
                    }
                    algorithm = new NewtonFractal();
                    break;

                case Algorithm.Halley:
                    if (Derivatives.Length < 2)
                    {
                        Derivatives = new ComplexFunction.ComplexFunction[] { Derivatives[0], Derivatives[0].GetDerivative() };
                    }
                    if (Derivatives.Length < 3)
                    {
                        Derivatives = new ComplexFunction.ComplexFunction[] { Derivatives[0], Derivatives[1], Derivatives[1].GetDerivative() };
                    }
                    algorithm = new HalleyFractal();
                    break;

                case Algorithm.Quadruple:
                    if (Derivatives.Length < 2)
                    {
                        Derivatives = new ComplexFunction.ComplexFunction[] { Derivatives[0], Derivatives[0].GetDerivative() };
                    }
                    if (Derivatives.Length < 3)
                    {
                        Derivatives = new ComplexFunction.ComplexFunction[] { Derivatives[0], Derivatives[1], Derivatives[1].GetDerivative() };
                    }
                    if (Derivatives.Length < 4)
                    {
                        Derivatives = new ComplexFunction.ComplexFunction[] { Derivatives[0], Derivatives[1], Derivatives[2], Derivatives[2].GetDerivative() };
                    }
                    algorithm = new HalleyFractal();
                    break;

                case Algorithm.Halley_overnewtoned:
                    if (Derivatives.Length < 2)
                    {
                        Derivatives = new ComplexFunction.ComplexFunction[] { Derivatives[0], Derivatives[0].GetDerivative() };
                    }
                    if (Derivatives.Length < 3)
                    {
                        Derivatives = new ComplexFunction.ComplexFunction[] { Derivatives[0], Derivatives[1], Derivatives[1].GetDerivative() };
                    }
                    algorithm = new HalleyNewton();
                    break;

                case Algorithm.Halley_without_derivative:
                    algorithm = new HalleyWithoutDerivativeFractal();
                    break;

                case Algorithm.Quadratic:
                    if (Derivatives.Length < 2)
                    {
                        Derivatives = new ComplexFunction.ComplexFunction[] { Derivatives[0], Derivatives[0].GetDerivative() };
                    }
                    if (Derivatives.Length < 3)
                    {
                        Derivatives = new ComplexFunction.ComplexFunction[] { Derivatives[0], Derivatives[1], Derivatives[1].GetDerivative() };
                    }
                    algorithm = new QuadraticFractal();
                    break;

                case Algorithm.Quadratic_without_derivative:
                    algorithm = new QuadraticWithoutDerivativeFractal();
                    break;

                case Algorithm.Newton_without_derivative:
                    algorithm = new NewtonWithoutDerivativeFractal();
                    break;

                case Algorithm.Secant_Newton_combination:
                    algorithm = new SecantNewtonFractal();
                    break;

                case Algorithm.Secant:
                    algorithm = new SecantFractal();
                    break;

                case Algorithm.Muller:
                    algorithm = new MullerFractal();
                    break;

                case Algorithm.Moler_real:
                    algorithm = new MolerRealFractal();
                    break;

                case Algorithm.Inverse:
                    algorithm = new InverseQuadraticFractal();
                    break;

                case Algorithm.Steffensen:
                    algorithm = new SteffensenFractal();
                    break;

                case Algorithm.Custom:
                    algorithm = new CustomFractal();
                    break;

                default:
                    algorithm = new NullAlgorithm();
                    break;
            }

            algorithm.Derivatives = Derivatives;
            algorithm.MaximumIterationCount = Settings.Instance.iterations;

            if (algorithm is ParametrizedFractalAlgorithm)
                (algorithm as ParametrizedFractalAlgorithm).Parameter = (double)Settings.Instance.parameter;

            return algorithm;
        }
    }

    internal class NullAlgorithm : FractalAlgorithm
    {
        public override AlgorithmResult Compute(Complex z) => new AlgorithmResult() { succeeded = false };
    }

    public class MullerFractal : FractalAlgorithm
    {

        public override AlgorithmResult Compute(Complex z)
        {
            int iterationsLeft = MaximumIterationCount;
            Complex delta;
            Complex zz, zzz, fz, fzz, fzzz;
            Complex q, A, B, C;

            zzz = z;
            fzzz = Derivatives[0].Compute(z);

            zz = z * (1 + eps);
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

                delta = -(z - zz) * 2 * C / best;

                if (double.IsNaN(delta.Real) || double.IsNaN(delta.Imaginary))
                    return new AlgorithmResult() { z = z, iterations = MaximumIterationCount - iterationsLeft, succeeded = false };

                zzz = zz;
                fzzz = fzz;
                zz = z;
                fzz = fz;
                z += delta;
                fz = Derivatives[0].Compute(z);

            } while (--iterationsLeft >= 0 && !ReachedConvergence(delta, z, fz));

            return new AlgorithmResult() { z = z, iterations = MaximumIterationCount - iterationsLeft, succeeded = ReachedZero(fz) && iterationsLeft >= 0 };//(z, , true);
        }

    }

    public class HalleyFractal : FractalAlgorithm
    {
        public override AlgorithmResult Compute(Complex z)
        {
            int iterationsLeft = MaximumIterationCount;
            Complex delta, fz, f_prim_z, f_bis_z;

            do
            {
                fz = Derivatives[0].Compute(z);
                f_prim_z = Derivatives[1].Compute(z);
                f_bis_z = Derivatives[2].Compute(z);
                delta = -fz * f_prim_z / (f_prim_z * f_prim_z - fz * f_bis_z / 2);

                if (double.IsNaN(delta.Real) || double.IsNaN(delta.Imaginary))
                    return new AlgorithmResult() { z = z, iterations = MaximumIterationCount - iterationsLeft, succeeded = false };

                z += delta;
            } while (--iterationsLeft >= 0 && !ReachedConvergence(delta, z, fz));

            return new AlgorithmResult() { z = z, iterations = MaximumIterationCount - iterationsLeft, succeeded = ReachedZero(fz) && iterationsLeft >= 0 };
        }
    }

    public class QuadrupleFractal : FractalAlgorithm
    {
        public override AlgorithmResult Compute(Complex z)
        {
            int iterationsLeft = MaximumIterationCount;
            Complex delta, fz, f_prim_z, f_bis_z, f_tris_z;

            do
            {
                fz = Derivatives[0].Compute(z);
                f_prim_z = Derivatives[1].Compute(z);
                f_bis_z = Derivatives[2].Compute(z);
                f_tris_z = Derivatives[3].Compute(z);

                delta = -fz / (f_prim_z - fz * f_bis_z / (2 * f_prim_z + fz * (2 / 3 * f_tris_z / f_bis_z - f_bis_z / f_prim_z)));

                if (double.IsNaN(delta.Real) || double.IsNaN(delta.Imaginary))
                    return new AlgorithmResult() { z = z, iterations = MaximumIterationCount - iterationsLeft, succeeded = false };

                z += delta;
            } while (--iterationsLeft >= 0 && !ReachedConvergence(delta, z, fz));

            return new AlgorithmResult() { z = z, iterations = MaximumIterationCount - iterationsLeft, succeeded = ReachedZero(fz) && iterationsLeft >= 0 };
        }
    }

    public class QuadraticFractal : FractalAlgorithm
    {
        public override AlgorithmResult Compute(Complex z)
        {
            int iterationsLeft = MaximumIterationCount;
            Complex delta, delta1, delta2, step, fz, f_prim_z, f_bis_z;

            do
            {
                fz = Derivatives[0].Compute(z);
                f_prim_z = Derivatives[1].Compute(z);
                f_bis_z = Derivatives[2].Compute(z);
                step = Complex.Sqrt(f_prim_z * f_prim_z - 2 * fz * f_bis_z);
                delta1 = -f_prim_z - step;
                delta2 = -f_prim_z + step;
                delta = delta1.Magnitude < delta2.Magnitude ? delta1 / f_bis_z : delta2 / f_bis_z;

                if (double.IsNaN(delta.Real) || double.IsNaN(delta.Imaginary))
                    return new AlgorithmResult() { z = z, iterations = MaximumIterationCount - iterationsLeft, succeeded = false };

                z += delta;
            } while (--iterationsLeft >= 0 && !ReachedConvergence(delta, z, fz));

            return new AlgorithmResult() { z = z, iterations = MaximumIterationCount - iterationsLeft, succeeded = ReachedZero(fz) && iterationsLeft >= 0 };
        }
    }

    public class HalleyNewton : FractalAlgorithm
    {
        public override AlgorithmResult Compute(Complex z)
        {
            int iterationsLeft = MaximumIterationCount;
            Complex delta, fz, f_prim_z, f_bis_z;

            do
            {
                fz = Derivatives[0].Compute(z);
                f_prim_z = Derivatives[1].Compute(z);
                f_bis_z = Derivatives[2].Compute(z);

                delta = fz / f_prim_z;
                delta = fz / (f_prim_z - f_bis_z / 2 * delta);
                delta = fz / (f_prim_z - f_bis_z / 2 * delta);

                if (double.IsNaN(delta.Real) || double.IsNaN(delta.Imaginary))
                    return new AlgorithmResult() { z = z, iterations = MaximumIterationCount - iterationsLeft, succeeded = false };

                z -= delta;
            } while (--iterationsLeft >= 0 && !ReachedConvergence(delta, z, fz));

            return new AlgorithmResult() { z = z, iterations = MaximumIterationCount - iterationsLeft, succeeded = ReachedZero(fz) && iterationsLeft >= 0 };
        }
    }

    public class NewtonFractal : FractalAlgorithm
    {
        public override AlgorithmResult Compute(Complex z)
        {
            int iterationsLeft = MaximumIterationCount;
            Complex delta, fz;

            do
            {
                fz = Derivatives[0].Compute(z);
                delta = -fz / Derivatives[1].Compute(z);

                if (double.IsNaN(delta.Real) || double.IsNaN(delta.Imaginary))
                    return new AlgorithmResult() { z = z, iterations = MaximumIterationCount - iterationsLeft, succeeded = false };

                z += delta;
            } while (--iterationsLeft >= 0 && !ReachedConvergence(delta, z, fz));

            return new AlgorithmResult() { z = z, iterations = MaximumIterationCount - iterationsLeft, succeeded = ReachedZero(fz) && iterationsLeft >= 0 };
        }
    }

    public class MolerRealFractal : FractalAlgorithm
    {
        public override AlgorithmResult Compute(Complex z)
        {
            int iterationsLeft = MaximumIterationCount;
            Complex delta, imaginaryEps, fz, f_prim_z, f_bis_z, fze;

            imaginaryEps = Complex.ImaginaryOne * eps;
            z = z.Magnitude * Math.Sign(z.Real);

            do
            {
                fz = Derivatives[0].Compute(z);
                fze = Derivatives[0].Compute(z + imaginaryEps);

                //fze2 = Derivatives[0].Compute(z + eps / 2);

                f_prim_z = fze.Imaginary / imaginaryEps.Magnitude;
                f_bis_z = (fze - fz).Real / (-imaginaryEps.Magnitude * imaginaryEps.Magnitude);

                //f_prim_z = (8 * fze2.Imaginary - fze.Imaginary) / (3 * eps.Magnitude);
                //f_bis_z =((2 * fze.Real - 32 * fze2.Real) / 6 + 5 * fz) / (eps.Magnitude * eps.Magnitude);

                delta = -fz * f_prim_z / (f_prim_z * f_prim_z - fz * f_bis_z / 2);

                if (double.IsNaN(delta.Real) || double.IsNaN(delta.Imaginary))
                    return new AlgorithmResult() { z = z, iterations = MaximumIterationCount - iterationsLeft, succeeded = false };

                z += delta;
            } while (--iterationsLeft >= 0 && !ReachedConvergence(delta, z, fz));

            return new AlgorithmResult() { z = z, iterations = MaximumIterationCount - iterationsLeft, succeeded = ReachedZero(fz) && iterationsLeft >= 0 };
        }
    }

    public class HalleyWithoutDerivativeFractal : FractalAlgorithm
    {
        public override AlgorithmResult Compute(Complex z)
        {
            int iterationsLeft = MaximumIterationCount;
            Complex delta, zz, zzz, fz, fzz, fzzz, f_prim_z, f_bis_z, zzz_zz;
            fzz = zz = fzzz = zzz = 0;
            fz = Derivatives[0].Compute(z);

            do
            {
                zzz = z * (1 - eps);
                fzzz = Derivatives[0].Compute(zzz);

                zz = z * (1 + eps);
                fzz = Derivatives[0].Compute(zz);

                zzz_zz = zzz - zz;

                f_prim_z = (fzz - fzzz) / (zz - zzz);

                f_bis_z = (fzzz * zz - fzz * zzz + fz * zzz_zz) / (2 * zz * zzz * zzz_zz);

                delta = -fz * f_prim_z / (f_prim_z * f_prim_z - fz * f_bis_z / 2);

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
                z += delta;
                fz = Derivatives[0].Compute(z);
            } while (--iterationsLeft >= 0 && !ReachedConvergence(delta, z, fz));

            return new AlgorithmResult()
            {
                z = z,
                iterations = MaximumIterationCount - iterationsLeft,
                succeeded = iterationsLeft >= 0,
            };
        }
    }

    public class NewtonWithoutDerivativeFractal : FractalAlgorithm
    {
        public override AlgorithmResult Compute(Complex z)
        {
            int iterationsLeft = MaximumIterationCount;
            Complex delta, zz, zzz, fz, fzz, fzzz;
            fzz = zz = fzzz = zzz = 0;
            fz = Derivatives[0].Compute(z);

            do
            {
                zzz = z * (1 - eps);
                fzzz = Derivatives[0].Compute(zzz);

                zz = z * (1 + eps);
                fzz = Derivatives[0].Compute(zz);

                delta = -fz * (zz - zzz) / (fzz - fzzz);

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
                z += delta;
                fz = Derivatives[0].Compute(z);
            } while (--iterationsLeft >= 0 && !ReachedConvergence(delta, z, fz));

            return new AlgorithmResult()
            {
                z = z,
                iterations = MaximumIterationCount - iterationsLeft,
                succeeded = ReachedZero(fz) && iterationsLeft >= 0,
            };
        }
    }


    public class QuadraticWithoutDerivativeFractal : FractalAlgorithm
    {
        public override AlgorithmResult Compute(Complex z)
        {
            int iterationsLeft = MaximumIterationCount;
            Complex delta, zz, zzz, fz, fzz, fzzz, zzz_zz, f_prim_z, f_bis_z, step, delta2;
            fzz = zz = fzzz = zzz = 0;
            fz = Derivatives[0].Compute(z);

            do
            {
                zzz = z * (1 - eps);
                fzzz = Derivatives[0].Compute(zzz);

                zz = z * (1 + eps);
                fzz = Derivatives[0].Compute(zz);

                zzz_zz = zzz - zz;

                f_prim_z = (fzz - fzzz) / (zz - zzz);
                f_bis_z = (fzzz * zz - fzz * zzz + fz * zzz_zz) / (2 * zz * zzz * zzz_zz);

                step = Complex.Sqrt(f_prim_z * f_prim_z - 2 * fz * f_bis_z);
                delta = -f_prim_z - step;
                delta2 = -f_prim_z + step;
                delta = delta.Magnitude < delta2.Magnitude ? delta / f_bis_z : delta2 / f_bis_z;

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
                z += delta;
                fz = Derivatives[0].Compute(z);
            } while (--iterationsLeft >= 0 && !ReachedConvergence(delta, z, fz));

            return new AlgorithmResult()
            {
                z = z,
                iterations = MaximumIterationCount - iterationsLeft,
                succeeded = ReachedZero(fz) && iterationsLeft >= 0,
            };
        }
    }

    public class SecantNewtonFractal : FractalAlgorithm
    {
        public override AlgorithmResult Compute(Complex z)
        {
            int iterationsLeft = MaximumIterationCount;
            Complex delta, zz, zzz, fz, fzz, fzzz, zzz_zz;
            fzz = zz = fzzz = zzz = 0;
            fz = Derivatives[0].Compute(z);

            do
            {
                if (iterationsLeft == MaximumIterationCount)
                {
                    zzz = z * (1 - eps);
                    fzzz = Derivatives[0].Compute(zzz);

                    zz = z * (1 + eps);
                    fzz = Derivatives[0].Compute(zz);
                }
                else
                {
                    zzz = z * (1 - eps);
                    fzzz = Derivatives[0].Compute(zzz);
                }

                zzz_zz = zzz - zz;

                delta = -fz / (zz * fzzz / (zzz_zz * zzz) - zzz * fzz / (zzz_zz * zz) + (zzz + zz) * fz / (zz * zzz));

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
                z += delta;
                fz = Derivatives[0].Compute(z);

            } while (--iterationsLeft >= 0 && !ReachedConvergence(delta, z, fz));

            return new AlgorithmResult()
            {
                z = z,
                iterations = MaximumIterationCount - iterationsLeft,
                succeeded = ReachedZero(fz) && iterationsLeft >= 0,
            };
        }
    }

    public class SecantFractal : ParametrizedFractalAlgorithm
    {
        public override AlgorithmResult Compute(Complex z)
        {
            //var orderOfConvergence = new Stack<double>();

            int iterationsLeft = MaximumIterationCount;
            Complex delta, zz, zzz, fz, fzz, fzzz;
            fzz = zz = fzzz = zzz = 0;
            fz = Derivatives[0].Compute(z);

            do
            {
                if (iterationsLeft == MaximumIterationCount)
                {
                    zzz = z * (1 - eps);
                    fzzz = Derivatives[0].Compute(zzz);

                    zz = z * (1 + eps);
                    fzz = Derivatives[0].Compute(zz);
                }
                else if (iterationsLeft == MaximumIterationCount - 1)
                {
                    zzz = z * (1 - eps);
                    fzzz = Derivatives[0].Compute(zzz);
                }

                delta = -fz / (Parameter * ((fzz - fz) * (zzz - z) * (zzz - z) - (fzzz - fz) * (zz - z) * (zz - z)) / ((zz - z) * (zzz - z) * (zzz - zz)) + (1 - Parameter) * (fzz - fz) / (zz - z));

                if (double.IsNaN(delta.Real) || double.IsNaN(delta.Imaginary))
                    return new AlgorithmResult()
                    {
                        z = z,
                        iterations = MaximumIterationCount - iterationsLeft,
                        succeeded = false,
                    };

                // if it's the secant method
                if (Parameter == 0)
                {
                    if (fz.Magnitude < fzz.Magnitude)
                    {
                        fzz = fz;
                        zz = z;
                    }

                    z += delta;
                    fz = Derivatives[0].Compute(z);
                }
                else
                {
                    zzz = zz;
                    fzzz = fzz;
                    zz = z;
                    fzz = fz;
                    z += delta;
                    fz = Derivatives[0].Compute(z);
                }


                //Complex ideal = -9;// new Complex(Math.Round(z.Real), Math.Round(z.Imaginary));
                //orderOfConvergence.Push((Complex.Log((ideal - z).Magnitude / (ideal - zz).Magnitude) / Complex.Log((ideal - zz).Magnitude / (ideal - zzz).Magnitude)).Magnitude);
            } while (--iterationsLeft >= 0 && !ReachedConvergence(delta, z, fz));

            return new AlgorithmResult()
            {
                z = z,
                iterations = MaximumIterationCount - iterationsLeft,
                succeeded = ReachedZero(fz) && iterationsLeft >= 0,
                //orderOfConvergence = orderOfConvergence.Peek(),
            };
        }
    }

    public class InverseQuadraticFractal : FractalAlgorithm
    {
        public override AlgorithmResult Compute(Complex z)
        {

            int iterationsLeft = MaximumIterationCount;
            Complex delta;
            Complex zz, zzz, fz, fzz, fzzz;
            zzz = z;
            fzzz = Derivatives[0].Compute(z);

            zz = z * (1 + eps);
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
                delta = -z +
                    (
                        (fzz * fz) / ((fzzz - fzz) * (fzzz - fz)) * zzz +
                        (fzzz * fz) / ((fzz - fzzz) * (fzz - fz)) * zz +
                        (fzzz * fzz) / ((fz - fzzz) * (fz - fzz)) * z
                    );

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
                z += delta;
                fz = Derivatives[0].Compute(z);

                //Complex ideal = -9;// new Complex(Math.Round(z.Real), Math.Round(z.Imaginary));
                //orderOfConvergence.Push((Complex.Log((ideal - z).Magnitude / (ideal - zz).Magnitude) / Complex.Log((ideal - zz).Magnitude / (ideal - zzz).Magnitude)).Magnitude);
            } while (--iterationsLeft >= 0 && !ReachedConvergence(delta, z, fz));

            return new AlgorithmResult()
            {
                z = z,
                iterations = MaximumIterationCount - iterationsLeft,
                succeeded = ReachedZero(fz) && iterationsLeft >= 0,
                //orderOfConvergence = orderOfConvergence.Peek(),
            };
        }

    }

    public class SteffensenFractal : FractalAlgorithm
    {
        public override AlgorithmResult Compute(Complex z)
        {
            int iterationsLeft = MaximumIterationCount;
            Complex delta, fz;

            do
            {
                fz = Derivatives[0].Compute(z);
                delta = -fz * fz / (Derivatives[0].Compute(z + fz) - fz);
                if (double.IsNaN(delta.Real) || double.IsNaN(delta.Imaginary))
                    return new AlgorithmResult() { z = z, iterations = MaximumIterationCount - iterationsLeft, succeeded = false };

                z += delta;
            } while (--iterationsLeft >= 0 && !ReachedConvergence(delta, z, fz));

            return new AlgorithmResult() { z = z, iterations = MaximumIterationCount - iterationsLeft, succeeded = ReachedZero(fz) && iterationsLeft >= 0 };
        }
    }

    public class CustomFractal : FractalAlgorithm
    {
        public override AlgorithmResult Compute(Complex z)
        {
            int iterationsLeft = MaximumIterationCount;
            Complex zz;

            do
            {
                zz = z;
                z = Derivatives[0].Compute(z);
            } while (--iterationsLeft >= 0);

            return new AlgorithmResult() { z = z, iterations = MaximumIterationCount - iterationsLeft, succeeded = true };
        }
    }
}
