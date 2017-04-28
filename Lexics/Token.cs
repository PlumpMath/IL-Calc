namespace IL_Calc.Lexics
{
    class Token
    {
        public TokenType Type { get; }

        public string Value { get; }
        public int SourcePosition { get; }

        public Token(TokenType type, string value, int sourcePosition)
        {
            Type = type;
            Value = value;
            SourcePosition = sourcePosition;
        }

        public override string ToString() => Value;
    }
}