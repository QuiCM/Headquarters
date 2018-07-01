using HQ.Attributes;
using HQ.Exceptions;
using HQ.Interfaces;
using HQ.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

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
        /// <param name="input">The original input string</param>
        /// <param name="additionalArgs">Any additional arguments to be added to the end of the argument list</param>
        /// <param name="metadata">CommandMetadata containing information used to parse and execute</param>
        /// <param name="exeData"><see cref="CommandExecutorData"/> containing the data required for execution</param>
        /// <param name="ctx">Context object passed to the executed command, and an <see cref="IObjectConverter"/>s that are used</param>
        /// <param name="callback">Reference to a method used as a callback when processing completes</param>
        public Parser(CommandRegistry registry,
            string input,
            IEnumerable<object> additionalArgs,
            CommandMetadata metadata,
            CommandExecutorData exeData,
            IContextObject ctx,
            InputResultDelegate callback)
            : base(registry, input, additionalArgs, metadata, exeData, ctx, callback)
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
            object command = Activator.CreateInstance(Metadata.Type);
            try
            {
                CheckBasicArgumentRules();
                InputResult preResult = InputResult.Failure;

                if (Metadata.Precondition.IsAsync)
                {
                    Task.Run(async () =>
                    {
                        preResult = await Metadata.Precondition.InvokeAsync(command, Context);
                    }).Wait();
                }
                else
                {
                    preResult = Metadata.Precondition.Invoke(command, Context);
                }

                if (preResult == InputResult.Failure)
                {
                    Output = $"[{nameof(Metadata.Type)}]: Precondition failed, command execution halted";
                    Callback?.Invoke(InputResult.Failure, Output);
                    return;
                }

                AttemptSwitchToSubcommand();
                ConvertArgumentsToTypes(Context);
                Output = ExecutorData.ExecutingMethod.Invoke(command, Objects.ToArray());

                if (ExecutorData.AsyncExecution)
                {
                    Task.Run(async () =>
                    {
                        Task<object> task = (Task<object>)Output;
                        Output = await task;
                    }).Wait();
                }

                Callback?.Invoke(InputResult.Success, Output);
            }
            catch (CommandParsingException e)
            {
                if (Metadata.ErrorHandler.IsAsync)
                {
                    Task.Run(async () =>
                    {
                        Task task = Metadata.ErrorHandler.InvokeAsync(command, Context, (Type)e.Data["ExpectedType"], e.Data["FailedInput"] as string, e);
                        await task;
                    }).Wait();
                }
                else
                {
                    Metadata.ErrorHandler.Invoke(command, Context, (Type)e.Data["ExpectedType"], e.Data["FailedInput"] as string, e);
                }

                Output = e;
                Callback?.Invoke(InputResult.Failure, Output);
            }
            catch (Exception e)
            {
                if (Metadata.ErrorHandler.IsAsync)
                {
                    Task.Run(async () =>
                    {
                        Task task = Metadata.ErrorHandler.InvokeAsync(command, Context, null, Input, e);
                        await task;
                    }).Wait();
                }
                else
                {
                    Metadata.ErrorHandler.Invoke(command, Context, null, Input, e);
                }

                Output = e;
                Callback?.Invoke(InputResult.Failure, Output);
            }
        }

        /// <summary>
        /// Ensures that the arguments provided meet basic rules in order to be used with the metadata provided
        /// </summary>
        protected override void CheckBasicArgumentRules()
        {
            if (Input == null)
            {
                CommandParsingException ex = new CommandParsingException(ParserFailReason.InvalidArguments, "Null was provided as arguments.");
                ex.Data.Add("FailedInput", null);
                ex.Data.Add("ExpectedType", null);
                throw ex;
            }
        }

        /// <summary>
        /// Attempts to switch to a subcommand instead of the main executor
        /// </summary>
        protected override void AttemptSwitchToSubcommand()
        {
            if (!ExecutorData.HasSubcommands || Input.Length == 0)
            {
                return;
            }

            CommandExecutorData subcommand = ExecutorData.Subcommands.FirstOrDefault(
                sub => sub.ExecutorAttribute.CommandMatcher.Matches(Input)
            );

            if (subcommand != null)
            {
                ExecutorData = subcommand;
                //Remove the subcommand name
                Input = subcommand.ExecutorAttribute.CommandMatcher.RemoveMatchedString(Input);
            }
        }

        /// <summary>
        /// Attempts to convert the arguments provided into objects of types required by the command executor
        /// </summary>
        /// <param name="ctx"></param>
        protected override void ConvertArgumentsToTypes(IContextObject ctx)
        {
            //All commands start with an IContextObject parameter
            Objects = new List<object> { ctx };
            int argumentIndex = 0;
            int parameterIndex = 0;
            IEnumerable<object> arguments = Input.ObjectiveExplode();

            if (AdditionalArgs != null)
            {
                arguments = arguments.Concat(AdditionalArgs);
            }

            foreach (KeyValuePair<ParameterInfo, CommandParameterAttribute> kvp in ExecutorData.ParameterData)
            {
                //Get the number of arguments going in to the parameter
                int count = kvp.Value.Repetitions <= 0 ? arguments.Count() - argumentIndex
                                                        : kvp.Value.Repetitions;

                //We only want to use the dynamically prepared argument length if one was not specified by the parameter attribute
                if (kvp.Value._generated)
                {
                    RegexString matcher = ExecutorData.ExecutorAttribute.CommandMatcher;
                    if (parameterIndex < matcher.FormatParameters.Count)
                    {
                        string fmtParam = matcher.FormatParameters[parameterIndex];
                        count = matcher.FormatData[fmtParam].Repetitions;
                    }
                }

                if (argumentIndex >= arguments.Count())
                {
                    //If we've used all our arguments, just add empty ones to satisfy the
                    //method signature for the command
                    Objects.Add(ObjectCreator.CreateDefaultObject(kvp.Key));
                    parameterIndex++;
                    continue;
                }

                object[] args = arguments.ReadToArray(argumentIndex, count);

                //If the provided object is already of the required type, add and continue
                if (count == 1 && args[0].GetType() == kvp.Key.ParameterType)
                {
                    Objects.Add(args[0]);
                    argumentIndex += count;
                    parameterIndex++;
                    continue;
                }

                IObjectConverter converter = Registry.Converters.Retrieve(kvp.Key.ParameterType);
                if (converter == null)
                {
                    //Use the object creator to attempt a conversion
                    Objects.Add(ObjectCreator.CreateObject(kvp.Key.ParameterType, args, ctx));
                }
                else
                {
                    //Use a defined converter.
                    object conversion = count > 1 ? converter.ConvertFromArray((string[])args, ctx)
                                                  : converter.ConvertFromString(args[0].ToString(), ctx);

                    if (conversion == null)
                    {
                        CommandParsingException ex = new CommandParsingException(
                               type: ParserFailReason.ParsingFailed,
                               message: $"Type conversion failed: Failed to convert '{string.Join(" ", args)}' to Type '{ kvp.Key.ParameterType.Name }'.",
                               innerException: new Exception($"Conversion failed in '{converter.GetType().Name}.{nameof(IObjectConverter.ConvertFromArray)}'")
                        );
                        ex.Data.Add("FailedInput", count > 1 ? string.Join(" ", (string[])args) : args[0].ToString());
                        ex.Data.Add("ExpectedType", kvp.Key.ParameterType);
                        throw ex;
                    }

                    Objects.Add(conversion);
                }

                parameterIndex++;
                argumentIndex += count;
            }
        }
    }
}
