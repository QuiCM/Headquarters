using System.Collections.Concurrent;

namespace HQ.Interfaces
{
    /// <summary>
    /// Defines an container that allows for storing objects identified by keys
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IConcurrentStorage<T>
    {
        /// <summary>
        /// The backing store for objects stored with this interface
        /// </summary>
        ConcurrentDictionary<T, object> Storage { get; }

        /// <summary>
        /// Gets or sets the value of the key referenced by <paramref name="key"/>
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        dynamic this[T key] { get; set; }

        /// <summary>
        /// Stores an object, making it accessible via <see cref="Retrieve(T)"/>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void Store(T key, object value);

        /// <summary>
        /// Retrieves an object of a given type
        /// </summary>
        /// <returns></returns>
        dynamic Retrieve(T key);

        /// <summary>
        /// Attempts to retrieve an object from a given key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool TryRetrieve(T key, out dynamic value);
    }
}
