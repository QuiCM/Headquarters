using HQ.Interfaces;
using System.Collections.Concurrent;

namespace HQ.Collections
{
    /// <summary>
    /// Default implementation of <see cref="IKeyedCollection{TKey, TValue}"/>
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class DefaultKeyedCollection<TKey, TValue> : IKeyedCollection<TKey, TValue>
    {
        /// <summary>
        /// Default implementation of <see cref="IKeyedCollection{TKey, TValue}.this[TKey]"/>
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public TValue this[TKey key] { get => Retrieve(key); set => Store(key, value); }

        /// <summary>
        /// Default implementation of <see cref="IKeyedCollection{TKey, TValue}.Storage"/>
        /// </summary>
        public ConcurrentDictionary<TKey, TValue> Storage { get; } = new ConcurrentDictionary<TKey, TValue>();

        /// <summary>
        /// Default implementation of <see cref="IKeyedCollection{TKey, TValue}.Clear()"/>
        /// </summary>
        public void Clear()
        {
            Storage.Clear();
        }

        /// <summary>
        /// Default implementation of <see cref="IKeyedCollection{TKey, TValue}.Retrieve(TKey)"/>
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public TValue Retrieve(TKey key)
        {
            if (Storage.TryGetValue(key, out TValue value))
            {
                return value;
            }

            return default(TValue);
        }

        /// <summary>
        /// Default implementation of <see cref="IKeyedCollection{TKey, TValue}.Store(TKey, TValue)"/>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Store(TKey key, TValue value)
        {
            //Replace the old object with the new if an old object exists, or add a new object
            Storage.AddOrUpdate(key, value, (oldKey, oldValue) => value);
        }

        /// <summary>
        /// Default implementation of <see cref="IKeyedCollection{TKey, TValue}.TryRetrieve(TKey, out TValue)"/>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryRetrieve(TKey key, out TValue value)
        {
            if (Storage.TryGetValue(key, out value))
            {
                return true;
            }

            return false;
        }
    }
}
