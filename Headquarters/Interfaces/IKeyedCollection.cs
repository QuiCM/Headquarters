using System.Collections.Concurrent;

namespace HQ.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public interface IKeyedCollection<TKey, TValue>
    {
        /// <summary>
        /// The backing store for objects stored with this interface
        /// </summary>
        ConcurrentDictionary<TKey, TValue> Storage { get; }
        
        /// <summary>
        /// Gets or sets the value of a key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        TValue this[TKey key] { get; set; }

        /// <summary>
        /// Stores a value, tying it to the given key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void Store(TKey key, TValue value);

        /// <summary>
        /// Retrieves a value from a given key
        /// </summary>
        /// <returns></returns>
        TValue Retrieve(TKey key);

        /// <summary>
        /// Attempts to retrieve a value from a given key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool TryRetrieve(TKey key, out TValue value);

        /// <summary>
        /// Clears the collection
        /// </summary>
        void Clear();
    }
}
