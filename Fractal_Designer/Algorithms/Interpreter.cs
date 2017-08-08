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
        struct OperatorInformation
        {
            public bool IsOpening;
            public byte Priority;
            public int ArgumentCount;
        }
        //struct BracketInformation
        //{
        //    public int ArgumentCount;
        //    public Func<Complex, IComplexFunction[], Complex> function;
        //}
        struct ConstantInformation
        {
            //public ComplexFunction complexFunction;
        }

        static Dictionary<string, OperatorInformation> operators = new Dictionary<string, OperatorInformation>()
        {
            //{ "abs)(", new OperatorInformation{ArgumentCount=1, IsOpening=true} },
            { "acos)(", new OperatorInformation{ArgumentCount=1, IsOpening=true} },
            { "asin(", new OperatorInformation{ArgumentCount=1, IsOpening=true} },
            { "atan(", new OperatorInformation{ArgumentCount=1, IsOpening=true} },
            //{ "conjugate(", new OperatorInformation{ArgumentCount=1, IsOpening=true} },
            //{ "phase(", new OperatorInformation{ArgumentCount=1, IsOpening=true} },
            //{ "real(", new OperatorInformation{ArgumentCount=1, IsOpening=true} },
            // unfortunately there is so many bugs and unfinished parsing that the first letter should be unique to separately operators and constants
            //{ "[imag](", new OperatorInformation{ArgumentCount=1, IsOpening=true} },
            //, function=(Complex z, ComplexFunction[] args) => Complex.FromPolarCoordinates((double)args[0].Compute(z).Magnitude, (double)args[1].Compute(z).Magnitude) 
            //{ "polar(", new OperatorInformation{ArgumentCount=2, IsOpening=true} },
            { "cos(", new OperatorInformation{ArgumentCount=1, IsOpening=true} },
            { "cosh(", new OperatorInformation{ArgumentCount=1, IsOpening=true} },
            { "[exp](", new OperatorInformation{ArgumentCount=1, IsOpening=true} },
            { "log(", new OperatorInformation{ArgumentCount=1, IsOpening=true} },
            { "log10(", new OperatorInformation{ArgumentCount=1, IsOpening=true} },
            { "sin(", new OperatorInformation{ArgumentCount=1, IsOpening=true} },
            { "sinh(", new OperatorInformation{ArgumentCount=1, IsOpening=true} },
            { "sqrt(", new OperatorInformation{ArgumentCount=1, IsOpening=true} },
            { "tan(", new OperatorInformation{ArgumentCount=1, IsOpening=true} },
            { "tanh(", new OperatorInformation{ArgumentCount=1, IsOpening=true} },
            // important multiplication
            { "&", new OperatorInformation{Priority=90, ArgumentCount=2 } },
            // how to process only the righthandside argument??
            //{ "'", new OperatorInformation{Priority=90, ArgumentCount=1, RightmostEvaluation = true, function=(Complex z, IComplexFunction[] args) => Complex.Conjugate(args[0].Compute(z)) } },
            { "^", new OperatorInformation{Priority=80, ArgumentCount=2} },
            { "*", new OperatorInformation{Priority=60, ArgumentCount=2} },
            { "/", new OperatorInformation{Priority=60, ArgumentCount=2} },
            { "+", new OperatorInformation{Priority=40, ArgumentCount=2} },
            { "-", new OperatorInformation{Priority=40, ArgumentCount=2} },
            { ")", new OperatorInformation{} },
            { "(", new OperatorInformation{IsOpening=true} },
        };

        static Dictionary<string, ConstantInformation> constants = new Dictionary<string, ConstantInformation>()
        {
            { "pi", new ConstantInformation{} },
            { "e", new ConstantInformation{} },
            { "i", new ConstantInformation{} },
            { "_", new ConstantInformation{} },
            { "x", new ConstantInformation{} },
            { "z", new ConstantInformation{} },
            //{ "n", new ConstantInformation{ complexFunction = new RandomComplexFunction() } },
        };

        class ParseTreeGenerator
        {
            Stack<ComplexFunction.ComplexFunction> ExpressionStack = new Stack<ComplexFunction.ComplexFunction>();
            Stack<int> DeepnessStack = new Stack<int>();
            Queue<string> StringExpressionStack = new Queue<string>();

            public bool TryParseExpression(string expression, out ComplexFunction.ComplexFunction complexFunction, bool mutable = false)
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
                        if (/*operators[expression].function == null || */ExpressionStack.Count < operators[expression].ArgumentCount)
                            return false;

                        // collect the arguments
                        var arguments = new ComplexFunction.ComplexFunction[operators[expression].ArgumentCount];

                        // remember about the reverse order!
                        for (int i = operators[expression].ArgumentCount - 1; i >= 0; i--)
                        {
                            arguments[i] = ExpressionStack.Pop();
                        }

                        complexFunction = ComplexFunction.Generator.Generate(expression, arguments);
                    }
                }
                else
                {
                    if (constants.ContainsKey(expression))
                    {
                        complexFunction = ComplexFunction.Generator.Generate(expression);
                    }
                    else
                    {
                        // could be a number
                        bool succeededParsing = double.TryParse(expression, out double result);

                        if (succeededParsing)
                        {
                            complexFunction = ComplexFunction.Generator.Generate("custom_constant", result);
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
                bool succeeded = TryParseExpression(expression, out ComplexFunction.ComplexFunction complexFunction, true);

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

            public bool TryParseTree(out ComplexFunction.ComplexFunction returnValue)
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

        public static bool TryParse(string formula, out ComplexFunction.ComplexFunction complexFunction)
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
                                                // important negation
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
                            // rightmost evaluation works ONLY for supported operators and ONLY if the are neighbouring to each other
                            // leftmost evaluation pops more often
                            while (operatorStack.Count > 0 &&
                                !operators[operatorStack.Peek()].IsOpening &&
                                operators[operatorStack.Peek()].Priority >= operators[expression].Priority)
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
