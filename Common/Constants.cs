using System;
using System.Collections.Generic;
using System.Reflection;
using NumberType = System.Double;

namespace IL_Calc.Common
{
    static class Constants
    {
        public static readonly Dictionary<string, NumberType> Mathematical = new Dictionary<string, double>()
        {
            ["pi"] = Math.PI,
            ["e"] = Math.E
        };

        public static readonly Dictionary<string, MethodInfo> Functional = new Dictionary<string, MethodInfo>()
        {
            ["sin"] = typeof(Math).GetMethod(nameof(Math.Sin)),
            ["cos"] = typeof(Math).GetMethod(nameof(Math.Cos)),
            ["tan"] = typeof(Math).GetMethod(nameof(Math.Tan)),
            ["exp"] = typeof(Math).GetMethod(nameof(Math.Exp)),
            ["sqrt"] = typeof(Math).GetMethod(nameof(Math.Sqrt)),
            ["pow"] = typeof(Math).GetMethod(nameof(Math.Pow))
        };
    }
}
