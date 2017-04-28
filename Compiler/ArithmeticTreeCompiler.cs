using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using IL_Calc.Common;
using IL_Calc.Syntax;

namespace IL_Calc.Compiler
{
    static class ArithmeticTreeCompiler
    {
        public static T Compile<T>(SyntaxTree expression) where T : class
        {
            if (!typeof(T).IsSubclassOf(typeof(Delegate)))
                throw new ArgumentException("T have to be a delegate type", nameof(T));

            MethodInfo methodInfo = typeof(T).GetMethod("Invoke");
            ParameterInfo[] methodParameters = methodInfo.GetParameters();
            DynamicMethod method = new DynamicMethod("", methodInfo.ReturnType, Array.ConvertAll(methodParameters, i => i.ParameterType));

            Dictionary<string, int> variables = expression.VariableNames
                .Select((item, index) => new { item, index })
                .ToDictionary(i => i.item, i => i.index);

            ILGenerator il = method.GetILGenerator();
            EmitNode(il, variables, expression.Root);

            if (methodInfo.ReturnType == typeof(int))
                il.Emit(OpCodes.Conv_I4);
            else if (methodInfo.ReturnType != typeof(double))
                throw new ArgumentException($"Return type {methodInfo.ReturnType} isn't supported", nameof(T));

            il.Emit(OpCodes.Ret);

            Delegate del = method.CreateDelegate(typeof(T));
            return del as T;
        }

        public static DynamicMethod Compile(SyntaxTree expression)
        {
            DynamicMethod method = new DynamicMethod("", typeof(double), Array.ConvertAll(expression.VariableNames, i => typeof(double)));

            Dictionary<string, int> variables = expression.VariableNames
                .Select((item, index) => new { item, index })
                .ToDictionary(i => i.item, i => i.index);

            ILGenerator il = method.GetILGenerator();
            EmitNode(il, variables, expression.Root);
            il.Emit(OpCodes.Ret);

            return method;
        }

        static void EmitNode(ILGenerator il, Dictionary<string, int> variables, Node root)
        {
            switch (root)
            {
                case NumberNode node:
                    il.Emit(OpCodes.Ldc_R8, node.Value);
                    break;

                case VariableNode node:
                    il.Emit(OpCodes.Ldarg, variables[node.Name]);
                    break;

                case FunctionNode node:
                    foreach (Node argNode in node.Arguments)
                        EmitNode(il, variables, argNode);
                    il.Emit(OpCodes.Call, Constants.Functional[node.Name]);
                    break;

                case NegationNode node:
                    EmitNode(il, variables, node.Target);
                    il.Emit(OpCodes.Neg);
                    break;

                case BinaryOperationNode node:
                    EmitNode(il, variables, node.Left);
                    EmitNode(il, variables, node.Right);

                    switch (node.Operation)
                    {
                        case BinaryOperation.Add:
                            il.Emit(OpCodes.Add);
                            break;
                        case BinaryOperation.Sub:
                            il.Emit(OpCodes.Sub);
                            break;
                        case BinaryOperation.Mul:
                            il.Emit(OpCodes.Mul);
                            break;
                        case BinaryOperation.Div:
                            il.Emit(OpCodes.Div);
                            break;
                        case BinaryOperation.Pow:
                            il.Emit(OpCodes.Call, Constants.Functional["pow"]);
                            break;
                    }
                    break;
            }

        }
    }
}
