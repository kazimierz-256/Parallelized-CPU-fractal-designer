using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Fractal_Designer
{
    namespace ComplexFunction
    {
        // new experiments
        public abstract class ComplexFunction
        {
            public abstract ComplexFunction GetDerivative();
            public abstract Complex Compute(Complex z);
            public virtual ComplexFunction[] Arguments { get; set; }
            public virtual void InsertArguments(params ComplexFunction[] Arguments) => this.Arguments = Arguments;
        }

        public static class Generator
        {
            public static ComplexFunction Generate(string name, object parameter = null)
            {
                ComplexFunction cf = null;
                switch (name.ToLower())
                {
                    //constants
                    case "x":
                    case "z":
                        return new ArgumentCF();
                    case "_":
                        return new ConstantCF(-1);
                    case "i":
                        return new ConstantCF(Complex.ImaginaryOne);
                    case "e":
                        return new ConstantCF(Math.E);
                    case "pi":
                        return new ConstantCF(Math.PI);
                    case "custom_constant":
                        return new ConstantCF((double)parameter);

                    //operators
                    case "-":
                        cf = new DifferenceCF();
                        break;
                    case "+":
                        cf = new SumCF();
                        break;
                    case "/":
                        cf = new QuotientCF();
                        break;
                    case "*":
                    case "&":
                        cf = new ProductCF();
                        break;
                    //case "^":
                    //    return null;
                    //case "tan(":
                    //    return null;
                    //case "tanh(":
                    //    return null;
                    //case "sqrt(":
                    //    return null;
                    //case "sinh(":
                    //    return null;
                    //case "sin(":
                    //    return null;
                    //case "log(":
                    //    return null;
                    //case "log10(":
                    //    return null;
                    //case "[exp](":
                    //    return null;
                    //case "cos(":
                    //    return null;
                    //case "cosh(":
                    //    return null;
                    //case "polar(":
                    //    return null;
                    //case "[imag](":
                    //    return null;
                    //case "real(":
                    //    return null;
                    //case "phase(":
                    //    return null;
                    //case "conjugate(":
                    //    return null;
                    //case "atan(":
                    //    return null;
                    //case "asin(":
                    //    return null;
                    //case "acos(":
                    //    return null;
                    //case "abs(":
                    //    return null;

                    default:
                        return null;
                }

                if (parameter != null)
                    cf.InsertArguments((ComplexFunction[]) parameter);

                return cf;
            }
        }

        class ArgumentCF : ComplexFunction
        {
            public override Complex Compute(Complex z) => z;
            public override ComplexFunction GetDerivative() => new ConstantCF(1);
        }

        class ConstantCF : ComplexFunction
        {
            public Complex Constant;
            public override Complex Compute(Complex z) => Constant;
            public override ComplexFunction GetDerivative() => new ConstantCF(0);
            public ConstantCF(Complex Constant) => this.Constant = Constant;
        }

        class DifferenceCF : ComplexFunction
        {
            public override Complex Compute(Complex z) =>
                Arguments[0].Compute(z) - Arguments[1].Compute(z);

            public override ComplexFunction GetDerivative() =>
                new DifferenceCF(Arguments[0].GetDerivative(),
                          Arguments[1].GetDerivative());

            public DifferenceCF(params ComplexFunction[] Arguments) => InsertArguments(Arguments);
        }

        class SumCF : ComplexFunction
        {
            public override Complex Compute(Complex z) =>
                Arguments[0].Compute(z) + Arguments[1].Compute(z);

            public override ComplexFunction GetDerivative() =>
                new SumCF(Arguments[0].GetDerivative(),
                          Arguments[1].GetDerivative());

            public SumCF(params ComplexFunction[] Arguments) => InsertArguments(Arguments);
        }

        class QuotientCF : ComplexFunction
        {
            public override Complex Compute(Complex z) => Arguments[0].Compute(z) / Arguments[1].Compute(z);
            public override ComplexFunction GetDerivative() =>
                new DifferenceCF(new QuotientCF(Arguments[0].GetDerivative(), Arguments[1]),
                          new QuotientCF(new ProductCF(Arguments[0], Arguments[1].GetDerivative()),
                              new ProductCF(Arguments[1], Arguments[1])
                              ));
            public QuotientCF(params ComplexFunction[] Arguments) => InsertArguments(Arguments);
        }

        class ProductCF : ComplexFunction
        {
            public override Complex Compute(Complex z) => Arguments[0].Compute(z) * Arguments[1].Compute(z);
            public override ComplexFunction GetDerivative() =>
                new SumCF(new ProductCF(Arguments[0], Arguments[1].GetDerivative()),
                          new ProductCF(Arguments[0].GetDerivative(), Arguments[1]));
            public ProductCF(params ComplexFunction[] Arguments) => InsertArguments(Arguments);
        }

        class NegateCF : ComplexFunction
        {
            public override Complex Compute(Complex z) => -Arguments[0].Compute(z);
            public override ComplexFunction GetDerivative() => new NegateCF(Arguments[0].GetDerivative());
            public NegateCF(params ComplexFunction[] Arguments) => InsertArguments(Arguments);
        }

        class SinCF : ComplexFunction
        {
            public override Complex Compute(Complex z) => Complex.Sin(Arguments[0].Compute(z));
            public override ComplexFunction GetDerivative() => new CosCF();
            public SinCF(params ComplexFunction[] Arguments) => InsertArguments(Arguments);
        }

        class CosCF : ComplexFunction
        {
            public override Complex Compute(Complex z) => Complex.Cos(Arguments[0].Compute(z));
            public override ComplexFunction GetDerivative() => new NegateCF(new SinCF());
            public CosCF(params ComplexFunction[] Arguments) => InsertArguments(Arguments);
        }
    }
}
