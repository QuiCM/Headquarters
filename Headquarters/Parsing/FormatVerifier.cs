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
    * The method must return type Task<Object>.
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
        private ParameterInfo[] _parameters;
        private Dictionary<ParameterInfo, CommandParameterAttribute> _parameterMetadata;
        private int requiredArgumentCount;
        private bool _async;

        private CommandMetadata _commandMetadata;

        /// <summary>
        /// Metadata about the command discovered during the verification process
        /// </summary>
        public CommandMetadata Metadata => _commandMetadata;

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

            _commandMetadata = new CommandMetadata
            {
                Executors = _executors,
                Type = _type
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
            _executors = (from methodInfo in _type.GetRuntimeMethods().Where(m => m.GetCustomAttribute<CommandExecutorAttribute>() != null)
                          select new CommandExecutorData
                          {
                              ExecutorAttribute = methodInfo.GetCustomAttribute<CommandExecutorAttribute>(),
                              ExecutingMethod = methodInfo,
                              AsyncExecution = methodInfo.GetCustomAttribute<System.Runtime.CompilerServices.AsyncStateMachineAttribute>() != null
                          }).ToList();

            if (_executors.Count() == 0)
            {
                throw new CommandParsingException(
                    ParserFailReason.NoExecutorFound,
                    $"Failed to discover an executor for command '{_type.Name}'."
                );
            }
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

            if (parameters.Length < 1)
            {
                inner = new Exception("Method defines no parameters, but requires at least one.");
            }
            else if (!typeof(IContextObject).GetTypeInfo().IsAssignableFrom(parameters[0].ParameterType))
            {
                inner = new InvalidCastException($"Parameter '{parameters[0].Name}' of type '{_parameters[0].ParameterType.Name}' must be castable to type '{nameof(IContextObject)}'.");
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

            foreach (ParameterInfo param in parameters)
            {
                CommandParameterAttribute attr = param.GetCustomAttribute<CommandParameterAttribute>();

                if (attr == null)
                {
                    attr = new CommandParameterAttribute(param.IsOptional);
                }

                paramData.Add(param, attr);

                if (optionalFound && !attr.Optional)
                {
                    throw new CommandParsingException(
                        ParserFailReason.InvalidParameter,
                        $"Parameter '{param.Name}' is required, but follows an optional parameter."
                    );
                }

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
            }

            executor.ParameterData = paramData;
        }
    }
}
