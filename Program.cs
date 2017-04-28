using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using IL_Calc.Compiler;
using IL_Calc.Lexics;
using IL_Calc.Syntax;

namespace IL_Calc
{
    using NumberType = Double;

    class Program
    {
        static void PrintHelp()
        {
            Console.WriteLine("Type \"list\" to view available variables\n" +
                              "Type `expression` to calculate it\"\n" +
                              "Type `variable` = `expression` to assign expression result to a variable");
        }

        static void PrintLexics(IEnumerable<Token> lexics)
        {
            foreach (Token token in lexics)
            {
                switch (token.Type)
                {
                    case TokenType.Literal:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;
                    case TokenType.Variable:
                    case TokenType.ConstantName:
                    case TokenType.FunctionName:
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    case TokenType.OpeningParenthesis:
                    case TokenType.ClosingParenthesis:
                    case TokenType.Comma:
                        Console.ForegroundColor = ConsoleColor.Green;
                        break;
                    default:
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                }
                Console.Write(token);
            }
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        static double ExecuteExpression(string expression, Dictionary<string, NumberType> variables, out IList<Token> lexics)
        {
            lexics = LexicalParser.Parse(expression).ToList();
            SyntaxTree syntaxTree = SyntaxParser.Parse(lexics);
            DynamicMethod method = ArithmeticTreeCompiler.Compile(syntaxTree);

            var undefinedVariables = syntaxTree.VariableNames.Except(variables.Keys).ToList();

            if (undefinedVariables.Count != 0)
                throw new ArgumentException($"Undefined variables found: {string.Join(", ", undefinedVariables)}");

            object[] args =
                syntaxTree.VariableNames
                    .OrderBy(i => i)
                    .Select(i => variables[i])
                    .Cast<object>()
                    .ToArray();

            return (double)method.Invoke(null, args);
        }

        static void Main()
        {
            Console.InputEncoding = Console.OutputEncoding = Encoding.Unicode;
            Console.ForegroundColor = ConsoleColor.White;

            PrintHelp();

            var variables = new Dictionary<string, NumberType>();

            string expression;
            while ((expression = Console.ReadLine()) != null)
            {
                if (expression == "list")
                {
                    foreach (var variable in variables.OrderBy(i => i.Key))
                        Console.WriteLine("{0}: {1}", variable.Key, variable.Value);
                }
                else
                {
                    string assigningVariable = null;
                    var match = Regex.Match(expression, @"^\s*(\w+)\s*=\s*(.*)");
                    if (match.Success)
                    {
                        assigningVariable = match.Groups[1].Value;
                        expression = match.Groups[2].Value;
                    }

                    try
                    {
                        var result = ExecuteExpression(expression, variables, out IList<Token> lexics);

                        if (assigningVariable != null)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.Write($"{assigningVariable}: ");

                            variables[assigningVariable] = result;
                        }

                        PrintLexics(lexics);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($" = {result}");
                    }
                    catch (SyntacticException error)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Syntax error! <{error.Token}> {error.Message}");
                    }
                    catch (LexicalException error)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Lexical error at char #{error.Position}! {error.Message}");
                    }
                    catch (ArgumentException error)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(error.Message);
                    }
                }

                Console.ForegroundColor = ConsoleColor.White;
            }
        }
    }
}
