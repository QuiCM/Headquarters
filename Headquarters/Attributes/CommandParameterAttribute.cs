using System;

namespace HQ.Attributes
{
	/// <summary>
	/// Decorates a parameter used in a command's executing method
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true)]
	public class CommandParameterAttribute : Attribute
	{
		/// <summary>
		/// The number of arguments expected to be condensed into the parameter.
		/// For example, Repetitions = 3 would result in 3 arguments being used to create the parameter
		/// </summary>
		public int Repetitions { get; set; } = 1;
		/// <summary>
		/// Whether or not the parameter is optional
		/// </summary>
		public bool Optional { get; set; } = false;

		/// <summary>
		/// Creates a new <see cref="CommandParameterAttribute"/> with 1 mandatory repetition
		/// </summary>
		public CommandParameterAttribute() { }

		/// <summary>
		/// Creates a new <see cref="CommandParameterAttribute"/> with a number of mandatory repetition
		/// </summary>
		/// <param name="repetitions">The number of times the parameter type should be repeated</param>
		public CommandParameterAttribute(int repetitions)
		{
			Repetitions = repetitions;
		}

		/// <summary>
		/// Creates a new <see cref="CommandParameterAttribute"/> with 1 repetition and the provided optionality
		/// </summary>
		/// <param name="optional">Whether or not the parameter is optional</param>
		public CommandParameterAttribute(bool optional)
		{
			Optional = optional;
		}

		/// <summary>
		/// Creates a new <see cref="CommandParameterAttribute"/> with a number of repetitions and the provided optionality
		/// </summary>
		/// <param name="repetitions">The number of times the parameter type should be repeated</param>
		/// <param name="optional">Whether or not the parameter is optional</param>
		public CommandParameterAttribute(int repetitions, bool optional)
		{
			Repetitions = repetitions;
			Optional = optional;
		}
	}
}
