// ReSharper disable CheckNamespace
namespace System.Collections.Generic
{
    /// <summary>
    /// Provides extension methods for collections
    /// </summary>
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
