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
        /// <inheritdoc/>
        /// <summary>
        /// See <see cref="IKeyedCollection{TKey, TValue}.Storage"/>
        /// </summary>
        public ConcurrentDictionary<object, object> Storage { get; } = new ConcurrentDictionary<object, object>();

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
        /// See <see cref="IKeyedCollection{TKey, TValue}.this[TKey]"/>
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
        /// See <see cref="IKeyedCollection{TKey, TValue}.Store(TKey, TValue)"/>
        /// </summary>
        public void Store(object key, object value)
        {
            ThrowIfFinalized();
            //Replace the old object with the new if an old object exists, or add a new object
            Storage.AddOrUpdate(key, value, (oldKey, oldValue) => value);
        }

        /// <inheritdoc/>
        /// <summary>
        /// See <see cref="IKeyedCollection{TKey, TValue}.Retrieve(TKey)"/>
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
        /// See <see cref="IKeyedCollection{TKey, TValue}.TryRetrieve(TKey, out TValue)"/>
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

        /// <inheritdoc/>
        /// <summary>
        /// See <see cref="IKeyedCollection{TKey, TValue}.Clear()"/>
        /// </summary>
        public void Clear()
        {
            ThrowIfFinalized();

            Storage.Clear();
        }
    }
}
