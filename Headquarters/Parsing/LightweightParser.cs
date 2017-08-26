using HQ.Attributes;
using HQ.Exceptions;
using HQ.Extensions;
using HQ.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HQ.Parsing
{
    /// <summary>
    /// A lightweight parser for parsing a string on the fly
    /// </summary>
    public class LightweightParser
    {
        private IContextObject _ctx;
        private Queue<object> _conversions;
        private List<(Type, CommandParameterAttribute)> _data;

        /// <summary>
        /// Constructs a new lightweight parser with the given context
        /// </summary>
        /// <param name="context"></param>
        public LightweightParser(IContextObject context)
        {
            _conversions = new Queue<object>();
            _data = new List<(Type, CommandParameterAttribute)>();
            _ctx = context;
        }

        /// <summary>
        /// Adds a Type and usage information to the list of types to be parsed
        /// </summary>
        /// <param name="typeData"></param>
        /// <returns></returns>
        public LightweightParser AddType((Type, CommandParameterAttribute) typeData)
        {
            _data.Add(typeData);
            return this;
        }

        /// <summary>
        /// Adds a Type to the list of types to be parsed
        /// </summary>
        /// <param name="type"></param>
        /// <param name="paramData">An optional <see cref="CommandParameterAttribute"/> object providing rules for parsing the type</param>
        /// <returns></returns>
        public LightweightParser AddType(Type type, CommandParameterAttribute paramData = null)
        {
            return AddType((type, paramData));
        }

        /// <summary>
        /// Adds a type to the list of types to be parsed
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="paramData"></param>
        /// <returns></returns>
        public LightweightParser AddType<T>(CommandParameterAttribute paramData = null)
        {
            return AddType((typeof(T), paramData));
        }

        /// <summary>
        /// Parses the given input using the data added from <see cref="AddType(Type, CommandParameterAttribute)"/>
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public LightweightParser Parse(string input)
        {
            IEnumerable<object> arguments = input.ObjectiveExplode();
            //Index holds the current position inside the 'arguments' enumerable
            int index = 0;

            foreach ((Type type, CommandParameterAttribute param) parserData in _data)
            {
                Type type = parserData.type;
                CommandParameterAttribute param = parserData.param ?? new CommandParameterAttribute();

                int count = param.Repetitions <= 0 ? arguments.Count() - index
                                                        : param.Repetitions;

                if (index >= arguments.Count())
                {
                    _conversions.Enqueue(ObjectCreator.CreateDefaultObject(type));
                    continue;
                }
                
                object[] args = arguments.ReadToArray(index, count);

                if (count == 1 && args[0].GetType() == type)
                {
                    _conversions.Enqueue(args[0]);
                    index++;
                    continue;
                }

                IObjectConverter converter = _ctx.Registry.GetConverter(type);

                if (converter == null)
                {
                    //Use the object creator to attempt a conversion
                    _conversions.Enqueue(ObjectCreator.CreateObject(type, args, _ctx));
                }
                else
                {
                    //Use a defined converter.
                    object conversion = count > 1 ? converter.ConvertFromArray((string[])args, _ctx)
                                                  : converter.ConvertFromString(args[0].ToString(), _ctx);

                    if (conversion == null)
                    {
                        string failedMethod = count > 1 ? nameof(IObjectConverter.ConvertFromArray)
                                                        : nameof(IObjectConverter.ConvertFromString);

                        throw new CommandParsingException(
                               ParserFailReason.ParsingFailed,
                               $"Type conversion failed: Failed to convert '{string.Join(" ", args)}' to Type '{ type.Name }'.",
                               new Exception($"Conversion failed in '{converter.GetType().Name}.{failedMethod}'")
                        );
                    }

                    _conversions.Enqueue(conversion);
                }

                index += count;
            }

            return this;
        }

        /// <summary>
        /// Retrieves the next result from the parsing operation.
        /// Results should be retrieved in the same order as Types were added
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Get<T>()
        {
            if (_conversions.Count == 0)
            {
                return default(T);
            }

            return (T)_conversions.Dequeue();
        }
    }
}
