using HQ.Interfaces;
using HQ.Parsing.IObjectConverters;
using HQ.Parsing;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

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
        /// Sets the parser to be used when parsing commands.
        /// </summary>
        /// <typeparam name="T">The type of the parser to be used</typeparam>
        public void SetParser<T>() where T : AbstractParser
        {
            ThrowIfDisposed();
            _parser = typeof(T);
        }

        /// <summary>
        /// Returns an instance of the parser that currently registered.
        /// </summary>
        /// <param name="registry">Registry to be used by the parser</param>
        /// <param name="input">String input provided to the parser</param>
        /// <param name="args">Additional arguments to be used by the parser</param>
        /// <param name="metadata">Metadata to be used by the parser</param>
        /// <param name="exeData">CommandExecutorData to be used by the parser</param>
        /// <param name="ctx">Context to be used by the parser</param>
        /// <param name="callback">Callback method to be invoked when the parser completes</param>
        public AbstractParser GetParser(CommandRegistry registry,
            string input, 
            IEnumerable<object> args,
            CommandMetadata metadata,
            CommandExecutorData exeData, 
            IContextObject ctx, 
            InputResultDelegate callback)
        {
            ThrowIfDisposed();
            return (AbstractParser)Activator.CreateInstance(_parser, registry, input, args, metadata, exeData, ctx, callback);
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
            }

            if (settings.Converters != null)
            {
                foreach (IObjectConverter converter in settings.Converters)
                {
                    _converters.TryAdd(converter.ConversionType, converter);
                }
            }

            _parser = settings.Parser;

            _queue = new CommandQueue(this, new System.Threading.CancellationTokenSource());
            _queue.BeginProcessing();
        }

        /// <summary>
        /// Queues an input for handling, returning a unique ID with which to obtain results
        /// </summary>
        /// <param name="input">The string from which command data will be parsed</param>
        /// <param name="ctx">A context object which is passed to the command method, and is used to convert data types</param>
        /// <param name="callback">A callback method for when the command completes</param>
        /// <returns></returns>
        public void HandleInput(string input, IContextObject ctx, InputResultDelegate callback)
        {
            ThrowIfDisposed();

            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }

            _queue.QueueInputHandling(input, ctx, callback);
        }

        /// <summary>
        /// Registers a type as a command container
        /// </summary>
        /// <param name="type">The type to be registered</param>
        public CommandRegistry AddCommand(Type type)
        {
            ThrowIfDisposed();

            _queue.AddMetadata(new FormatVerifier(type).Run().Metadata);
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
