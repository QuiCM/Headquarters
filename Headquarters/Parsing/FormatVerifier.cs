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
    * The method must return type Task<Object>.
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
    public class FormatVerifier : IFormatVerifier
    {
        private Type _type;
        private MethodInfo _executor;
        private IEnumerable<MethodInfo> _subExecutors;
        private ParameterInfo[] _parameters;
        private Dictionary<ParameterInfo, CommandParameterAttribute> _parameterMetadata;
        private int _requiredArgumentCount;
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
        public void Run()
        {
            CheckClassAttribute();
            CheckExecutor();
            CheckMethodStructure(_executor);
            CheckParameterStructure(_parameters.Skip(1));

            _commandMetadata = new CommandMetadata
            {
                Type = _type,
                ExecutingMethod = _executor,
                RequiredArguments = _requiredArgumentCount,
                ParameterData = _parameterMetadata,
                SubcommandMetadata = new List<CommandMetadata>(),
                 AsyncExecution = _async
            };

            CheckSubExecutorMethodStructures();
        }

        /// <summary>
        /// Ensures that the type follows command class rules
        /// </summary>
        /// <returns></returns>
        /// <exception cref="CommandParsingException">Thrown if the class does not conform to command class style rules</exception>
        public void CheckClassAttribute()
        {
            CommandClassAttribute attr = _type.GetTypeInfo().GetCustomAttribute<CommandClassAttribute>();
            if (attr == null)
            {
                throw new CommandParsingException(
                    ParserFailReason.IncorrectType,
                    $"Command '{_type.Name}' does not display a '{nameof(CommandClassAttribute)}'");
            }
            _async = attr.AsyncExecution;
        }

        /// <summary>
        /// Ensures that the type has a method with a <see cref="CommandExecutorAttribute"/> decoration
        /// </summary>
        /// <exception cref="CommandParsingException">Thrown if the executor does not follow command executor style rules</exception>
        public void CheckExecutor()
        {
            try
            {
                _executor = _type.GetRuntimeMethods().First(
                    m => m.GetCustomAttribute<CommandExecutorAttribute>() != null
                );
            }
            catch (Exception e)
            {
                throw new CommandParsingException(
                    ParserFailReason.NoExecutorFound,
                    $"Failed to discover an executor for command '{_type.Name}'.",
                    e
                );
            }

            if (_async && _executor.GetCustomAttribute<System.Runtime.CompilerServices.AsyncStateMachineAttribute>() == null)
            {
                throw new CommandParsingException(
                    ParserFailReason.MalformedExecutor,
                    $"'{_type.Name}' claims to have an async executor, but does not declare the 'async' keyword."
                );
            }
        }

        /// <summary>
        /// Ensures the executing method follows the command executor method rules
        /// </summary>
        /// <exception cref="CommandParsingException">Thrown if the executor does not follow command executor method rules</exception>
        public void CheckMethodStructure(MethodInfo method)
        {
            if (!_async)
            {
                if (method.ReturnType != typeof(object))
                {
                    throw new CommandParsingException(
                        ParserFailReason.MalformedExecutor,
                        $"Executor method '{method.Name}' of command '{_type.Name}' does not return '{nameof(Object)}'."
                    );
                }
            }
            else
            {
                if (method.ReturnType != typeof(Task<object>))
                {
                    throw new CommandParsingException(
                           ParserFailReason.MalformedExecutor,
                           $"Executor method '{method.Name}' of command '{_type.Name}' does not return '{nameof(Task<object>)}'."
                       );
                }
            }

            _parameters = method.GetParameters();
            Exception inner = null;

            if (_parameters.Length < 1)
            {
                inner = new Exception("Method defines no parameters, but requires at least one.");
            }
            else if (!typeof(IContextObject).GetTypeInfo().IsAssignableFrom(_parameters[0].ParameterType))
            {
                inner = new InvalidCastException($"Parameter '{_parameters[0].Name}' of type '{_parameters[0].ParameterType.Name}' must be castable to type '{nameof(IContextObject)}'.");
            }

            if (inner != null)
            {
                throw new CommandParsingException(
                    ParserFailReason.MalformedExecutor,
                    $"Executor method '{method.Name}' of command '{_type.Name}' does not meet the required parameter rulings.",
                    inner
                );
            }
        }

        /// <summary>
        /// Ensures all subcommand executors follow the command executor method rules
        /// </summary>
        /// <exception cref="CommandParsingException">Thrown if the subcommand executor does not follow command executor method rules</exception>
        public void CheckSubExecutorMethodStructures()
        {
            _subExecutors = _type.GetRuntimeMethods().Where(m => m.GetCustomAttribute<SubcommandExecutorAttribute>() != null);

            foreach (MethodInfo method in _subExecutors)
            {
                CheckMethodStructure(method);
                CheckParameterStructure(_parameters.Skip(1));

                CommandMetadata metadata = new CommandMetadata
                {
                    Type = _type,
                    ExecutingMethod = method,
                    RequiredArguments = _requiredArgumentCount,
                    ParameterData = _parameterMetadata,
                    Aliases = method.GetCustomAttribute<SubcommandExecutorAttribute>().SubCommands
                };

                (_commandMetadata.SubcommandMetadata as List<CommandMetadata>).Add(metadata);
            }
        }

        /// <summary>
        /// Ensures the given parameters follow the parameter rules
        /// </summary>
        /// <exception cref="CommandParsingException">Thrown if a parameter does not follow command parameter rules</exception>
        public void CheckParameterStructure(IEnumerable<ParameterInfo> parameters)
        {
            bool optionalFound = false;
            bool unknownLengthFound = false;
            _requiredArgumentCount = 0;
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
                    _requiredArgumentCount = -1;
                }
                if (!attr.Optional)
                {
                    _requiredArgumentCount += attr.Repetitions;
                }
            }

            _parameterMetadata = paramData;
        }
    }
}
