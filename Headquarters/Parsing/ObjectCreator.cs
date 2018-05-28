using HQ.Exceptions;
using HQ.Interfaces;
using System;
using System.ComponentModel;
using System.Reflection;

namespace HQ.Parsing
{
    /// <summary>
    /// Creates objects from types and strings
    /// </summary>
    public class ObjectCreator
    {
        /// <summary>
        /// Creates a default object for the type defined by the parameter info
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static object CreateDefaultObject(ParameterInfo parameter)
        {
            if (parameter.HasDefaultValue)
            {
                return parameter.DefaultValue;
            }

            return CreateDefaultObject(parameter.ParameterType);
        }

        /// <summary>
        /// Creates a default object of the given type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static object CreateDefaultObject<T>()
        {
            return CreateDefaultObject(typeof(T));
        }

        /// <summary>
        /// Creates a default object for the given type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object CreateDefaultObject(Type type)
        {
            if (type == typeof(string))
            {
                return null;
            }
            
            return Activator.CreateInstance(type);
        }

        /// <summary>
        /// Creates an object of the given type, using the given arguments for construction.
        /// Converters for object types are retrieved from the given registry
        /// </summary>
        /// <param name="type"></param>
        /// <param name="arguments"></param>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static object CreateObject(Type type, object[] arguments, IContextObject ctx)
        {
            string rejectionStr;
            if (arguments == null)
            {
                rejectionStr = $"failed to create an instance of type '{type.Name}' via the default constructor.";
            }
            else
            {
                rejectionStr = $"failed to create an instance of type '{type.Name}' from arguments '{string.Join(" ", arguments)}'.";
            }

            if (type == typeof(string))
            {
                //strings get special treatment - why convert to a string when it already is one?
                return string.Join(" ", arguments);
            }

            TypeInfo tInfo = type.GetTypeInfo();

            if (tInfo.IsValueType)
            {
                try
                {
                    //value types are converted with a TypeConverter
                    TypeConverter tc = TypeDescriptor.GetConverter(type);
                    return tc.ConvertFromString(string.Join(" ", arguments));
                }
                catch (Exception e)
                {
                    throw new CommandParsingException(ParserFailReason.ParsingFailed, $"TypeConverter {rejectionStr}", e);
                }
            }

            if (tInfo.IsArray)
            {
                //Arrays get their own method
                return CreateArray(type, arguments, ctx);
            }

            try
            {
                return Activator.CreateInstance(type, arguments);
            }
            catch (Exception e)
            {
                throw new CommandParsingException(ParserFailReason.ParsingFailed, $"Activator {rejectionStr}", e);
            }
        }

        /// <summary>
        /// Creates an array of the given type from the given arguments
        /// </summary>
        /// <param name="type"></param>
        /// <param name="arguments"></param>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static object CreateArray(Type type, object[] arguments, IContextObject ctx)
        {
            Type elementType = type.GetElementType();
            IObjectConverter converter = ctx.Registry.Converters.Retrieve(elementType);

            string failedConvert = $"Failed to convert '{string.Join(" ", arguments)}' to Type {type.Name}.";
            string failedCreate = $"failed to create an instance of type {elementType.Name} from argument ";

            //Create the generic array of the required size
            Array array = (Array)Activator.CreateInstance(type, arguments.Length);
            for (int i = 0; i < array.Length; i++)
            {
                if (converter != null)
                {
                    //if we have a converter, use it
                    object conversion = converter.ConvertFromString(arguments[i].ToString(), ctx);

                    if (conversion == null)
                    {
                        throw new CommandParsingException(
                            ParserFailReason.ParsingFailed,
                            failedConvert,
                            new Exception($"Conversion failed by '{converter.GetType().Name}.'")
                        );
                    }

                    array.SetValue(conversion, i);
                    continue;
                }

                if (elementType == typeof(string))
                {
                    //strings are special, so have special treatment
                    array.SetValue(arguments[i], i);
                    continue;
                }

                if (elementType.GetTypeInfo().IsValueType)
                {
                    //value types can be created with a typeconverter
                    TypeConverter tc = TypeDescriptor.GetConverter(type.GetElementType());
                    try
                    {
                        //but a bad argument will throw an exception, so handle that
                        array.SetValue(tc.ConvertFrom(arguments[i]), i);
                    }
                    catch (Exception e)
                    {
                        throw new CommandParsingException(ParserFailReason.ParsingFailed, $"TypeConverter {failedCreate} '{arguments[i]}'.", e);
                    }
                }
                else
                {
                    //reference types need to be created with the Activator
                    try
                    {
                        //once again, bad arguments can throw an exception
                        object element = Activator.CreateInstance(elementType, arguments[i]);
                        array.SetValue(element, i);
                    }
                    catch (Exception e)
                    {
                        throw new CommandParsingException(ParserFailReason.ParsingFailed, $"Activator {failedCreate} '{arguments[i]}'.", e);
                    }
                }
            }
            
            return array;
        }
    }
}
