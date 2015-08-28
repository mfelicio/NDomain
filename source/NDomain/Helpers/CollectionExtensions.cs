using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Collections.Generic
{
    public static class CollectionExtensions
    {
        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> source, TKey key, Func<TKey, TValue> createValue)
        {
            TValue value;
            if (!source.TryGetValue(key, out value))
            {
                source[key] = value = createValue(key);
            }

            return value;
        }

        public static TValue GetOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> source, TKey key)
        {
            TValue value;
            if (source.TryGetValue(key, out value))
            {
                return value;
            }

            return default(TValue);
        }
    }
}
