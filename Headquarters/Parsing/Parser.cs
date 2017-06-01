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
    public class Parser : AbstractParser
    {
        /// <summary>
        /// Generates a new parser that uses the given registry, args, metadata, context, and ID to run
        /// </summary>
        /// <param name="registry">Registry from which the parser will obtain <see cref="IObjectConverter"/>s</param>
        /// <param name="args">Enumerable of objects to be parsed</param>
        /// <param name="metadata">CommandMetadata containing information used to parse and execute</param>
        /// <param name="ctx">Context object passed to the executed command, and an <see cref="IObjectConverter"/>s that are used</param>
        /// <param name="callback">Reference to a method used as a callback when processing completes</param>
        public Parser(CommandRegistry registry, IEnumerable<object> args, CommandMetadata metadata, IContextObject ctx, InputResultDelegate callback)
            : base(registry, args, metadata, ctx, callback)
        {
        }

        /// <summary>
        /// Returns the thread on which the parser will run
        /// </summary>
        /// <returns></returns>
        public override Thread GetThread()
        {
            //Run the executor thread
            return new Thread(() => ThreadCallback());
        }

        /// <summary>
        /// Begins the processing of the parser
        /// </summary>
        public override void Start()
        {
            GetThread().Start();
        }

        /// <summary>
        /// Runs the parser operations
        /// </summary>
        protected override void ThreadCallback()
        {
            try
            {
                CheckBasicArgumentRules();
                AttemptSwitchToSubcommand();
                ConvertArgumentsToTypes(Context);

                object command = Activator.CreateInstance(Metadata.Type);
                Output = Metadata.ExecutingMethod.Invoke(command, Objects.ToArray());
                Callback?.Invoke(InputResult.Success, Output);
            }
            catch (Exception e)
            {
                Output = e;
                Callback?.Invoke(InputResult.Failure, Output);
            }
        }

        /// <summary>
        /// Ensures that the arguments provided meet basic rules in order to be used with the metadata provided
        /// </summary>
        protected override void CheckBasicArgumentRules()
        {
            if (Args == null)
            {
                throw new CommandParsingException(ParserFailReason.InvalidArguments, "Null was provided as arguments.");
            }

            if (Args.Count() < Metadata.RequiredArguments)
            {
                throw new CommandParsingException(ParserFailReason.InvalidArguments, $"Insufficient arguments provided. Expected {Metadata.RequiredArguments} but received {Args.Count()}.");
            }
        }

        /// <summary>
        /// Attempts to switch to a subcommand instead of the main executor
        /// </summary>
        protected override void AttemptSwitchToSubcommand()
        {
            if (!Metadata.HasSubcommands || Args.Count() == 0)
            {
                return;
            }

            CommandMetadata subcommand = Metadata.SubcommandMetadata.FirstOrDefault(
                c => c.Aliases.Any(
                    a => a.Matches(Args.First().ToString().ToLowerInvariant())
                )
            );

            if (subcommand != null)
            {
                Metadata = subcommand;
                //Remove the subcommand name
                Args = Args.Skip(1);
            }
        }

        /// <summary>
        /// Attempts to convert the arguments provided into objects of types required by the command executor
        /// </summary>
        /// <param name="ctx"></param>
        protected override void ConvertArgumentsToTypes(IContextObject ctx)
        {
            Objects = new List<object>() { ctx };
            int index = 0;

            foreach (KeyValuePair<ParameterInfo, CommandParameterAttribute> kvp in Metadata.ParameterData)
            {
                //Get the number of arguments going in to the parameter
                int count = kvp.Value.Repetitions <= 0 ? Args.Count() - index
                                                        : kvp.Value.Repetitions;

                if (index >= Args.Count())
                {
                    //If we've used all our arguments, just add empty ones to satisfy the
                    //method signature for the command
                    Objects.Add(ObjectCreator.CreateDefaultObject(kvp.Key));
                    continue;
                }

                object[] args = Args.ReadToArray(index, count);

                //If the provided object is already of the required type, add and continue
                if (count == 1 && args[0].GetType() == kvp.Key.ParameterType)
                {
                    Objects.Add(args[0]);
                    continue;
                }

                IObjectConverter converter = Registry.GetConverter(kvp.Key.ParameterType);
                if (converter == null)
                {
                    //Use the object creator to attempt a conversion
                    Objects.Add(ObjectCreator.CreateObject(kvp.Key.ParameterType, args, ctx, Registry));
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

                    Objects.Add(conversion);
                }

                index += count;
            }
        }
    }
}
