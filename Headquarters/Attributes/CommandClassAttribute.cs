using System;

namespace HQ.Attributes
{
	/// <summary>
	/// Decorates a class to mark it as containing commands.
	/// Allows definition of the caching rule for this command
	/// </summary>
    public class CommandClassAttribute : Attribute
    {
		/// <summary>
		/// Whether or not the command object will be cached and reused between executions.
		/// Defaults to true
		/// </summary>
		public bool CacheMetadata { get; } = true;

		/// <summary>
		/// Decorates a class containing commands, and optionally sets the metadata caching rule
		/// </summary>
		/// <param name="cacheMetadata"></param>
		public CommandClassAttribute(bool cacheMetadata = true)
		{
			CacheMetadata = cacheMetadata;
		}
    }
}
