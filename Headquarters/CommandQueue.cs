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
            internal int ID { get; }
            internal IContextObject Context { get; }

            internal QueueData(string input, int id, IContextObject ctx)
            {
                Input = input;
                ID = id;
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

        private int _id = 0;

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
        public int QueueInputHandling(string input, IContextObject ctx)
        {
            ThrowIfDisposed();
            int id = Interlocked.Increment(ref _id);
            _queue.Enqueue(new QueueData(input, id, ctx));

            //Set the MRE so that our parser thread knows there's data
            _mre.Set();

            return id;
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
                    Thread pipeThread = new Thread(() => PipeThreadCallback(inputs, data.ID, data.Context));
                    pipeThread.Start();
                    _mre.Set();
                    continue;
                }

                string input = data.Input;

                List<CommandMetadata> metadatas;
                lock (_lock)
                {
                    //Lock while iterating collection, then release. Create a copy with ToList()
                    metadatas = _metadata.Where(c => c.Aliases.Any(a => a.Matches(input.ToLowerInvariant()))).ToList();
                }

                if (metadatas.Count == 0)
                {
                    _registry.Invoke_InputResult(InputResult.Unhandled, null, data.ID);

                    //No command matches, so ignore this input
                    _mre.Set();
                    continue;
                }

                RegexString trigger = metadatas.FirstOrDefault().Aliases.FirstOrDefault();
                input = trigger.RemoveMatchedString(input);
                IEnumerable<object> arguments = input.ObjectiveExplode();

                Parser parser = new Parser(_registry, arguments, metadatas.First(), data.Context, data.ID);
                parser.OnProcessingComplete += Parser_OnProcessingComplete;

                try
                {
                    parser.GetThread().Start();
                }
                finally
                {
                    //Our job is done, so prepare for the next input
                    _mre.Set();
                }
            }

            _mre.Dispose();
        }

        private void PipeThreadCallback(string[] inputs, int id, IContextObject ctx)
        {
            object output = null;

            for (int i = 0; i < inputs.Length; i++)
            {
                string input = $"{inputs[i]}".Trim();
                List<CommandMetadata> metadatas;
                lock (_lock)
                {
                    //Lock while iterating collection, then release. Create a copy with ToList() in order to be threadsafe
                    metadatas = _metadata.Where(c => c.Aliases.Any(a => a.Matches(input.ToLowerInvariant()))).ToList();
                }

                if (metadatas.Count == 0)
                {
                    //No command matches, so ignore this entire piped input
                    _registry.Invoke_InputResult(InputResult.Unhandled, null, id);
                    break;
                }

                RegexString trigger = metadatas.FirstOrDefault().Aliases.FirstOrDefault();
                input = trigger.RemoveMatchedString(input);
                IEnumerable<object> arguments = input.ObjectiveExplode();
                if (output != null)
                {
                    //If there's output from a previous command, append it to the arguments for this command
                    arguments = arguments.Concat(new[] { output });
                }

                Parser parser = new Parser(_registry, arguments, metadatas.First(), ctx, id);

                if (i == inputs.Length - 1)
                {
                    //We only want the final parsing to invoke the parser callback
                    parser.OnProcessingComplete += Parser_OnProcessingComplete;
                }

                //Threads are joined for synchronous behaviour. Concurrency will not work here
                Thread thread = parser.GetThread();
                thread.Start();
                thread.Join();

                //Set output so that it's appended to the end of the next input
                output = parser.Output;
            }
        }

        private void Parser_OnProcessingComplete(object sender, InputResultEventArgs e)
        {
            _registry.Invoke_InputResult(e.Result, e.Output, e.ID);
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

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
