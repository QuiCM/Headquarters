using Headquarters_Core.Builders;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Headquarters_Core.Builders
{
    /// <summary>
    /// Provides contextual information to a <see cref="CommandBuilder"/>
    /// </summary>
    public class CommandBuilderContext : IBuilderContext
    {
        private Dictionary<int, IEnumerable<CommandMetadata>> _builtTypes;
        private readonly Regexer _regexer;

        /// <summary>
        /// Returns an enumerable of all acquired metadata
        /// </summary>
        public IEnumerable<CommandMetadata> Metadata => _builtTypes.Values.SelectMany(v => v);

        /// <summary>
        /// Constructs a new CommandBuilderContext instance
        /// </summary>
        public CommandBuilderContext()
        {
            _regexer = new Regexer();
            _builtTypes = new Dictionary<int, IEnumerable<CommandMetadata>>();
        }

        /// <summary>
        /// Attempts to retrieve metadata for a given type
        /// </summary>
        /// <param name="hostType"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public bool TryGetMetadata(Type hostType, out IEnumerable<CommandMetadata> metadata)
        {
            return _builtTypes.TryGetValue(hostType.GetHashCode(), out metadata);
        }

        /// <summary>
        /// Generates and assigns the command matcher for a given piece of metadata
        /// </summary>
        /// <param name="metadata"></param>
        public void GenerateCommandMatcher(CommandMetadata metadata)
        {
            metadata.Matcher = _regexer.GenerateCommandMatcher(metadata);
        }

        /// <summary>
        /// Adds metadata into the context
        /// </summary>
        /// <param name="hostType"></param>
        /// <param name="metadata"></param>
        public void AddMetadata(Type hostType, IEnumerable<CommandMetadata> metadata)
        {
            _builtTypes.Add(hostType.GetHashCode(), metadata);
        }
    }
}
