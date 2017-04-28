using System;
using IL_Calc.Lexics;

namespace IL_Calc.Syntax
{
    class SyntacticException : Exception
    {
        public Token Token { get; }

        public SyntacticException(Token token = null, string message = null) : base(message)
        {
            Token = token;
        }
    }
}