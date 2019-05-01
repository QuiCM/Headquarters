using Headquarters_Core.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Headquarters_Core
{
    public class HQContext
    {
        public CommandBuilderContext BuilderContext { get; }
        public CommandBuilder Builder { get; }

        public HQContext()
        {
            BuilderContext = new CommandBuilderContext();
            Builder = new CommandBuilder(BuilderContext);
        }

        public async Task RegisterTypeAsync(Type type)
        {
            _ = await Builder.BuildAsync(type);
            if (Builder.Status == BuildStatus.Faulted)
            {
                //One day this might notify the user that their type can't build. For now chuck an exception at them
                throw Builder.BuildState.Exception;
            }

            if ((Builder.Status | BuildStatus.Warning) == BuildStatus.Warning)
            {
                //Notify of warnings
            }
        }
    }
}
