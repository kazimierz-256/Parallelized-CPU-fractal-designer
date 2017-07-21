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
            public bool IgnoreLeftArgument;
            public byte Priority;
            public int ArgumentCount;
            public Func<Complex, IComplexFunction[], Complex> function;
        }
        struct BracketInformation
        {
            public int ArgumentCount;
            public Func<Complex, IComplexFunction[], Complex> function;
        }
        struct ConstantInformation
        {
            public IComplexFunction complexFunction;
        }

        static Dictionary<string, BracketInformation> openbrackets = new Dictionary<string, BracketInformation>()
        {
            //{ "abs(", new OperatorInformation{ArgumentCount=1, IsOpening=true, function=(Complex z, IComplexFunction[] args) => Complex.Abs(args[0].Compute(z)) } },
            //{ "acos(", new OperatorInformation{ArgumentCount=1, IsOpening=true, function=(Complex z, IComplexFunction[] args) => Complex.Acos(args[0].Compute(z)) } },
            //{ "asin(", new OperatorInformation{ArgumentCount=1, IsOpening=true, function=(Complex z, IComplexFunction[] args) => Complex.Asin(args[0].Compute(z)) } },
            //{ "atan(", new OperatorInformation{ArgumentCount=1, IsOpening=true, function=(Complex z, IComplexFunction[] args) => Complex.Atan(args[0].Compute(z)) } },
            //{ "conjugate(", new OperatorInformation{ArgumentCount=1, IsOpening=true, function=(Complex z, IComplexFunction[] args) => Complex.Conjugate(args[0].Compute(z)) } },
            //{ "phase(", new OperatorInformation{ArgumentCount=1, IsOpening=true, function=(Complex z, IComplexFunction[] args) => args[0].Compute(z).Phase } },
            //{ "real(", new OperatorInformation{ArgumentCount=1, IsOpening=true, function=(Complex z, IComplexFunction[] args) => args[0].Compute(z).Real } },
            //{ "imag(", new OperatorInformation{ArgumentCount=1, IsOpening=true, function=(Complex z, IComplexFunction[] args) => args[0].Compute(z).Imaginary } },
            //{ "polar(", new OperatorInformation{ArgumentCount=2, IsOpening=true, function=(Complex z, IComplexFunction[] args) => Complex.FromPolarCoordinates(args[0].Compute(z).Magnitude, args[1].Compute(z).Magnitude) } },
            //{ "cos(", new OperatorInformation{ArgumentCount=1, IsOpening=true, function=(Complex z, IComplexFunction[] args) => Complex.Cos(args[0].Compute(z)) } },
            //{ "cosh(", new OperatorInformation{ArgumentCount=1, IsOpening=true, function=(Complex z, IComplexFunction[] args) => Complex.Cosh(args[0].Compute(z)) } },
            //{ "exp(", new OperatorInformation{ArgumentCount=1, IsOpening=true, function=(Complex z, IComplexFunction[] args) => Complex.Exp(args[0].Compute(z)) } },
            //{ "log(", new OperatorInformation{ArgumentCount=1, IsOpening=true, function=(Complex z, IComplexFunction[] args) => Complex.Log(args[0].Compute(z)) } },
            //{ "log10(", new OperatorInformation{ArgumentCount=1, IsOpening=true, function=(Complex z, IComplexFunction[] args) => Complex.Log10(args[0].Compute(z)) } },
            //{ "sin(", new OperatorInformation{ArgumentCount=1, IsOpening=true, function=(Complex z, IComplexFunction[] args) => Complex.Sin(args[0].Compute(z)) } },
            //{ "sinh(", new OperatorInformation{ArgumentCount=1, IsOpening=true, function=(Complex z, IComplexFunction[] args) => Complex.Sinh(args[0].Compute(z)) } },
            //{ "sqrt(", new OperatorInformation{ArgumentCount=1, IsOpening=true, function=(Complex z, IComplexFunction[] args) => Complex.Sqrt(args[0].Compute(z)) } },
            //{ "tan(", new OperatorInformation{ArgumentCount=1, IsOpening=true, function=(Complex z, IComplexFunction[] args) => Complex.Tan(args[0].Compute(z)) } },
            //{ "tanh(", new OperatorInformation{ArgumentCount=1, IsOpening=true, function=(Complex z, IComplexFunction[] args) => Complex.Tanh(args[0].Compute(z)) } }
        };

        static Dictionary<string, OperatorInformation> operators = new Dictionary<string, OperatorInformation>()
        {
            { "abs(", new OperatorInformation{ArgumentCount=1, IsOpening=true, function=(Complex z, IComplexFunction[] args) => Complex.Abs(args[0].Compute(z)) } },
            { "acos(", new OperatorInformation{ArgumentCount=1, IsOpening=true, function=(Complex z, IComplexFunction[] args) => Complex.Acos(args[0].Compute(z)) } },
            { "asin(", new OperatorInformation{ArgumentCount=1, IsOpening=true, function=(Complex z, IComplexFunction[] args) => Complex.Asin(args[0].Compute(z)) } },
            { "atan(", new OperatorInformation{ArgumentCount=1, IsOpening=true, function=(Complex z, IComplexFunction[] args) => Complex.Atan(args[0].Compute(z)) } },
            { "conjugate(", new OperatorInformation{ArgumentCount=1, IsOpening=true, function=(Complex z, IComplexFunction[] args) => Complex.Conjugate(args[0].Compute(z)) } },
            { "phase(", new OperatorInformation{ArgumentCount=1, IsOpening=true, function=(Complex z, IComplexFunction[] args) => args[0].Compute(z).Phase } },
            { "real(", new OperatorInformation{ArgumentCount=1, IsOpening=true, function=(Complex z, IComplexFunction[] args) => args[0].Compute(z).Real } },
            { "imag(", new OperatorInformation{ArgumentCount=1, IsOpening=true, function=(Complex z, IComplexFunction[] args) => args[0].Compute(z).Imaginary } },
            { "polar(", new OperatorInformation{ArgumentCount=2, IsOpening=true, function=(Complex z, IComplexFunction[] args) => Complex.FromPolarCoordinates(args[0].Compute(z).Magnitude, args[1].Compute(z).Magnitude) } },
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
            // strange multiplication
            { "&", new OperatorInformation{Priority=90, ArgumentCount=2, function=(Complex z, IComplexFunction[] args) => args[0].Compute(z)*args[1].Compute(z) } },
            // how to process only the righthandside argument??
            { "'", new OperatorInformation{Priority=90, ArgumentCount=1, IgnoreLeftArgument = true, function=(Complex z, IComplexFunction[] args) => Complex.Conjugate(args[0].Compute(z)) } },
            { "^", new OperatorInformation{Priority=80, ArgumentCount=2, function=(Complex z, IComplexFunction[] args) => Complex.Pow(args[0].Compute(z), args[1].Compute(z)) } },
            { "*", new OperatorInformation{Priority=60, ArgumentCount=2, function=(Complex z, IComplexFunction[] args) => args[0].Compute(z)*args[1].Compute(z) } },
            { "/", new OperatorInformation{Priority=60, ArgumentCount=2, function=(Complex z, IComplexFunction[] args) => args[0].Compute(z)/args[1].Compute(z) } },
            { "+", new OperatorInformation{Priority=40, ArgumentCount=2, function=(Complex z, IComplexFunction[] args) => args[0].Compute(z)+args[1].Compute(z) } },
            { "-", new OperatorInformation{Priority=40, ArgumentCount=2, function=(Complex z, IComplexFunction[] args) => args[0].Compute(z)-args[1].Compute(z) } },
            { ")", new OperatorInformation{} },
            { "(", new OperatorInformation{IsOpening=true} }
        };


        static Dictionary<string, ConstantInformation> constants = new Dictionary<string, ConstantInformation>()
        {
            { "pi", new ConstantInformation{ complexFunction = new ConstantComplexFunction(Math.PI)} },
            { "e", new ConstantInformation{ complexFunction = new ConstantComplexFunction(Math.E)} },
            { "i", new ConstantInformation{ complexFunction = new ConstantComplexFunction(Complex.ImaginaryOne)} },
            { "_", new ConstantInformation{ complexFunction = new ConstantComplexFunction(-1)} },
            { "x", new ConstantInformation{ complexFunction = new ArgumentComplexFunction() } },
            { "z", new ConstantInformation{ complexFunction = new ArgumentComplexFunction() } }
        };

        class ParseTreeGenerator
        {
            Stack<IComplexFunction> ExpressionStack = new Stack<IComplexFunction>();
            Stack<int> DeepnessStack = new Stack<int>();
            Queue<string> StringExpressionStack = new Queue<string>();

            public bool TryParseExpression(string expression, out IComplexFunction complexFunction, bool mutable = false)
            {
                complexFunction = null;

                if (operators.ContainsKey(expression))
                {
                    if (mutable)
                    {
                        //if (expression == "-" && ExpressionStack.Count == 1)
                        //{
                        //    //= "Could fix the minus...";
                        //}
                        // an unsupported operator
                        if (operators[expression].function == null || ExpressionStack.Count < operators[expression].ArgumentCount)
                            return false;

                        // collect the arguments
                        var arguments = new IComplexFunction[operators[expression].ArgumentCount];

                        // remember about the reverse order!
                        for (int i = operators[expression].ArgumentCount - 1; i >= 0; i--)
                        {
                            arguments[i] = ExpressionStack.Pop();
                        }

                        complexFunction = new ClassicComplexFunction(z => operators[expression].function(z, arguments));
                    }
                }
                else
                {
                    if (constants.ContainsKey(expression))
                    {
                        complexFunction = constants[expression].complexFunction;
                    }
                    else
                    {
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
                    }
                }

                // let's be fair and do not provide anythin undefined if the user just asks for parsability
                if (!mutable)
                    complexFunction = null;

                return true;
            }

            public bool Enqueue(string expression)
            {
                bool succeeded = TryParseExpression(expression, out IComplexFunction complexFunction, true);

                if (succeeded)
                {
                    ExpressionStack.Push(complexFunction);
                    StringExpressionStack.Enqueue(expression);
                }
                else
                {
                    throw new ArgumentException("Could not parse the expression");
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

        public static bool TryParse(string formula, out IComplexFunction complexFunction)
        {
            var expressionQueue = new ParseTreeGenerator();
            var operatorStack = new Stack<string>();
            string lastExpression = string.Empty;
            string expression = string.Empty;
            complexFunction = null;
            bool delayUntilLast = false;
            int deepness = 0;

            // remove all whitespace characters
            formula = Regex.Replace(formula, @"\s+", "");

            try
            {
                for (int cursor = 0; cursor < formula.Length; cursor++)
                {
                    expression += formula[cursor].ToString().ToLower();

                    if (!expressionQueue.TryParseExpression(expression, out _, false))
                        continue;

                    var lastPossibility = cursor == formula.Length - 1 || !expressionQueue.TryParseExpression(expression + formula[cursor + 1], out _, false);

                    // nonoperator should be interpreted lazily
                    // operators should be treated firstly

                    // first successful encounter
                    if (!delayUntilLast)
                    {
                        delayUntilLast = true;
                        // succeeded interpreting the first time// if able to parse, but no further (lazy evaluation)
                        if (operators.ContainsKey(expression))
                        {
                            if (expression == "-")
                            {
                                if (operators.ContainsKey(lastExpression))
                                {
                                    if (operators[lastExpression].IsOpening || lastExpression != ")")
                                    {
                                        // "sin(-" "acos(-" "(-" "*-" "--" "+-" "^-"
                                        // interpret as negation
                                        // carry on to last interpretation]

                                        // consider replacing "-" with "(-1)*" or "i*i*"
                                        switch (expression)
                                        {
                                            case "-":
                                                formula = formula.Remove(cursor, 1).Insert(cursor, "_&");
                                                break;
                                                //case "+":
                                                //    formula = formula.Remove(cursor, 1);
                                                //    break;
                                        }
                                        cursor -= expression.Length;
                                        expression = string.Empty;
                                        continue;
                                    }
                                    else
                                    {
                                        // )-
                                        // interpret as subtraction
                                    }
                                }
                                else
                                {
                                    // a variable
                                    // 1234-
                                    // z-
                                    // could be 123+456-
                                    // interpret as subtraction
                                }
                            }
                        }
                        // expression is not an operator
                        else if (lastExpression != string.Empty && !operators.ContainsKey(lastExpression))
                        {
                            // should not be the case that nonoperator follows another nonoperator (as if it was implicit multiplication)
                            return false;
                        }
                        else
                        {
                            // last time I've interpreted an operator
                            // it is alwright
                            if (!lastPossibility)
                                continue;
                        }

                    }
                    else if (!lastPossibility)
                        continue;
                    // other cases consider subsequent (not first) succsessful interpretation

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
                                expressionQueue.Enqueue(operatorStack.Pop());
                            }

                            // no "(" nor "sin(" found that is an error!
                            if (operatorStack.Count == 0)
                                return false;

                            var openingOperator = operatorStack.Pop();

                            // an opening operator that is not "(" should carry useful information with it, let's keep it!
                            if (openingOperator != "(")
                                expressionQueue.Enqueue(openingOperator);

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
                            // > and >= shoud be differentiated when computing from the right or from the left
                            while (operatorStack.Count > 0 && !operators[operatorStack.Peek()].IsOpening && operators[operatorStack.Peek()].Priority >= operators[expression].Priority)
                            {
                                expressionQueue.Enqueue(operatorStack.Pop());
                            }

                            operatorStack.Push(expression);
                        }
                    }
                    else
                    {
                        // a constant value like "12345", or argument "z"
                        expressionQueue.Enqueue(expression);
                    }

                    lastExpression = expression;
                    // clear the expression before proceeding
                    expression = string.Empty;
                    delayUntilLast = false;
                }

                if (deepness != 0 || expression != string.Empty)
                    return false;

                while (operatorStack.Count > 0)
                {
                    expressionQueue.Enqueue(operatorStack.Pop());
                }

                return expressionQueue.TryParseTree(out complexFunction);
            }
            catch (ArgumentException e)
            {
                // incorrect formula
                return false;
            }
        }
    }

}
