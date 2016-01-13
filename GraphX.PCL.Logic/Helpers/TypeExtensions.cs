using System;
using System.Collections.Generic;
using QuickGraph;

namespace GraphX.PCL.Logic.Helpers
{
    public static class TypeExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> list, Action<T> func)
        {
            foreach (var item in list)
                func(item);
        }
    }
}
