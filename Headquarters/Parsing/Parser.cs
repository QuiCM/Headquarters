using HQ.Attributes;
using HQ.Exceptions;
using HQ.Interfaces;
using HQ.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace HQ.Parsing
{
    /// <summary>
    /// Responsible for parsing input into types required by a command, then executing the command
    /// </summary>
    public class Parser
    {
        private IEnumerable<object> _args;
        private CommandMetadata _metadata;
        private IContextObject _context;
        private int _id;
        private CommandRegistry _registry;
        private List<Object> _objects;

        /// <summary>
        /// The output of the executed command. Null until execution completes
        /// </summary>
        public object Output { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<InputResultEventArgs> OnProcessingComplete;

        /// <summary>
        /// Generates a new parser that uses the given registry, args, metadata, context, and ID to run
        /// </summary>
        /// <param name="registry">Registry from which the parser will obtain <see cref="IObjectConverter"/>s</param>
        /// <param name="args">Enumerable of objects to be parsed</param>
        /// <param name="metadata">CommandMetadata containing information used to parse and execute</param>
        /// <param name="ctx">Context object passed to the executed command, and an <see cref="IObjectConverter"/>s that are used</param>
        /// <param name="id">The unique ID generated when input was received by a <see cref="CommandQueue"/></param>
        public Parser(CommandRegistry registry, IEnumerable<object> args, CommandMetadata metadata, IContextObject ctx, int id)
        {
            _registry = registry;
            _args = args;
            _metadata = metadata;
            _context = ctx;
            _id = id;
        }

        /// <summary>
        /// Returns the thread on which the parser will run
        /// </summary>
        /// <returns></returns>
        public Thread GetThread()
        {
            //Run the executor thread
            return new Thread(() => ParserThreadCallback());
        }

        /// <summary>
        /// Runs the parser operations
        /// </summary>
        private void ParserThreadCallback()
        {
            try
            {
                CheckBasicArgumentRules();
                AttemptSwitchToSubcommand();
                ConvertArgumentsToTypes(_context);

                object command = Activator.CreateInstance(_metadata.Type);
                Output = _metadata.ExecutingMethod.Invoke(command, _objects.ToArray());

                OnProcessingComplete?.Invoke(this, new InputResultEventArgs(InputResult.Success, Output, _id));
            }
            catch (Exception e)
            {
                OnProcessingComplete?.Invoke(this, new InputResultEventArgs(InputResult.Failure, e, _id));
            }
        }

        /// <summary>
        /// Ensures that the arguments provided meet basic rules in order to be used with the metadata provided
        /// </summary>
        private void CheckBasicArgumentRules()
        {
            if (_args == null)
            {
                throw new CommandParsingException(ParserFailReason.InvalidArguments, "Null was provided as arguments.");
            }

            if (_args.Count() < _metadata.RequiredArguments)
            {
                throw new CommandParsingException(ParserFailReason.InvalidArguments, $"Insufficient arguments provided. Expected {_metadata.RequiredArguments} but received {_args.Count()}.");
            }
        }

        /// <summary>
        /// Attempts to switch to a subcommand instead of the main executor
        /// </summary>
        private void AttemptSwitchToSubcommand()
        {
            if (!_metadata.HasSubcommands || _args.Count() == 0)
            {
                return;
            }

            CommandMetadata subcommand = _metadata.SubcommandMetadata.FirstOrDefault(
                c => c.Aliases.Any(
                    a => a.Matches(_args.First().ToString().ToLowerInvariant())
                )
            );

            if (subcommand != null)
            {
                _metadata = subcommand;
                //Remove the subcommand name
                _args = _args.Skip(1);
            }
        }

        /// <summary>
        /// Attempts to convert the arguments provided into objects of types required by the command executor
        /// </summary>
        /// <param name="ctx"></param>
        private void ConvertArgumentsToTypes(IContextObject ctx)
        {
            _objects = new List<object>() { ctx };
            int index = 0;

            foreach (KeyValuePair<ParameterInfo, CommandParameterAttribute> kvp in _metadata.ParameterData)
            {
                //Get the number of arguments going in to the parameter
                int count = kvp.Value.Repetitions <= 0 ? _args.Count() - index
                                                        : kvp.Value.Repetitions;

                if (index >= _args.Count())
                {
                    //If we've used all our arguments, just add empty ones to satisfy the
                    //method signature for the command
                    _objects.Add(ObjectCreator.CreateDefaultObject(kvp.Key));
                    continue;
                }

                object[] args = _args.ReadToArray(index, count);

                //If the provided object is already of the required type, add and continue
                if (count == 1 && args[0].GetType() == kvp.Key.ParameterType)
                {
                    _objects.Add(args[0]);
                    continue;
                }

                IObjectConverter converter = _registry.GetConverter(kvp.Key.ParameterType);
                if (converter == null)
                {
                    //Use the object creator to attempt a conversion
                    _objects.Add(ObjectCreator.CreateObject(kvp.Key.ParameterType, args, ctx, _registry));
                }
                else
                {
                    //Use a defined converter.
                    object conversion = count > 1 ? converter.ConvertFromArray((string[])args, ctx)
                                                  : converter.ConvertFromString(args[0].ToString(), ctx);

                    if (conversion == null)
                    {
                        throw new CommandParsingException(
                               ParserFailReason.ParsingFailed,
                               $"Type conversion failed: Failed to convert '{string.Join(" ", args)}' to Type '{ kvp.Key.ParameterType.Name }'.",
                               new Exception($"Conversion failed in '{converter.GetType().Name}.{nameof(IObjectConverter.ConvertFromArray)}'")
                        );
                    }

                    _objects.Add(conversion);
                }

                index += count;
            }
        }
    }
}
