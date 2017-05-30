using HQ.Exceptions;
using System;

namespace HQ
{
	/// <summary>
	/// Optional class from which commands can inherit to provide helper functions
	/// </summary>
	public class Command
	{
		/// <summary>
		/// Fails the command, providing no reason or exception
		/// </summary>
		/// <returns></returns>
		public object Fail() => Fail("", null);

		/// <summary>
		/// Fails the command with the given message
		/// </summary>
		/// <param name="message"></param>
		public object Fail(string message) => Fail(message, null);

		/// <summary>
		/// Fails the command with the given exception
		/// </summary>
		/// <param name="ex"></param>
		public object Fail(Exception ex) => Fail("Command failed!", ex);

		/// <summary>
		/// Fails the command with the given message and exception
		/// </summary>
		/// <param name="message"></param>
		/// <param name="ex"></param>
		public object Fail(string message, Exception ex)
		{
			throw new CommandFailedException(message, ex);
		}
    }
}
