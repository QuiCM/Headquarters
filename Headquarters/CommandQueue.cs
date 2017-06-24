using HQ.Interfaces;
using HQ.Parsing;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using HQ.Extensions;

namespace HQ
{
    /// <summary>
    /// Stores metadata and handles multi-threaded input parsing
    /// </summary>
    public class CommandQueue : IDisposable
    {
        internal class QueueData
        {
            internal string Input { get; }
            internal InputResultDelegate Callback { get; }
            internal IContextObject Context { get; }

            internal QueueData(string input, InputResultDelegate callback, IContextObject ctx)
            {
                Input = input;
                Callback = callback;
                Context = ctx;
            }
        }

        /// <summary>
        /// This queue must be concurrent as it can be modified on different threads at any time
        /// </summary>
        private ConcurrentQueue<QueueData> _queue;
        private List<CommandMetadata> _metadata;
        private CancellationTokenSource _tokenSource;
        private Object _lock;
        private ManualResetEvent _mre;
        private CommandRegistry _registry;

        private bool _started = false;

        /// <summary>
        /// Generates a new CommandQueue with the given CancellationToken
        /// </summary>
        /// <param name="registry"></param>
        /// <param name="tokenSource"></param>
        public CommandQueue(CommandRegistry registry, CancellationTokenSource tokenSource)
        {
            _registry = registry;
            _tokenSource = tokenSource;
            _queue = new ConcurrentQueue<QueueData>();
            _metadata = new List<CommandMetadata>();
            _lock = new Object();
            _mre = new ManualResetEvent(false);
        }

        /// <summary>
        /// Adds new command metadata
        /// </summary>
        /// <param name="metadata"></param>
        public void AddMetadata(CommandMetadata metadata)
        {
            ThrowIfDisposed();
            //Lock to add to metadata so that we don't modify the collection while it's being
            //iterated on another thread
            lock (_lock)
            {
                _metadata.Add(metadata);
            }
        }

        /// <summary>
        /// Starts the CommandQueue's input processing
        /// </summary>
        public void BeginProcessing()
        {
            ThrowIfDisposed();

            if (_started)
            {
                throw new InvalidOperationException("The CommandQueue has already been started.");
            }

            _started = true;

            Thread thread = new Thread(() => ParserThreadCallback());
            thread.Start();
        }

        /// <summary>
        /// Adds a command to the command queue, ready to be executed
        /// </summary>
        public void QueueInputHandling(string input, IContextObject ctx, InputResultDelegate callback)
        {
            ThrowIfDisposed();

            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }

            _queue.Enqueue(new QueueData(input, callback, ctx));

            //Set the MRE so that our parser thread knows there's data
            _mre.Set();
        }

        /// <summary>
        /// Stops the queue from handling input. A stopped queue cannot be started again
        /// </summary>
        public void Stop()
        {
            ThrowIfDisposed();
            _tokenSource.Cancel();
            _mre.Set();
        }

        private void ParserThreadCallback()
        {
            while (!_tokenSource.Token.IsCancellationRequested)
            {
                //Check if the MRE has been set
                if (!_mre.WaitOne(100))
                {
                    continue;
                }

                if (!_queue.TryDequeue(out QueueData data))
                {
                    _mre.Reset();
                    continue;
                }

                //The vertical bar is a pipe character. inputA | inputB = run command B with the output of command A
                string[] inputs = data.Input.Split('|');

                if (inputs.Length > 1)
                {
                    //If we have a piped command, send it off to a new thread to be handled,
                    //As each command needs to be handled in order
                    Thread pipeThread = new Thread(() => PipeThreadCallback(inputs, data.Callback, data.Context));
                    pipeThread.Start();
                    _mre.Set();
                    continue;
                }

                string input = data.Input; //We don't lower the data because case sensitivity is an option for command matching

                CommandMetadata metadata;
                lock (_lock)
                {
                    //Lock the metadata collection, and grab the first metadata that has a matching executor
                    List<CommandMetadata> metadatas = _metadata.Where(m => m.GetFirstOrDefaultExecutorData(input) != null).ToList();
                    metadata = metadatas.FirstOrDefault();
                }

                if (metadata == null)
                {
                    data.Callback.Invoke(InputResult.Unhandled, null);

                    //No command matches, so ignore this input
                    _mre.Set();
                    continue;
                }

                CommandExecutorData exeData = metadata.GetFirstOrDefaultExecutorData(input);

                RegexString trigger = exeData.ExecutorAttribute.CommandMatchers.First(m => m.Matches(input));
                input = trigger.RemoveMatchedString(input).TrimStart();
                
                AbstractParser parser = _registry.GetParser( _registry, input, null, metadata, exeData, data.Context, data.Callback);

                try
                {
                    parser.Start();
                }
                finally
                {
                    //Our job is done, so prepare for the next input
                    _mre.Set();
                }
            }

            _mre.Dispose();
        }

        private void PipeThreadCallback(string[] inputs, InputResultDelegate callback, IContextObject ctx)
        {
            object output = null;

            for (int i = 0; i < inputs.Length; i++)
            {
                string input = $"{inputs[i]}".Trim();
                CommandMetadata metadata;
                lock (_lock)
                {
                    //Lock the metadata collection, and grab the first metadata that has a matching executor
                    List<CommandMetadata> metadatas = _metadata.Where(m => m.GetFirstOrDefaultExecutorData(input) != null).ToList();
                    metadata = metadatas.FirstOrDefault();
                }

                if (metadata == null)
                {
                    //No command matches, so ignore this entire piped input
                    callback.Invoke(InputResult.Unhandled, null);
                    break;
                }

                CommandExecutorData exeData = metadata.GetFirstOrDefaultExecutorData(input);

                RegexString trigger = exeData.ExecutorAttribute.CommandMatchers.First(m => m.Matches(input));
                input = trigger.RemoveMatchedString(input);
                object[] arguments = null;
                if (output != null)
                {
                    //If there's output from a previous command, append it to the arguments for this command
                    arguments = new[] { output };
                }

                AbstractParser parser;

                if (i == inputs.Length - 1)
                {
                    //We only want the final parsing to invoke the parser callback
                    parser = _registry.GetParser(_registry, input, arguments, metadata, exeData, ctx, callback);
                }
                else
                {
                    parser = _registry.GetParser(_registry, input, arguments, metadata, exeData, ctx, null);
                }

                //Threads are joined for synchronous behaviour. Concurrency will not work here
                Thread thread = parser.GetThread();
                thread.Start();
                thread.Join();

                //Set output so that it's appended to the end of the next input
                output = parser.Output;
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        private void ThrowIfDisposed()
        {
            if (disposedValue)
            {
                throw new ObjectDisposedException(nameof(CommandQueue));
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
                    Stop();
                }

                disposedValue = true;
            }
        }

        /// <summary>
        /// Disposes the queue, stopping processing and releasing resources. A stopped queue cannot be restarted
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
