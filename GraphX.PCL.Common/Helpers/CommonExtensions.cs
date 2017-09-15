using System;
using System.Collections.Generic;

namespace GraphX.PCL.Common
{
    public static class CommonExtensions
    {
        public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary.ContainsKey(key))
                dictionary[key] = value;
            else dictionary.Add(key, value);
        }

        public static void ForEach<T>(this IEnumerable<T> list, Action<T> func)
        {
            foreach (var item in list)
                func(item);
        }
    }
}
