using System.Collections.Generic;
using NumberType = System.Double;

namespace IL_Calc.Syntax
{
    abstract class Node { }

    class SyntaxTree
    {
        public Node Root { get; private set; }
        public string[] VariableNames { get; private set; }

        public SyntaxTree(Node root, string[] variableNames)
        {
            Root = root;
            VariableNames = variableNames;
        }
    }

    class NumberNode : Node
    {
        public NumberType Value { get; }

        public NumberNode(NumberType value)
        {
            Value = value;
        }

        public override string ToString() => Value.ToString();
    }

    class VariableNode : Node
    {
        public string Name { get; }

        public VariableNode(string name)
        {
            Name = name;
        }

        public override string ToString() => Name;
    }

    class FunctionNode : Node
    {
        public string Name { get; }
        public IReadOnlyList<Node> Arguments { get; }

        public FunctionNode(string name, IReadOnlyList<Node> arguments)
        {
            Name = name;
            Arguments = arguments;
        }

        public override string ToString() => $"{Name}({string.Join(", ", Arguments)})";
    }

    class BinaryOperationNode : Node
    {
        public Node Left { get; }
        public Node Right { get; }

        public BinaryOperation Operation { get; }

        public BinaryOperationNode(Node left, Node right, BinaryOperation operation)
        {
            Left = left;
            Right = right;
            Operation = operation;
        }

        public override string ToString() => $"({Left} {Operation} {Right})";
    }

    class NegationNode : Node
    {
        public Node Target { get; }

        public NegationNode(Node target)
        {
            Target = target;
        }

        public override string ToString() => $"(-{Target})";
    }
}
