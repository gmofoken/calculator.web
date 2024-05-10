using calculator.web.Services.Interface;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace calculator.web.Services.Implementation
{
    public class Calculator : ICalculator
    {
        #region Fields

        #region Markers (each marker should have length equals to 1)

        private const string NumberMaker = "#";
        private const string OperatorMarker = "$";
        private const string FunctionMarker = "@";

        #endregion Markers (each marker should have length equals to 1)

        #region Internal tokens

        private const string Plus = OperatorMarker + "+";
        private const string Minus = OperatorMarker + "-";
        private const string Multiply = OperatorMarker + "*";
        private const string Divide = OperatorMarker + "/";
        private const string Degree = OperatorMarker + "^";
        private const string LeftParent = OperatorMarker + "(";
        private const string RightParent = OperatorMarker + ")";

        #endregion Internal tokens

        #region Dictionaries (containts supported input tokens, exclude number)

        private readonly Dictionary<string, string> supportedOperators =
            new Dictionary<string, string>
            {
                { "+", Plus },
                { "-", Minus },
                { "*", Multiply },
                { "/", Divide },
                { "(", LeftParent },
                { ")", RightParent }
            };

        /// <summary> TO DO
        private readonly Dictionary<string, string> supportedFunctions =
            new Dictionary<string, string>
            {
            };

        private readonly Dictionary<string, string> supportedConstants =
            new Dictionary<string, string>
            {
            };

        #endregion Dictionaries (containts supported input tokens, exclude number)

        #endregion Fields

        private readonly char decimalSeparator;
        private bool isRadians;

        #region Constructors

        /// <summary>
        /// Initialize new instance of MathParser (symbol of decimal separator is read from regional
        public Calculator()
        {
            try
            {
                decimalSeparator = Char.Parse(System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
            }
            catch (FormatException ex)
            {
                throw new FormatException("Error: can't read char decimal separator from system, check your regional settings.", ex);
            }
        }

        /// <summary>
        /// Initialize new instance of MathParser
        public Calculator(char decimalSeparator)
        {
            this.decimalSeparator = decimalSeparator;
        }

        #endregion Constructors

        /// <summary> Produce result of the given math expression
        public double Parse(string expression, bool isRadians = true)
        {
            this.isRadians = isRadians;

            try
            {
                return Calculate(ConvertToRPN(FormatString(expression)));
            }
            catch (DivideByZeroException e)
            {
                throw e;
            }
            catch (FormatException e)
            {
                throw e;
            }
            catch (InvalidOperationException e)
            {
                throw e;
            }
            catch (ArgumentOutOfRangeException e)
            {
                throw e;
            }
            catch (ArgumentException e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary> Produce formatted string by the given string
        private string FormatString(string expression)
        {
            if (string.IsNullOrEmpty(expression))
            {
                throw new ArgumentNullException("Expression is null or empty");
            }

            StringBuilder formattedString = new StringBuilder();
            int balanceOfParenth = 0; // Check number of parenthesis

            // Format string in one iteration and check number of parenthesis 
            for (int i = 0; i < expression.Length; i++)
            {
                char ch = expression[i];

                if (ch == '(')
                {
                    balanceOfParenth++;
                }
                else if (ch == ')')
                {
                    balanceOfParenth--;
                }

                if (Char.IsWhiteSpace(ch))
                {
                    continue;
                }
                else if (Char.IsUpper(ch))
                {
                    formattedString.Append(Char.ToLower(ch));
                }
                else
                {
                    formattedString.Append(ch);
                }
            }

            if (balanceOfParenth != 0)
            {
                throw new FormatException("Number of left and right parenthesis is not equal");
            }

            return formattedString.ToString();
        }

        #region Convert to Reverse-Polish Notation

        /// <summary> Produce math expression in reverse polish notation by the given string
        private string ConvertToRPN(string expression)
        {
            int pos = 0; // Current position of lexical analysis
            StringBuilder outputString = new StringBuilder();
            Stack<string> stack = new Stack<string>();

            // While there is unhandled char in expression
            while (pos < expression.Length)
            {
                string token = LexicalAnalysisInfixNotation(expression, ref pos);

                outputString = SyntaxAnalysisInfixNotation(token, outputString, stack);
            }

            // Pop all elements from stack to output string
            while (stack.Count > 0)
            {
                // There should be only operators
                if (stack.Peek()[0] == OperatorMarker[0])
                {
                    outputString.Append(stack.Pop());
                }
                else
                {
                    throw new FormatException("Format exception,"
                    + " there is function without parenthesis");
                }
            }

            return outputString.ToString();
        }

        /// <summary> Produce token by the given math expression
        private string LexicalAnalysisInfixNotation(string expression, ref int pos)
        {
            // Receive first char
            StringBuilder token = new StringBuilder();
            token.Append(expression[pos]);

            // If it is a operator
            if (supportedOperators.ContainsKey(token.ToString()))
            {
                // Determine it is unary or binary operator
                bool isUnary = pos == 0 || expression[pos - 1] == '(';
                pos++;

                switch (token.ToString())
                {
                    case "+":
                        return  Plus;

                    case "-":
                        return  Minus;

                    default:
                        return supportedOperators[token.ToString()];
                }
            }
            else if (Char.IsDigit(token[0]) || token[0] == decimalSeparator)
            {
                // Read number

                // Read the whole part of number
                if (Char.IsDigit(token[0]))
                {
                    while (++pos < expression.Length
                    && Char.IsDigit(expression[pos]))
                    {
                        token.Append(expression[pos]);
                    }
                }
                else
                {
                    // Because system decimal separator will be added below
                    token.Clear();
                }

                // Read the fractional part of number
                if (pos < expression.Length
                    && expression[pos] == decimalSeparator)
                {
                    // Add current system specific decimal separator
                    token.Append(CultureInfo.CurrentCulture
                        .NumberFormat.NumberDecimalSeparator);

                    while (++pos < expression.Length
                    && Char.IsDigit(expression[pos]))
                    {
                        token.Append(expression[pos]);
                    }
                }

                return NumberMaker + token.ToString();
            }
            else
            {
                throw new ArgumentException("Unknown token in expression");
            }
        }

        /// <summary> Syntax analysis of infix notation)</returns>
        private StringBuilder SyntaxAnalysisInfixNotation(string token, StringBuilder outputString, Stack<string> stack)
        {
            // If it's a number just put to string
            if (token[0] == NumberMaker[0])
            {
                outputString.Append(token);
            }
            else if (token == LeftParent)
            {
                // If its '(' push to stack
                stack.Push(token);
            }
            else if (token == RightParent)
            {
                // If its ')' pop elements from stack to output string until find the ')'

                string elem;
                while ((elem = stack.Pop()) != LeftParent)
                {
                    outputString.Append(elem);
                }

                // if after this a function is in the peek of stack then put it to string
                if (stack.Count > 0 &&
                    stack.Peek()[0] == FunctionMarker[0])
                {
                    outputString.Append(stack.Pop());
                }
            }
            else
            {
                // While priority of elements at peek of stack >= (>) token's priority put these
                // elements to output string
                while (stack.Count > 0 &&
                    Priority(token, stack.Peek()))
                {
                    outputString.Append(stack.Pop());
                }

                stack.Push(token);
            }

            return outputString;
        }

        /// <summary> Is priority of token less (or equal) to priority of p
        private bool Priority(string token, string p)
        {
            return IsRightAssociated(token) ?
                GetPriority(token) < GetPriority(p) :
                GetPriority(token) <= GetPriority(p);
        }

        /// <summary> Is right associated operator
        private bool IsRightAssociated(string token)
        {
            return token == Degree;
        }

        /// <summary> Get priority of operator
        private int GetPriority(string token)
        {
            switch (token)
            {
                case LeftParent:
                    return 0;

                case Plus:
                case Minus:
                    return 2;

                case Multiply:
                case Divide:
                    return 4;
                    return 10;

                default:
                    throw new ArgumentException("Unknown operator");
            }
        }

        #endregion Convert to Reverse-Polish Notation

        #region Calculate expression in RPN

        /// <summary> Calculate expression in reverse-polish notation
        private double Calculate(string expression)
        {
            int pos = 0; // Current position of lexical analysis
            var stack = new Stack<double>(); // Contains operands

            // Analyse entire expression
            while (pos < expression.Length)
            {
                string token = LexicalAnalysisRPN(expression, ref pos);

                stack = SyntaxAnalysisRPN(stack, token);
            }

            // At end of analysis in stack should be only one operand (result)
            if (stack.Count > 1)
            {
                throw new ArgumentException("Excess operand");
            }

            return stack.Pop();
        }

        /// <summary> Produce token by the given math expression
        private string LexicalAnalysisRPN(string expression, ref int pos)
        {
            StringBuilder token = new StringBuilder();

            // Read token from marker to next marker

            token.Append(expression[pos++]);

            while (pos < expression.Length && expression[pos] != NumberMaker[0]
                && expression[pos] != OperatorMarker[0]
                && expression[pos] != FunctionMarker[0])
            {
                token.Append(expression[pos++]);
            }

            return token.ToString();
        }

        /// <summary> Syntax analysis of reverse-polish notation
        private Stack<double> SyntaxAnalysisRPN(Stack<double> stack, string token)
        {
            // if it's operand then just push it to stack
            if (token[0] == NumberMaker[0])
            {
                stack.Push(double.Parse(token.Remove(0, 1)));
            }
            else
            {
                double arg2 = stack.Pop();
                double arg1 = stack.Pop();

                double rst;

                switch (token)
                {
                    case Plus:
                        rst = arg1 + arg2;
                        break;

                    case Minus:
                        rst = arg1 - arg2;
                        break;

                    case Multiply:
                        rst = arg1 * arg2;
                        break;

                    case Divide:
                        if (arg2 == 0)
                        {
                            throw new DivideByZeroException("Second argument is zero");
                        }
                        rst = arg1 / arg2;
                        break;

                    default:
                        throw new ArgumentException("Unknown operator");
                }

                stack.Push(rst);
            }

            return stack;
        }

        /// <summary> Apply trigonometric function
        private double ApplyTrigFunction(Func<double, double> func, double arg)
        {
            if (!isRadians)
            {
                arg = arg * Math.PI / 180; // Convert value to degree
            }

            return func(arg);
        }

        /// <summary> Produce number of arguments for the given operator
        private int NumberOfArguments(string token)
        {
            switch (token)
            {
                case Plus:
                case Minus:
                case Multiply:
                case Divide:

                default:
                    throw new ArgumentException("Unknown operator");
            }
        }

        #endregion Calculate expression in RPN
    }
}