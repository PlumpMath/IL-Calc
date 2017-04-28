using System;
using System.Collections.Generic;
using IL_Calc.Common;

namespace IL_Calc.Lexics
{
    static class LexicalParser
    {
        private static readonly Dictionary<char, TokenType> SingleCharacterTokenTypes =
            new Dictionary<char, TokenType>()
            {
                ['+'] = TokenType.Operator,
                ['-'] = TokenType.Operator,
                ['*'] = TokenType.Operator,
                ['/'] = TokenType.Operator,
                ['^'] = TokenType.Operator,
                ['('] = TokenType.OpeningParenthesis,
                [')'] = TokenType.ClosingParenthesis,
                [','] = TokenType.Comma,
            };

        private static string AcceptOneOrMore(string text, ref int index, Predicate<char> predicate)
        {
            int start = index++;

            while (index < text.Length && predicate(text[index]))
                index++;

            return text.Substring(start, index - start);
        }

        private static string AcceptLiteral(string text, ref int index)
        {
            int start = index++;

            while (index < text.Length && char.IsDigit(text[index]))
                ++index;

            if (index < text.Length && text[index] == '.')
            {
                index++;

                while (index < text.Length && char.IsDigit(text[index]))
                    ++index;
            }

            return text.Substring(start, index - start);
        }

        public static IEnumerable<Token> Parse(string text)
        {
            int index = 0;
            while (index < text.Length)
            {
                int start = index;

                if (char.IsWhiteSpace(text[index]))
                {
                    yield return new Token(TokenType.WhiteSpace, AcceptOneOrMore(text, ref index, char.IsWhiteSpace), start);
                }
                else if (char.IsDigit(text[index]))
                {
                    yield return new Token(TokenType.Literal, AcceptLiteral(text, ref index), start);
                }
                else if (SingleCharacterTokenTypes.TryGetValue(text[index], out TokenType type))
                {
                    yield return new Token(type, text.Substring(index++, 1), start);
                }
                else if (char.IsLetter(text[index]))
                {
                    string identifier = AcceptOneOrMore(text, ref index, char.IsLetterOrDigit);

                    if (Constants.Functional.ContainsKey(identifier))
                        yield return new Token(TokenType.FunctionName, identifier, start);
                    else if (Constants.Mathematical.ContainsKey(identifier))
                        yield return new Token(TokenType.ConstantName, identifier, start);
                    else
                        yield return new Token(TokenType.Variable, identifier, start);
                }
                else
                {
                    throw new LexicalException(index, "Unexpected character: " + text[index]);
                }
            }
        }
    }
}
