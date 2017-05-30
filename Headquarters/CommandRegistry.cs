using HQ.Interfaces;
using HQ.ObjectConverters;
using HQ.Parsing;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using HQ.Extensions;

namespace HQ
{
    /*
        THOUGHT:
        Track the number of commands executed/messages handled per second.
        Every time number exceeds some stepped value, add another command queue.
        Select which command queue is used to run some input based on whichever queue is free.
        I.E., Dynamically create and release command queues as required.
        If this is implemented, input IDs returned by a command queue will need to be reworked
    */

    /// <summary>
    /// Responsible for command registrations and beginning the command execution process
    /// </summary>
    public class CommandRegistry : IDisposable
    {
        private CommandQueue _queue;
        private ConcurrentDictionary<Type, IObjectConverter> _converters;

        /// <summary>
        /// Event invoked when an input has been computed and a result returned.
        /// </summary>
        public event EventHandler<InputResultEventArgs> OnInputResult;

        /// <summary>
        /// A concurrent dictionary with Types as keys, and IObjectConverters to convert those Types as values
        /// </summary>
        public ConcurrentDictionary<Type, IObjectConverter> Converters => _converters;

        /// <summary>
        /// Gets an IObjectConverter for the given type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conversionType"></param>
        /// <returns></returns>
        public IObjectConverter GetConverter<T>(T conversionType) where T : Type
        {
            ThrowIfDisposed();

            if (_converters.TryGetValue(conversionType, out IObjectConverter converter))
            {
                return converter;
            }

            return null;
        }

        /// <summary>
        /// Constructs a new <see cref="CommandRegistry"/> using the given <see cref="RegistrySettings"/>
        /// </summary>
        /// <param name="settings"></param>
        public CommandRegistry(RegistrySettings settings)
        {
            _converters = new ConcurrentDictionary<Type, IObjectConverter>();
            if (settings.EnableDefaultConverters)
            {
                _converters.TryAdd(typeof(int[]), new IntArrayObjectConverter());
                _converters.TryAdd(typeof(int), new IntObjectConverter());
                _converters.TryAdd(typeof(string[]), new StringArrayObjectConverter());
                _converters.TryAdd(typeof(string), new StringObjectConverter());
            }

            if (settings.Converters != null)
            {
                foreach (IObjectConverter converter in settings.Converters)
                {
                    _converters.TryAdd(converter.ConversionType, converter);
                }
            }

            _queue = new CommandQueue(this, new System.Threading.CancellationTokenSource());
            _queue.BeginProcessing();
        }

        public int HandleInput(string input, IContextObject ctx)
        {
            ThrowIfDisposed();

            return _queue.QueueInputHandling(input, ctx);
        }

        /// <summary>
        /// Registers a type as a command
        /// </summary>
        /// <param name="type"></param>
        /// <param name="names"></param>
        /// <param name="description"></param>
        /// <exception cref="CommandParsingException">Thrown if the verifier fails when checking the given Type</exception>
        public CommandRegistry AddCommand(Type type, IEnumerable<RegexString> names, string description)
        {
            ThrowIfDisposed();

            FormatVerifier verifier = new FormatVerifier(type);
            verifier.Run();
            CommandMetadata metadata = verifier.Metadata;
            metadata.Aliases = names;
            metadata.Description = description;

            _queue.AddMetadata(metadata);

            return this;
        }

        internal void Invoke_InputResult(InputResult result, object output, int id)
        {
            OnInputResult?.Invoke(this, new InputResultEventArgs(result, output, id));
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        private void ThrowIfDisposed()
        {
            if (disposedValue)
            {
                throw new ObjectDisposedException(nameof(CommandRegistry));
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _queue.Dispose();
                    _converters.Clear();
                }

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}
