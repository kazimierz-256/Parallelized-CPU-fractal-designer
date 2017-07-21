using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Fractal_Designer
{

    class Interpreter
    {
        //interface AbstractExpression
        //{
        //    List<IComplexFunction> arguments { get; set; }
        //    Complex compute();
        //}

        struct OperatorInformation
        {
            public bool IsOpening;
            public byte Priority;
            public int ArgumentCount;
            public Func<Complex, IComplexFunction[], Complex> function;
        }

        static Dictionary<string, OperatorInformation> operators = new Dictionary<string, OperatorInformation>() {
            { "abs(", new OperatorInformation{ArgumentCount=1, IsOpening=true, function=(Complex z, IComplexFunction[] args) => Complex.Abs(args[0].Compute(z)) } },
            { "acos(", new OperatorInformation{ArgumentCount=1, IsOpening=true, function=(Complex z, IComplexFunction[] args) => Complex.Acos(args[0].Compute(z)) } },
            { "asin(", new OperatorInformation{ArgumentCount=1, IsOpening=true, function=(Complex z, IComplexFunction[] args) => Complex.Asin(args[0].Compute(z)) } },
            { "atan(", new OperatorInformation{ArgumentCount=1, IsOpening=true, function=(Complex z, IComplexFunction[] args) => Complex.Atan(args[0].Compute(z)) } },
            { "'(", new OperatorInformation{ArgumentCount=1, IsOpening=true, function=(Complex z, IComplexFunction[] args) => Complex.Conjugate(args[0].Compute(z)) } },
            { "-(", new OperatorInformation{ArgumentCount=1, IsOpening=true, function=(Complex z, IComplexFunction[] args) => Complex.Negate(args[0].Compute(z)) } },
            { "cos(", new OperatorInformation{ArgumentCount=1, IsOpening=true, function=(Complex z, IComplexFunction[] args) => Complex.Cos(args[0].Compute(z)) } },
            { "cosh(", new OperatorInformation{ArgumentCount=1, IsOpening=true, function=(Complex z, IComplexFunction[] args) => Complex.Cosh(args[0].Compute(z)) } },
            { "exp(", new OperatorInformation{ArgumentCount=1, IsOpening=true, function=(Complex z, IComplexFunction[] args) => Complex.Exp(args[0].Compute(z)) } },
            { "log(", new OperatorInformation{ArgumentCount=1, IsOpening=true, function=(Complex z, IComplexFunction[] args) => Complex.Log(args[0].Compute(z)) } },
            { "log10(", new OperatorInformation{ArgumentCount=1, IsOpening=true, function=(Complex z, IComplexFunction[] args) => Complex.Log10(args[0].Compute(z)) } },
            { "sin(", new OperatorInformation{ArgumentCount=1, IsOpening=true, function=(Complex z, IComplexFunction[] args) => Complex.Sin(args[0].Compute(z)) } },
            { "sinh(", new OperatorInformation{ArgumentCount=1, IsOpening=true, function=(Complex z, IComplexFunction[] args) => Complex.Sinh(args[0].Compute(z)) } },
            { "sqrt(", new OperatorInformation{ArgumentCount=1, IsOpening=true, function=(Complex z, IComplexFunction[] args) => Complex.Sqrt(args[0].Compute(z)) } },
            { "tan(", new OperatorInformation{ArgumentCount=1, IsOpening=true, function=(Complex z, IComplexFunction[] args) => Complex.Tan(args[0].Compute(z)) } },
            { "tanh(", new OperatorInformation{ArgumentCount=1, IsOpening=true, function=(Complex z, IComplexFunction[] args) => Complex.Tanh(args[0].Compute(z)) } },
            { "^", new OperatorInformation{Priority=80, ArgumentCount=2, function=(Complex z, IComplexFunction[] args) => Complex.Pow(args[0].Compute(z), args[1].Compute(z)) } },
            { "**", new OperatorInformation{Priority=80, ArgumentCount=2, function=(Complex z, IComplexFunction[] args) => Complex.Pow(args[0].Compute(z), args[1].Compute(z)) } },
            { "*", new OperatorInformation{Priority=60, ArgumentCount=2, function=(Complex z, IComplexFunction[] args) => args[0].Compute(z)*args[1].Compute(z) } },
            { "/", new OperatorInformation{Priority=60, ArgumentCount=2, function=(Complex z, IComplexFunction[] args) => args[0].Compute(z)/args[1].Compute(z) } },
            { "+", new OperatorInformation{Priority=40, ArgumentCount=2, function=(Complex z, IComplexFunction[] args) => args[0].Compute(z)+args[1].Compute(z) } },
            { "-", new OperatorInformation{Priority=40, ArgumentCount=2, function=(Complex z, IComplexFunction[] args) => args[0].Compute(z)-args[1].Compute(z) } },
            { ")", new OperatorInformation{} },
            { "(", new OperatorInformation{IsOpening=true} }
        };

        class ParseTreeGenerator
        {
            Stack<IComplexFunction> ExpressionStack = new Stack<IComplexFunction>();
            Stack<int> DeepnessStack = new Stack<int>();

            public bool TryParseExpression(string expression, out IComplexFunction complexFunction, out int outdeepness, int deepness, bool mutable = false)
            {
                complexFunction = null;
                outdeepness = -1;

                if (operators.ContainsKey(expression))
                {

                    if (mutable)
                    {
                        if (expression == "-" && ExpressionStack.Count == 1)
                        {
                            //= "Could fix the minus...";
                        }
                        // an unsupported operator
                        if (operators[expression].function == null || ExpressionStack.Count < operators[expression].ArgumentCount)
                            return false;

                        // collect the arguments
                        var arguments = new IComplexFunction[operators[expression].ArgumentCount];

                        // remember about the reverse order!
                        for (int i = operators[expression].ArgumentCount - 1; i >= 0; i--)
                        {
                            arguments[i] = ExpressionStack.Pop();
                            if (DeepnessStack.Peek() != deepness)
                            {

                            }
                            DeepnessStack.Pop();
                        }

                        complexFunction = new ClassicComplexFunction(z => operators[expression].function(z, arguments));
                        outdeepness = deepness - 1;
                    }
                }
                else
                {
                    // a constant expression not an operator
                    switch (expression)
                    {
                        case "x":
                        case "z":
                            complexFunction = new ArgumentComplexFunction();
                            break;
                        case "i":
                            complexFunction = new ConstantComplexFunction(Complex.ImaginaryOne);
                            break;
                        case "e":
                            complexFunction = new ConstantComplexFunction(Math.E);
                            break;
                        case "pi":
                            complexFunction = new ConstantComplexFunction(Math.PI);
                            break;
                        default:
                            // could be a number
                            bool succeededParsing = double.TryParse(expression, out double result);

                            if (succeededParsing)
                            {
                                complexFunction = new ConstantComplexFunction(result);
                            }
                            else
                            {
                                return false;
                            }
                            break;
                    }
                }

                // let's be fair and do not provide anythin undefined if the user just asks for parsability
                if (!mutable)
                    complexFunction = null;
                outdeepness = deepness;
                return true;
            }

            public bool Enqueue(string expression, int deepness)
            {
                bool succeeded = TryParseExpression(expression, out IComplexFunction complexFunction, out int outdeepness, deepness, true);

                if (succeeded)
                {
                    ExpressionStack.Push(complexFunction);
                    DeepnessStack.Push(outdeepness);
                }

                return succeeded;
            }

            public bool TryParseTree(out IComplexFunction returnValue)
            {
                if (ExpressionStack.Count == 1)
                {
                    returnValue = ExpressionStack.Peek();
                    return true;
                }
                else
                {
                    returnValue = null;
                    return false;
                }
            }
        }

        public static bool TryParse(string formula, out IComplexFunction returnValue)
        {
            var expressionQueue = new ParseTreeGenerator();
            var operatorStack = new Stack<string>();
            string expression = string.Empty;
            returnValue = null;
            int deepness = 0;

            // remove all whitespace characters
            formula = Regex.Replace(formula, @"\s+", "");

            for (int cursor = 0; cursor < formula.Length; cursor++)
            {
                expression += formula[cursor].ToString().ToLower();

                var succeeded = expressionQueue.TryParseExpression(expression, out _, out _, deepness);

                if (!operators.ContainsKey(expression) && cursor < formula.Length - 1)
                    succeeded = succeeded && !expressionQueue.TryParseExpression(expression + formula[cursor + 1], out _, out _, deepness);

                // if able to parse, but no further (lazy evaluation)
                if (!succeeded)
                    continue;

                // encountered a space! try parsing the expression!
                if (operators.ContainsKey(expression))
                {

                    if (expression == ",")
                    {
                        // crush to another "," or to "sin("
                    }
                    else if (expression == ")")
                    {
                        // crushing until "(" or "sin(" found
                        while (operatorStack.Count > 0 && !operators[operatorStack.Peek()].IsOpening)
                        {
                            if (!expressionQueue.Enqueue(operatorStack.Pop(), deepness))
                                return false;
                        }

                        // no "(" nor "sin(" found that is an error!
                        if (operatorStack.Count == 0)
                            return false;

                        var openingOperator = operatorStack.Pop();

                        // an opening operator that is not "(" should carry useful information with it, let's keep it!
                        if (openingOperator != "(")
                            if (!expressionQueue.Enqueue(openingOperator, deepness))
                                return false;

                        --deepness;

                    }
                    else if (operators[expression].IsOpening)
                    {
                        // highest priority when adding yet lowest when already inside (to stop propagating)
                        operatorStack.Push(expression);
                        ++deepness;
                    }
                    else
                    {
                        while (operatorStack.Count > 0 && !operators[operatorStack.Peek()].IsOpening && operators[operatorStack.Peek()].Priority >= operators[expression].Priority)
                        {
                            if (!expressionQueue.Enqueue(operatorStack.Pop(), deepness))
                                return false;
                        }
                        
                        operatorStack.Push(expression);
                    }
                }
                else
                {
                    // a constant value like "12345", or argument "z"
                    if (!expressionQueue.Enqueue(expression, deepness))
                        return false;
                }

                // clear the expression before proceeding
                expression = string.Empty;
            }

            while (operatorStack.Count > 0)
            {
                if (!expressionQueue.Enqueue(operatorStack.Pop(), deepness))
                    return false;
            }

            return expressionQueue.TryParseTree(out returnValue);
        }
    }

}
