using HQ.Interfaces;
using System;
using System.Collections.Concurrent;

namespace HQ
{
    /// <inheritdoc/>
    /// <summary>
    /// See <see cref="IContextObject"/>
    /// </summary>
    public class ContextObject : IContextObject
    {
        /// <inheritdoc/>
        /// <summary>
        /// See <see cref="IContextObject.TypedStorage"/>
        /// </summary>
        public ConcurrentDictionary<Type, object> TypedStorage => new ConcurrentDictionary<Type, object>();

        /// <inheritdoc/>
        /// <summary>
        /// See <see cref="IContextObject.NamedStorage"/>
        /// </summary>
        public ConcurrentDictionary<string, object> NamedStorage => new ConcurrentDictionary<string, object>();

        /// <inheritdoc/>
        /// <summary>
        /// See <see cref="IContextObject.Registry"/>
        /// </summary>
        public CommandRegistry Registry { get; }

        /// <summary>
        /// Constructs a new context object using the given command registry
        /// </summary>
        /// <param name="registry"></param>
        public ContextObject(CommandRegistry registry)
        {
            Registry = registry;
        }

        /// <inheritdoc/>
        /// <summary>
        /// See <see cref="IContextObject.Retrieve{T}()"/>
        /// </summary>
        public T Retrieve<T>()
        {
            if (TypedStorage.TryGetValue(typeof(T), out object value))
            {
                return (T)value;
            }

            return default(T);
        }

        /// <inheritdoc/>
        /// <summary>
        /// See <see cref="IContextObject.Retrieve{T}(string)"/>
        /// </summary>
        public T Retrieve<T>(string identifier)
        {
            if (NamedStorage.TryGetValue(identifier, out object value))
            {
                return (T)value;
            }

            return default(T);
        }

        /// <inheritdoc/>
        /// <summary>
        /// See <see cref="IContextObject.TryRetrieve{T}(out T)"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryRetrieve<T>(out T value)
        {
            if( TypedStorage.TryGetValue(typeof(T), out object val))
            {
                value = (T)val;
                return true;
            }

            value = default(T);
            return false;
        }

        /// <inheritdoc/>
        /// <summary>
        /// See <see cref="IContextObject.TryRetrieve{T}(string, out T)"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="identifier"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryRetrieve<T>(string identifier, out T value)
        {
            if (NamedStorage.TryGetValue(identifier, out object val))
            {
                value = (T)val;
                return true;
            }

            value = default(T);
            return false;
        }

        /// <inheritdoc/>
        /// <summary>
        /// See <see cref="IContextObject.Store{T}(object)"/>
        /// </summary>
        public void Store<T>(object obj)
        {
            //Replace the old object with the new if an old object exists, or add a new object
            TypedStorage.AddOrUpdate(typeof(T), obj, (type, existing) => obj);
        }

        /// <inheritdoc/>
        /// <summary>
        /// See <see cref="IContextObject.Store(string, object)"/>
        /// </summary>
        public void Store(string name, object obj)
        {
            //Replace the old object with the new if an old object exists, or add a new object
            NamedStorage.AddOrUpdate(name, obj, (n, existing) => obj);
        }
    }
}
