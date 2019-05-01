using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Headquarters_Core.Builders
{
    /// <summary>
    /// Builds command metadata from Types
    /// </summary>
    public class CommandBuilder : IBuilder<IEnumerable<CommandMetadata>, Type>
    {
        private CommandBuilderContext Context => (CommandBuilderContext)BuilderContext;

        /// <summary>
        /// <see cref="IBuilderContext"/> instance providing contextual information to this builder
        /// </summary>
        public IBuilderContext BuilderContext { get; set; }
        public BuildState<IEnumerable<CommandMetadata>> BuildState { get; set; }

        public BuildStatus Status => BuildState.Result;

        /// <summary>
        /// Constructs a new CommandBuilder instance in the given context
        /// </summary>
        public CommandBuilder(CommandBuilderContext context)
        {
            BuilderContext = context;
            BuildState = new BuildState<IEnumerable<CommandMetadata>>();
        }

        /// <summary>
        /// Builds metadata for all commands found on the given type
        /// </summary>
        /// <param name="hostType"></param>
        /// <returns></returns>
        public Task<IEnumerable<CommandMetadata>> BuildAsync(Type hostType)
        {
            return Task.Run(() =>
            {
                BuildState.Reset();

                if (Context.TryGetMetadata(hostType, out IEnumerable<CommandMetadata> metadata))
                {
                    return BuildState.Succeed(metadata);
                }

                metadata =
                    (from methodInfo
                     in hostType.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                     where methodInfo.GetCustomAttribute<HQCommandAttribute>() != null
                     select new CommandMetadata
                     {
                         Attribute = methodInfo.GetCustomAttribute<HQCommandAttribute>(),
                         Method = methodInfo
                     }
                    ).ToList();

                if (metadata.Count() < 1)
                {
                    return Fail(
                        new BuildStateException(
                            Constants.Exceptions.CommandBuilder.NoValidMetadataException,
                            hostType
                        )
                    );
                }

                foreach (CommandMetadata metadatum in metadata)
                {
                    BuildCommand(metadatum);
                }

                Context.AddMetadata(hostType, metadata);

                return BuildState.Succeed(metadata);
            });
        }

        private void BuildCommand(CommandMetadata metadatum)
        {
            metadatum.Parameters = (
                from ParameterInfo param in metadatum.Method.GetParameters()
                select new ParameterMetadata
                {
                    ParameterInfo = param,
                    Attribute = param.GetCustomAttribute<HQCommandParameterAttribute>()
                }
            ).ToList();

            Context.GenerateCommandMatcher(metadatum);
        }

        public IEnumerable<CommandMetadata> Fail(Exception e) => BuildState.Fail(e);

        private void OnFailedBuild(object sender, Exception e)
        {

        }
    }
}