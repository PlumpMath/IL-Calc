using System;

namespace IL_Calc.Lexics
{
    class LexicalException : Exception
    {
        public int Position { get; }

        public LexicalException(int position, string message) : base(message)
        {
            Position = position;
        }
    }
}
