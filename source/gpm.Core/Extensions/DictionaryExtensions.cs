using System.Collections.Generic;
using DynamicData.Kernel;

namespace gpm.core.Extensions
{
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Adds or updates a given value in a dictionary by key
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        public static void AddOrUpdate<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
            where TKey : notnull
        {
            if (dictionary.ContainsKey(key))
            {
                dictionary[key] = value;
            }
            else
            {
                dictionary.Add(key, value);
            }
        }

        /// <summary>
        /// Gets a value from a dictionary by key or adds it if it does not exist
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <returns></returns>
        public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
            where TKey : notnull where TValue : new()
        {
            if (!dictionary.ContainsKey(key))
            {
                dictionary.Add(key, new TValue());
            }

            return dictionary[key];
        }

        /// <summary>
        /// Gets an Optional value from a dictionary by key
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <returns></returns>
        public static Optional<TValue> GetOptional<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
            where TKey : notnull =>
            dictionary.ContainsKey(key)
                ? Optional<TValue>.ToOptional(dictionary[key])
                : Optional<TValue>.None;
    }
}
