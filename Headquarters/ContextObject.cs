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
        private ConcurrentDictionary<Type, object> _typedStorage = new ConcurrentDictionary<Type, object>();
        private ConcurrentDictionary<string, object> _namedStorage = new ConcurrentDictionary<string, object>();

        /// <inheritdoc/>
        /// <summary>
        /// See <see cref="IContextObject.TypedStorage"/>
        /// </summary>
        public ConcurrentDictionary<Type, object> TypedStorage => _typedStorage;

        /// <inheritdoc/>
        /// <summary>
        /// See <see cref="IContextObject.NamedStorage"/>
        /// </summary>
        public ConcurrentDictionary<string, object> NamedStorage => _namedStorage;

        /// <inheritdoc/>
        /// <summary>
        /// See <see cref="IContextObject.Registry"/>
        /// </summary>
        public CommandRegistry Registry { get; }

        /// <inheritdoc/>
        /// <summary>
        /// See <see cref="IContextObject.Finalized"/>
        /// </summary>
        public bool Finalized { get; set; }

        /// <inheritdoc/>
        /// <summary>
        /// See <see cref="IContextObject.this[Type]"/>
        /// </summary>
        public dynamic this[Type type]
        {
            get => Retrieve<dynamic>(type);
            set => Store(type, value);
        }

        /// <inheritdoc/>
        /// <summary>
        /// See <see cref="IContextObject.this[string]"/>
        /// </summary>
        public dynamic this[string name]
        {
            get => Retrieve<dynamic>(name);
            set => Store(name, value);
        }

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
        public T Retrieve<T>() => Retrieve<T>(typeof(T));

        /// <inheritdoc/>
        /// <summary>
        /// See <see cref="IContextObject.Retrieve{T}(Type)"/>
        /// </summary>
        public T Retrieve<T>(Type type)
        {
            ThrowIfFinalized();

            if (TypedStorage.TryGetValue(type, out object value))
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
            ThrowIfFinalized();

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
            ThrowIfFinalized();

            if ( TypedStorage.TryGetValue(typeof(T), out object val))
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
            ThrowIfFinalized();

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
        public void Store<T>(object obj) => Store(typeof(T), obj);

        /// <inheritdoc/>
        /// <summary>
        /// See <see cref="IContextObject.Store(Type, object)"/>
        /// </summary>
        public void Store(Type type, object obj)
        {
            ThrowIfFinalized();
            //Replace the old object with the new if an old object exists, or add a new object
            TypedStorage.AddOrUpdate(type, obj, (t, existing) => obj);
        }

        /// <inheritdoc/>
        /// <summary>
        /// See <see cref="IContextObject.Store(string, object)"/>
        /// </summary>
        public void Store(string name, object obj)
        {
            ThrowIfFinalized();
            //Replace the old object with the new if an old object exists, or add a new object
            NamedStorage.AddOrUpdate(name, obj, (n, existing) => obj);
        }

        private void ThrowIfFinalized()
        {
            if (Finalized)
            {
                throw new InvalidOperationException("This context has been finalized.");
            }
        }
    }
}
