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
                    case "^":
                        cf = new PowerCF();
                        break;
                    case "tan(":
                        cf = new TanCF();
                        break;
                    case "tanh(":
                        cf = new TanhCF();
                        break;
                    case "sqrt(":
                        cf = new SqrtCF();
                        break;
                    case "sinh(":
                        cf = new SinhCF();
                        break;
                    case "sin(":
                        cf = new SinCF();
                        break;
                    case "log(":
                        cf = new LogCF();
                        break;
                    case "log10(":
                        cf = new Log10CF();
                        break;
                    case "[exp](":
                        cf = new ExpCF();
                        break;
                    case "cos(":
                        cf = new CosCF();
                        break;
                    case "cosh(":
                        cf = new CoshCF();
                        break;
                    case "atan(":
                        cf = new AtanCF();
                        break;
                    case "asin(":
                        cf = new AsinCF();
                        break;
                    case "acos(":
                        cf = new AcosCF();
                        break;

                    default:
                        return null;
                }

                if (parameter is ComplexFunction[])
                    cf.InsertArguments((ComplexFunction[])parameter);

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
            public override Complex Compute(Complex z) => Arguments[0].Compute(z) - Arguments[1].Compute(z);

            public override ComplexFunction GetDerivative() =>
                new DifferenceCF(Arguments[0].GetDerivative(),
                          Arguments[1].GetDerivative());

            public DifferenceCF(params ComplexFunction[] Arguments) => InsertArguments(Arguments);
        }

        class SumCF : ComplexFunction
        {
            public override Complex Compute(Complex z) => Arguments[0].Compute(z) + Arguments[1].Compute(z);

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
            public override ComplexFunction GetDerivative() => new ProductCF(new CosCF(Arguments[0]), Arguments[0].GetDerivative());
            public SinCF(params ComplexFunction[] Arguments) => InsertArguments(Arguments);
        }

        class CosCF : ComplexFunction
        {
            public override Complex Compute(Complex z) => Complex.Cos(Arguments[0].Compute(z));
            public override ComplexFunction GetDerivative() =>
                new ProductCF(new NegateCF(new SinCF(Arguments[0])), Arguments[0].GetDerivative());
            public CosCF(params ComplexFunction[] Arguments) => InsertArguments(Arguments);
        }

        class PowerCF : ComplexFunction
        {
            public override Complex Compute(Complex z) => Complex.Pow(Arguments[0].Compute(z), Arguments[1].Compute(z));
            public override ComplexFunction GetDerivative() =>
                new ProductCF(new PowerCF(Arguments[0], Arguments[1]),
                    new SumCF(new QuotientCF(new ProductCF(Arguments[1], Arguments[0].GetDerivative()), Arguments[0]),
                        new ProductCF(new LogCF(Arguments[0]), Arguments[1].GetDerivative())));
            public PowerCF(params ComplexFunction[] Arguments) => InsertArguments(Arguments);
        }

        class LogCF : ComplexFunction
        {
            public override Complex Compute(Complex z) => Complex.Log(Arguments[0].Compute(z));
            public override ComplexFunction GetDerivative() => new QuotientCF(Arguments[0].GetDerivative(), Arguments[0]);
            public LogCF(params ComplexFunction[] Arguments) => InsertArguments(Arguments);
        }

        class Log10CF : ComplexFunction
        {
            public override Complex Compute(Complex z) => Complex.Log10(Arguments[0].Compute(z));
            public override ComplexFunction GetDerivative() =>
                new QuotientCF(Arguments[0].GetDerivative(), new ProductCF(Arguments[0], new ConstantCF(Math.Log(10))));
            public Log10CF(params ComplexFunction[] Arguments) => InsertArguments(Arguments);
        }

        class TanCF : ComplexFunction
        {
            public override Complex Compute(Complex z) => Complex.Tan(Arguments[0].Compute(z));
            public override ComplexFunction GetDerivative() =>
                new QuotientCF(Arguments[0].GetDerivative(), new ProductCF(new CosCF(Arguments[0]), new CosCF(Arguments[0])));
            public TanCF(params ComplexFunction[] Arguments) => InsertArguments(Arguments);
        }

        class SinhCF : ComplexFunction
        {
            public override Complex Compute(Complex z) => Complex.Sinh(Arguments[0].Compute(z));
            public override ComplexFunction GetDerivative() =>
                new ProductCF(Arguments[0].GetDerivative(), new CoshCF(Arguments[0]));
            public SinhCF(params ComplexFunction[] Arguments) => InsertArguments(Arguments);
        }

        class CoshCF : ComplexFunction
        {
            public override Complex Compute(Complex z) => Complex.Cosh(Arguments[0].Compute(z));
            public override ComplexFunction GetDerivative() =>
                new ProductCF(Arguments[0].GetDerivative(), new SinhCF(Arguments[0]));
            public CoshCF(params ComplexFunction[] Arguments) => InsertArguments(Arguments);
        }

        class TanhCF : ComplexFunction
        {
            public override Complex Compute(Complex z) => Complex.Tanh(Arguments[0].Compute(z));
            public override ComplexFunction GetDerivative() =>
                new QuotientCF(Arguments[0].GetDerivative(), new ProductCF(new CoshCF(Arguments[0]), new CoshCF(Arguments[0])));
            public TanhCF(params ComplexFunction[] Arguments) => InsertArguments(Arguments);
        }

        class SqrtCF : ComplexFunction
        {
            public override Complex Compute(Complex z) => Complex.Sqrt(Arguments[0].Compute(z));
            public override ComplexFunction GetDerivative() =>
                new QuotientCF(Arguments[0].GetDerivative(), new ProductCF(new SqrtCF(Arguments[0]), new ConstantCF(2)));
            public SqrtCF(params ComplexFunction[] Arguments) => InsertArguments(Arguments);
        }

        class ExpCF : ComplexFunction
        {
            public override Complex Compute(Complex z) => Complex.Exp(Arguments[0].Compute(z));
            public override ComplexFunction GetDerivative() =>
                new ProductCF(Arguments[0].GetDerivative(), new ExpCF(Arguments[0]));
            public ExpCF(params ComplexFunction[] Arguments) => InsertArguments(Arguments);
        }

        class AtanCF : ComplexFunction
        {
            public override Complex Compute(Complex z) => Complex.Atan(Arguments[0].Compute(z));
            public override ComplexFunction GetDerivative() =>
                new QuotientCF(Arguments[0].GetDerivative(), new SumCF(
                    new ConstantCF(1),
                    new ProductCF(Arguments[0], Arguments[0])
                    ));
            public AtanCF(params ComplexFunction[] Arguments) => InsertArguments(Arguments);
        }

        class AsinCF : ComplexFunction
        {
            public override Complex Compute(Complex z) => Complex.Asin(Arguments[0].Compute(z));
            public override ComplexFunction GetDerivative() =>
                new QuotientCF(Arguments[0].GetDerivative(),
                    new SqrtCF(
                        new DifferenceCF(
                            new ConstantCF(1),
                            new ProductCF(Arguments[0], Arguments[0])
                        )
                    ));
            public AsinCF(params ComplexFunction[] Arguments) => InsertArguments(Arguments);
        }

        class AcosCF : ComplexFunction
        {
            public override Complex Compute(Complex z) => Complex.Acos(Arguments[0].Compute(z));
            public override ComplexFunction GetDerivative() =>
                new QuotientCF(new NegateCF(Arguments[0].GetDerivative()),
                    new SqrtCF(
                        new DifferenceCF(
                            new ConstantCF(1),
                            new ProductCF(Arguments[0], Arguments[0])
                        )
                    ));
            public AcosCF(params ComplexFunction[] Arguments) => InsertArguments(Arguments);
        }
    }
}
