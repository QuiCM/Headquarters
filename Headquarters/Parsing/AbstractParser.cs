using HQ.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;

namespace HQ.Parsing
{
    /// <summary>
    /// The abstract base class for a command parser.
    /// </summary>
    public abstract class AbstractParser
    {
        /// <summary>
        /// Input provided to the command
        /// </summary>
        protected string Input { get; set; }
        /// <summary>
        /// Arguments provided to the parser
        /// </summary>
        protected IEnumerable<object> AdditionalArgs { get; set; }
        /// <summary>
        /// Metadata provided to the parser
        /// </summary>
        protected CommandMetadata Metadata { get; set; }
        /// <summary>
        /// Executor data provided to the parser
        /// </summary>
        protected CommandExecutorData ExecutorData { get; set; }
        /// <summary>
        /// Context provided to the parser, passed to <see cref="IObjectConverter"/>s, and passed to the command executor
        /// </summary>
        protected IContextObject Context { get; set; }
        /// <summary>
        /// Callback method invoked when processing completes
        /// </summary>
        protected InputResultDelegate Callback { get; set; }
        /// <summary>
        /// Registry from which <see cref="IObjectConverter"/>s are obtained
        /// </summary>
        protected CommandRegistry Registry { get; set; }
        /// <summary>
        /// List of objects parsed from <see cref="Input"/>
        /// </summary>
        protected List<Object> Objects { get; set; }

        /// <summary>
        /// Output from executing the parsed command
        /// </summary>
        public object Output { get; protected set; }

        /// <summary>
        /// Generates a new parser that uses the given registry, args, metadata, context, and ID to run
        /// </summary>
        /// <param name="registry">Registry from which the parser will obtain <see cref="IObjectConverter"/>s</param>
        /// <param name="input">The original input string</param>
        /// <param name="additionalArgs">Enumerable of objects to be parsed</param>
        /// <param name="metadata">CommandMetadata containing information used to parse and execute</param>
        /// <param name="exeData"><see cref="CommandExecutorData"/> containing the data required for execution</param>
        /// <param name="ctx">Context object passed to the executed command, and an <see cref="IObjectConverter"/>s that are used</param>
        /// <param name="callback">Reference to a method to be invoked when parsing completes</param>
        public AbstractParser(CommandRegistry registry,
            string input,
            IEnumerable<object> additionalArgs,
            CommandMetadata metadata,
            CommandExecutorData exeData,
            IContextObject ctx,
            InputResultDelegate callback)
        {
            Registry = registry;
            Input = input;
            AdditionalArgs = additionalArgs;
            Metadata = metadata;
            ExecutorData = exeData;
            Context = ctx;
            Callback = callback;
        }

        /// <summary>
        /// Returns the thread on which the parser will execute
        /// </summary>
        /// <returns></returns>
        public abstract Thread GetThread();

        /// <summary>
        /// Begins the processing of the parser
        /// </summary>
        public abstract void Start();

        /// <summary>
        /// Executing method of the thread
        /// </summary>
        protected abstract void ThreadCallback();

        /// <summary>
        /// Ensures that <see cref="AdditionalArgs"/> contains a suitable number of arguments
        /// </summary>
        protected abstract void CheckBasicArgumentRules();

        /// <summary>
        /// Attempts to switch the command target to a subcommand, if any suitable subcommands exist.
        /// </summary>
        protected abstract void AttemptSwitchToSubcommand();

        /// <summary>
        /// Attempts to fill <see cref="Objects"/> with objects parsed from <see cref="AdditionalArgs"/>
        /// </summary>
        /// <param name="context"></param>
        protected abstract void ConvertArgumentsToTypes(IContextObject context);
    }
}
