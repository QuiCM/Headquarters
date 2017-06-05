using HQ.Interfaces;
using HQ.Parsing.IObjectConverters;
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
    /// The main point of control for the Headquarters library.
    /// A CommandRegistry provides methods to register commands, converters, and parsers, as well as providing input.
    /// </summary>
    public class CommandRegistry : IDisposable
    {
        private CommandQueue _queue;
        private ConcurrentDictionary<Type, IObjectConverter> _converters;
        private Type _asyncParser = typeof(AbstractParser);
        private Type _parser = typeof(Parser);

        /// <summary>
        /// A concurrent dictionary with Types as keys, and IObjectConverters to convert those Types as values
        /// </summary>
        public ConcurrentDictionary<Type, IObjectConverter> Converters
        {
            get
            {
                ThrowIfDisposed();
                return _converters;
            }
        }

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
        /// Sets the parser to be used when parsing commands. If <paramref name="asyncParser"/> is true, sets the async parser.
        /// </summary>
        /// <typeparam name="T">The type of the parser to be used</typeparam>
        /// <param name="asyncParser">Whether or not the new parser replaces the async parser</param>
        public void SetParser<T>(bool asyncParser = false) where T : AbstractParser
        {
            ThrowIfDisposed();
            if (asyncParser)
            {
                _asyncParser = typeof(T);
            }
            else
            {
                _parser = typeof(T);
            }
        }

        /// <summary>
        /// Returns a parser object.
        /// </summary>
        /// <param name="async">Whether or not the async parser is required</param>
        /// <param name="registry">Registry to be used by the parser</param>
        /// <param name="args">Arguments to be used by the parser</param>
        /// <param name="metadata">Metadata to be used by the parser</param>
        /// <param name="ctx">Context to be used by the parser</param>
        /// <param name="callback">Callback method to be invoked when the parser completes</param>
        public AbstractParser GetParser(bool async, CommandRegistry registry, IEnumerable<object> args, CommandMetadata metadata, IContextObject ctx, InputResultDelegate callback)
        {
            ThrowIfDisposed();
            if (async)
            {
                return (AbstractParser)Activator.CreateInstance(_asyncParser, registry, args, metadata, ctx, callback);
            }
            return (AbstractParser)Activator.CreateInstance(_parser, registry, args, metadata, ctx, callback);
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

        /// <summary>
        /// Queues an input for handling, returning a unique ID with which to obtain results
        /// </summary>
        /// <param name="input"></param>
        /// <param name="ctx"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public void HandleInput(string input, IContextObject ctx, InputResultDelegate callback)
        {
            ThrowIfDisposed();

            _queue.QueueInputHandling(input, ctx, callback);
        }

        /// <summary>
        /// Registers a type as a command
        /// </summary>
        /// <param name="type"></param>
        /// <param name="names"></param>
        /// <param name="description"></param>
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

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        private void ThrowIfDisposed()
        {
            if (disposedValue)
            {
                throw new ObjectDisposedException(nameof(CommandRegistry));
            }
        }

        /// <summary>
        /// Implements the IDisposable pattern
        /// </summary>
        /// <param name="disposing"></param>
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
        
        /// <summary>
        /// Disposes the registry and the underlying <see cref="CommandQueue"/>. A disposed registry cannot be reused.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
