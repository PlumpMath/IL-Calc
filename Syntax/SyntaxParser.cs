using System;
using System.Collections.Generic;
using System.Linq;
using IL_Calc.Common;
using IL_Calc.Lexics;
using NumberType = System.Double;

namespace IL_Calc.Syntax
{
    static class SyntaxParser
    {
        private static Node ParseGroup(IEnumerator<Token> lexics)
        {
            // eliminating subgroups
            var expressionNodes = new LinkedList<object>();
            do
            {
                Token token = lexics.Current;
                switch (token.Type)
                {
                    case TokenType.Literal:
                        if (NumberType.TryParse(token.Value, out NumberType value))
                            expressionNodes.AddLast(new NumberNode(value));
                        else
                            throw new SyntacticException(token, "Incorrect literal " + token);
                        break;

                    case TokenType.ConstantName:
                        expressionNodes.AddLast(new NumberNode(Constants.Mathematical[token.Value]));
                        break;

                    case TokenType.Variable:
                        expressionNodes.AddLast(new VariableNode(token.Value));
                        break;

                    case TokenType.OpeningParenthesis:

                        if (!lexics.MoveNext())
                            throw new SyntacticException(token, "Expression expected");

                        expressionNodes.AddLast(ParseGroup(lexics));

                        if (lexics.Current == null || lexics.Current.Type != TokenType.ClosingParenthesis)
                            throw new SyntacticException(token, "Closing paranthesis expected");

                        break;

                    case TokenType.FunctionName:

                        if (!lexics.MoveNext() || lexics.Current.Type != TokenType.OpeningParenthesis)
                            throw new SyntacticException(token, "Argument list expected");

                        Token lastTokenBeforeArgument = lexics.Current;
                        var arguments = new List<Node>();
                        while (lexics.Current.Type == TokenType.OpeningParenthesis ||
                               lexics.Current.Type == TokenType.Comma)
                        {
                            if (!lexics.MoveNext())
                                throw new SyntacticException(lastTokenBeforeArgument, "Argument expected");

                            arguments.Add(ParseGroup(lexics));
                            lastTokenBeforeArgument = lexics.Current;
                        }
                        if (lexics.Current == null || lexics.Current.Type != TokenType.ClosingParenthesis)
                            throw new SyntacticException(token, "Closing paranthesis expected");

                        int expectedParamsCount = Constants.Functional[token.Value].GetParameters().Length;
                        if (arguments.Count != expectedParamsCount)
                            throw new SyntacticException(token, $"{expectedParamsCount} parameters expected, got {arguments.Count}");

                        expressionNodes.AddLast(new FunctionNode(token.Value, arguments));
                        break;

                    case TokenType.Operator:
                        expressionNodes.AddLast(token);
                        break;

                    default:
                        throw new SyntacticException(token, "Unexpected token");
                }
            } while (lexics.MoveNext() &&
                     lexics.Current.Type != TokenType.ClosingParenthesis &&
                     lexics.Current.Type != TokenType.Comma);

            // eliminate power operations
            for (var i = expressionNodes.First; i != null; i = i.Next)
            {
                Token current;
                if ((current = i.Value as Token) != null && current.Value == "^")
                {
                    bool rightSignIsNegative = false;
                    Node left, right;
                    if (i.Previous == null || (left = i.Previous.Value as Node) == null)
                        throw new SyntacticException(current, "Left expression expected");

                    while (true)
                    {
                        var j = i.Next;
                        Token signToken;
                        if ((signToken = j.Value as Token) != null)
                        {
                            if (signToken.Value == "+")
                            {
                                expressionNodes.Remove(j);
                                continue;
                            }
                            if (signToken.Value == "-")
                            {
                                rightSignIsNegative = !rightSignIsNegative;
                                expressionNodes.Remove(j);
                                continue;
                            }
                        }
                        else if ((right = j.Value as Node) != null)
                        {
                            expressionNodes.Remove(j);
                            break;
                        }
                        throw new SyntacticException(current, "Right expression expected");
                    }

                    if (rightSignIsNegative)
                        right = new NegationNode(right);

                    var newI = i.Previous;
                    newI.Value = new BinaryOperationNode(left, right, BinaryOperation.Pow);
                    expressionNodes.Remove(i);
                    i = newI;
                }
            }

            // eliminate multiplication groups (like "2x" = "2*x" or "1 x 2" = "1*x*2")
            for (var i = expressionNodes.First; i != null; i = i.Next)
            {
                Node current, next;
                if ((current = i.Value as Node) != null)
                {
                    var j = i.Next;
                    while (j != null && (next = j.Value as Node) != null)
                    {
                        current = new BinaryOperationNode(current, next, BinaryOperation.Mul);
                        var nextJ = j.Next;
                        expressionNodes.Remove(j);
                        j = nextJ;
                    }
                    i.Value = current;
                }
            }

            // eliminate multiplication operations
            for (var i = expressionNodes.First; i != null; i = i.Next)
            {
                Token current;
                if ((current = i.Value as Token) != null && (current.Value == "*" || current.Value == "/"))
                {
                    bool rightSignIsNegative = false;
                    Node left, right;
                    if (i.Previous == null || (left = i.Previous.Value as Node) == null)
                        throw new SyntacticException(current, "Left expression expected");

                    while (true)
                    {
                        var j = i.Next;
                        Token signToken;
                        if ((signToken = j.Value as Token) != null)
                        {
                            if (signToken.Value == "+")
                            {
                                expressionNodes.Remove(j);
                                continue;
                            }
                            if (signToken.Value == "-")
                            {
                                rightSignIsNegative = !rightSignIsNegative;
                                expressionNodes.Remove(j);
                                continue;
                            }
                        }
                        else if ((right = j.Value as Node) != null)
                        {
                            expressionNodes.Remove(j);
                            break;
                        }
                        throw new SyntacticException(current, "Right expression expected");
                    }

                    if (rightSignIsNegative)
                        right = new NegationNode(right);

                    var newI = i.Previous;
                    newI.Value = new BinaryOperationNode(left, right,
                        current.Value == "*" ? BinaryOperation.Mul : BinaryOperation.Div);
                    expressionNodes.Remove(i);
                    i = newI;
                }
            }

            // eliminate additive operations
            int sign = 0;
            for (var i = expressionNodes.First; i != null;)
            {
                Token token;
                if ((token = i.Value as Token) != null)
                {
                    if (sign == 0)
                        sign = 1;
                    if (token.Value == "-")
                        sign = -sign;
                    var newI = i.Next;
                    expressionNodes.Remove(i);
                    i = newI;
                }
                else
                {
                    Node right = (Node)i.Value;

                    if (i == expressionNodes.First)
                    {
                        i.Value = sign == -1 ? new NegationNode(right) : right;
                        i = i.Next;
                    }
                    else
                    {
                        expressionNodes.First.Value = new BinaryOperationNode((Node)expressionNodes.First.Value, right,
                            sign == -1 ? BinaryOperation.Sub : BinaryOperation.Add);
                        var newI = i.Next;
                        expressionNodes.Remove(i);
                        i = newI;
                    }

                    sign = 0;
                }
            }
            if (sign != 0)
                throw new SyntacticException(message: "Expression is expected in the end of text");

            if (expressionNodes.Count != 1)
                throw new NotImplementedException();

            return (Node)expressionNodes.First.Value;
        }

        public static SyntaxTree Parse(IEnumerable<Token> lexics)
        {
            // remove all whitespaces from equation
            lexics = lexics.Where(i => i.Type != TokenType.WhiteSpace).ToList();

            var e = lexics.GetEnumerator();
            if (!e.MoveNext())
                throw new SyntacticException(message: "Expression expected");

            string[] variableNames =
                lexics.Where(i => i.Type == TokenType.Variable)
                    .Select(i => i.Value)
                    .Distinct()
                    .OrderBy(i => i)
                    .ToArray();

            return new SyntaxTree(ParseGroup(e), variableNames);
        }
    }
}
