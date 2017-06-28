using System;
using System.Collections.Concurrent;

namespace HQ.Interfaces
{
    /// <summary>
    /// An object that describes the context of a command
    /// </summary>
    public interface IContextObject
    {
        /// <summary>
        /// A reference to the CommandRegistry relevant to this context
        /// </summary>
        CommandRegistry Registry { get; }
        /// <summary>
        /// Objects stored via a Type using <see cref="Store{T}(object)"/>
        /// </summary>
        ConcurrentDictionary<Type, object> TypedStorage { get; }
        /// <summary>
        /// Objects stored via a name using <see cref="Store(string, object)"/>
        /// </summary>
        ConcurrentDictionary<string, object> NamedStorage { get; }

        /// <summary>
        /// Stores an object, making it accessible via <see cref="Retrieve{T}()"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        void Store<T>(object obj);

        /// <summary>
        /// Stores an object using a named identifier, making it accessible via <see cref="Retrieve{T}(string)"/>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="obj"></param>
        void Store(string name, object obj);

        /// <summary>
        /// Retrieves an object of a given type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T Retrieve<T>();

        /// <summary>
        /// Retrieves an object with a given name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="identifier"></param>
        /// <returns></returns>
        T Retrieve<T>(string identifier);

        /// <summary>
        /// Attempts to retrieve an object of a given type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        bool TryRetrieve<T>(out T value);

        /// <summary>
        /// Attempts to retrieve an object with a given name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="identifier"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool TryRetrieve<T>(string identifier, out T value);
    }
}
