using HQ.Attributes;
using System;
using System.Reflection;
using System.Linq;
using HQ.Exceptions;
using HQ.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HQ.Parsing
{
    /* 
    * **COMMAND CLASS RULE SET**
    * A class that specifies a command must be decorated with a CommandClassAttribute
    * 
    * **COMMAND EXECUTOR RULE SET**
    * A method that is considered the main runnable method for a command must be decorated with a CommandExecutorAttribute.
    * The method must return type Task<Object> or Object.
    * The method must have at least 1 parameter.
    * The method's first parameter must be castable to type IContextObject.
    * 
    * **COMMAND SUBCOMMAND RULE SET**
    * A method that is considered a sub-command runnable method for a command must be decorated with a SubcommandExecutorAttribute.
    * The method must return type Task<Object> or Object.
    * The method must have at least 1 parameter.
    * The method's first parameter must be castable to type IContextObject.
    * 
    * **COMMAND PARAMETER RULE SET**
    * Parameters in an executor or subcommand must follow the following rules:
    * An optional parameter must not be followed by any required parameter
    * An unknown-lengthed parameter must not be followed by any parameters.
    */

    /// <summary>
    /// Ensures a command class and its components conform to a set of rules.
    /// </summary>
    public class FormatVerifier
    {
        private Type _type;
        private List<CommandExecutorData> _executors;
        private List<CommandExecutorData> _subExecutors;
        private CommandPrecondition _precondition;
        private int requiredArgumentCount;

        /// <summary>
        /// Metadata about the command discovered during the verification process
        /// </summary>
        public CommandMetadata Metadata { get; private set; }

        /// <summary>
        /// Creates a new FormatVerifier to verify the given type
        /// </summary>
        /// <param name="type"></param>
        public FormatVerifier(Type type)
        {
            _type = type;
        }

        /// <summary>
        /// Runs the verifier, generating command metadata if the verification succeeds
        /// </summary>
        public FormatVerifier Run()
        {
            VerifyClassAttribute();

            DiscoverExecutors();
            foreach (CommandExecutorData data in _executors)
            {
                ParameterInfo[] parameters = VerifyMethodStructure(data);
                CheckParameterStructure(data, parameters.Skip(1));
            }

            DiscoverSubexecutorMethods();
            foreach (CommandExecutorData data in _subExecutors)
            {
                ParameterInfo[] parameters = VerifyMethodStructure(data);
                CheckParameterStructure(data, parameters.Skip(1));
            }

            foreach (CommandExecutorData data in _executors)
            {
                data.Subcommands = _subExecutors.Where(sub => sub.ParentCommand == data);
            }

            DiscoverPrecondition();

            Metadata = new CommandMetadata
            {
                Executors = _executors,
                Type = _type,
                Precondition = _precondition
            };

            return this;
        }

        /// <summary>
        /// Ensures that the type follows command class rules
        /// </summary>
        /// <returns></returns>
        /// <exception cref="CommandParsingException">Thrown if the class does not conform to command class style rules</exception>
        public void VerifyClassAttribute()
        {
            CommandClassAttribute attr = _type.GetTypeInfo().GetCustomAttribute<CommandClassAttribute>();
            //If the command class is not tagged with a CommandClassAttribute, the command has been defined incorrectly
            if (attr == null)
            {
                throw new CommandParsingException(
                    ParserFailReason.IncorrectType,
                    $"Command '{_type.Name}' does not display a '{nameof(CommandClassAttribute)}'");
            }
        }

        /// <summary>
        /// Ensures that the type has a method with a <see cref="CommandExecutorAttribute"/> decoration
        /// </summary>
        /// <exception cref="CommandParsingException">Thrown if the executor does not follow command executor style rules</exception>
        public void DiscoverExecutors()
        {
            //Selects all methods present on the type that are decorated with CommandExecutorAttributes
            _executors = (from methodInfo in _type.GetRuntimeMethods().Where(m => m.GetCustomAttribute<CommandExecutorAttribute>() != null
                          && !(m.GetCustomAttribute<CommandExecutorAttribute>() is SubcommandExecutorAttribute))
                          select new CommandExecutorData
                          {
                              ExecutorAttribute = methodInfo.GetCustomAttribute<CommandExecutorAttribute>(),
                              ExecutingMethod = methodInfo,
                              AsyncExecution = methodInfo.GetCustomAttribute<System.Runtime.CompilerServices.AsyncStateMachineAttribute>() != null
                          }).ToList();

            //If the command class has no executors, the command has been defined incorrectly
            if (_executors.Count() == 0)
            {
                throw new CommandParsingException(
                    ParserFailReason.NoExecutorFound,
                    $"Failed to discover an executor for command '{_type.Name}'."
                );
            }
        }

        /// <summary>
        /// Discovers the first precondition method specified on the command class
        /// </summary>
        public void DiscoverPrecondition()
        {
            MethodInfo pre = _type.GetRuntimeMethods().FirstOrDefault(m => m.GetCustomAttribute<PreconditionAttribute>() != null);
            if (pre != null)
            {
                if (pre.ReturnType != typeof(InputResult) && pre.ReturnType != typeof(bool))
                {
                    throw new CommandParsingException(
                        ParserFailReason.MalformedExecutor,
                        $"Precondition must return '{nameof(InputResult)}' or '{nameof(Boolean)}'."
                    );
                }

                ParameterInfo[] parameters = pre.GetParameters();
                if (parameters.Length < 1 || !typeof(IContextObject).GetTypeInfo().IsAssignableFrom(parameters[0].ParameterType))
                {
                    throw new CommandParsingException(
                        ParserFailReason.MalformedExecutor,
                        $"Precondition must have one parameter of type '{nameof(IContextObject)}"
                    );
                }
            }

            _precondition = new CommandPrecondition(pre);
        }

        /// <summary>
        /// Ensures the executing method follows the command executor method rules
        /// </summary>
        /// <exception cref="CommandParsingException">Thrown if the executor does not follow command executor method rules</exception>
        public ParameterInfo[] VerifyMethodStructure(CommandExecutorData data)
        {
            MethodInfo mInfo = data.ExecutingMethod;
            if (!data.AsyncExecution)
            {
                //If the command is not asynchronous and does not return Object, the command is defined incorrectly
                if (mInfo.ReturnType != typeof(object))
                {
                    throw new CommandParsingException(
                        ParserFailReason.MalformedExecutor,
                        $"Executor method '{mInfo.Name}' of command '{_type.Name}' does not return '{nameof(Object)}'."
                    );
                }
            }
            else
            {
                //If the command is asynchronous and does not return Task<Object>, the command is defined incorrectly
                if (mInfo.ReturnType != typeof(Task<object>))
                {
                    throw new CommandParsingException(
                           ParserFailReason.MalformedExecutor,
                           $"Executor method '{mInfo.Name}' of command '{_type.Name}' does not return '{nameof(Task<object>)}'."
                       );
                }
            }

            ParameterInfo[] parameters = mInfo.GetParameters();
            Exception inner = null;

            //If the method has no parameters, the command is defined incorrectly
            if (parameters.Length < 1)
            {
                inner = new Exception("Method defines no parameters, but requires at least one.");
            }
            //If the first parameter of the method is not of a type inheriting from IContextObject, the command is defined incorrectly
            else if (!typeof(IContextObject).GetTypeInfo().IsAssignableFrom(parameters[0].ParameterType))
            {
                inner = new InvalidCastException($"Parameter '{parameters[0].Name}' of type '{parameters[0].ParameterType.Name}' must be castable to type '{nameof(IContextObject)}'.");
            }
            //If the command specifies a number of format parameters, but the method parameters are fewer, the command is defined incorrectly
            else if (parameters.Length <= data.ExecutorAttribute.CommandMatcher.FormatParameters.Count())
            {
                inner = new Exception($"Method requires at least {data.ExecutorAttribute.CommandMatcher.FormatParameters.Count()}"
                    + $"parameters to satisfy its format parameters, but only provides {parameters.Length}.");
            }

            if (inner != null)
            {
                throw new CommandParsingException(
                    ParserFailReason.MalformedExecutor,
                    $"Executor method '{mInfo.Name}' of command '{_type.Name}' does not meet the required parameter rulings.",
                    inner
                );
            }

            return parameters;
        }

        /// <summary>
        /// Ensures all subcommand executors follow the command executor method rules
        /// </summary>
        /// <exception cref="CommandParsingException">Thrown if the subcommand executor does not follow command executor method rules</exception>
        public void DiscoverSubexecutorMethods()
        {
            _subExecutors = (from methodInfo in _type.GetRuntimeMethods().Where(m => m.GetCustomAttribute<SubcommandExecutorAttribute>() != null)
                             select new CommandExecutorData
                             {
                                 ExecutorAttribute = methodInfo.GetCustomAttribute<SubcommandExecutorAttribute>(),
                                 ExecutingMethod = methodInfo,
                                 AsyncExecution = methodInfo.GetCustomAttribute<System.Runtime.CompilerServices.AsyncStateMachineAttribute>() != null,
                                 ParentCommand = _executors.First(
                                     e => e.ExecutingMethod.Name == methodInfo.GetCustomAttribute<SubcommandExecutorAttribute>().ParentName)
                             }).ToList();
        }

        /// <summary>
        /// Ensures the given parameters follow the parameter rules
        /// </summary>
        /// <exception cref="CommandParsingException">Thrown if a parameter does not follow command parameter rules</exception>
        public void CheckParameterStructure(CommandExecutorData executor, IEnumerable<ParameterInfo> parameters)
        {
            bool optionalFound = false;
            bool unknownLengthFound = false;
            requiredArgumentCount = 0;
            Dictionary<ParameterInfo, CommandParameterAttribute> paramData = new Dictionary<ParameterInfo, CommandParameterAttribute>();

            List<string> formatParams = executor.ExecutorAttribute.CommandMatcher.FormatParameters;
            //Index refers to the current index of the parameter being investigated.
            //It is offset by 1 (index - 1) to retrieve the format parameter from formatParams that matches the parameter info
            int index = 1;

            foreach (ParameterInfo param in parameters)
            {
                CommandParameterAttribute attr = param.GetCustomAttribute<CommandParameterAttribute>();

                //All parameters should have attributes
                if (attr == null)
                {
                    attr = new CommandParameterAttribute(optional: param.IsOptional)
                    {
                        _generated = true
                    };
                }

                if (index < formatParams.Count + 1)
                {
                    //If the current format parameter doesn't match the current parameter, the command is defined incorrectly
                    if (param.Name != formatParams[index - 1])
                    {
                        throw new CommandParsingException(
                            ParserFailReason.InvalidParameter,
                            $"Parameter '{param.Name}' does not match equivalent format parameter. Should be '{formatParams[index - 1]}'."
                        );
                    }

                    index++;

                    //If an optional parameter has been found previously, and this parameter is not optional, the command is defined incorrectly
                    if (optionalFound && !attr.Optional)
                    {
                        throw new CommandParsingException(
                            ParserFailReason.InvalidParameter,
                            $"Parameter '{param.Name}' is required, but follows an optional parameter."
                        );
                    }

                    //If an unlengthed parameter has been found previously, the command is defined incorrectly
                    if (unknownLengthFound)
                    {
                        throw new CommandParsingException(
                            ParserFailReason.InvalidParameter,
                            $"Parameter '{param.Name}' follows an unknown-length parameter."
                        );
                    }

                    if (attr.Optional)
                    {
                        optionalFound = true;
                    }
                    if (attr.Repetitions < 1)
                    {
                        unknownLengthFound = true;
                        requiredArgumentCount = -1;
                    }
                    if (!attr.Optional)
                    {
                        requiredArgumentCount += attr.Repetitions;
                    }

                    paramData.Add(param, attr);
                }
            }

            executor.ParameterData = paramData;
        }
    }
}
