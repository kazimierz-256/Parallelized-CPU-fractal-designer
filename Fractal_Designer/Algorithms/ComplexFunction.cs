using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Fractal_Designer
{
    class ClassicComplexFunction : IComplexFunction
    {
        Func<Complex, Complex> function;

        public Complex Compute(Complex z) => function(z);

        public ClassicComplexFunction(Func<Complex, Complex> function) => this.function = function;

    }

    class ConstantComplexFunction : IComplexFunction
    {
        Complex constant;

        public Complex Compute(Complex z) => constant;

        public ConstantComplexFunction(Complex constant) => this.constant = constant;

    }

    class ArgumentComplexFunction : IComplexFunction
    {
        public Complex Compute(Complex z) => z;
    }

    class RandomComplexFunction : IComplexFunction
    {
        static Random random = new Random();
        public Complex Compute(Complex z) => random.NextDouble();
    }
}
