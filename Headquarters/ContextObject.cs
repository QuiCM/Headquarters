using HQ.Interfaces;
using System;
using System.Collections.Concurrent;

namespace HQ
{
    /// <inheritdoc/>
    /// <summary>
    /// The default implementation of <see cref="IContextObject"/>
    /// </summary>
    public class ContextObject : IContextObject
    {
        private ConcurrentDictionary<object, object> _storage = new ConcurrentDictionary<object, object>();

        /// <inheritdoc/>
        /// <summary>
        /// See <see cref="IConcurrentStorage{T}.Storage"/>
        /// </summary>
        public ConcurrentDictionary<object, object> Storage => _storage;
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
        /// See <see cref="IConcurrentStorage{T}.this[T]"/>
        /// </summary>
        public dynamic this[object type]
        {
            get => Retrieve(type);
            set => Store(type, value);
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
        /// See <see cref="IConcurrentStorage{T}.Store(T, object)"/>
        /// </summary>
        public void Store(object key, object value)
        {
            ThrowIfFinalized();
            //Replace the old object with the new if an old object exists, or add a new object
            Storage.AddOrUpdate(key, value, (oldKey, oldValue) => value);
        }

        /// <inheritdoc/>
        /// <summary>
        /// See <see cref="IConcurrentStorage{T}.Retrieve(T)"/>
        /// </summary>
        public dynamic Retrieve(object key)
        {
            ThrowIfFinalized();

            if (Storage.TryGetValue(key, out dynamic value))
            {
                return value;
            }

            return null;
        }

        /// <inheritdoc/>
        /// <summary>
        /// See <see cref="IConcurrentStorage{T}.TryRetrieve(T, out dynamic)"/>
        /// </summary>
        public bool TryRetrieve(object key, out dynamic value)
        {
            ThrowIfFinalized();

            if (Storage.TryGetValue(key, out value))
            {
                return true;
            }

            value = null;
            return false;
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
