using System;
using System.Threading.Tasks;

namespace Headquarters_Core.Builders
{
    public interface IBuilder<RetType, InType>
    {
        /// <summary>
        /// Contextual information passed to the builder upon creation
        /// </summary>
        IBuilderContext BuilderContext { get; set; }
        /// <summary>
        /// Provides information on the state of a build
        /// </summary>
        BuildState<RetType> BuildState { get; set; }

        /// <summary>
        /// Invoked when building
        /// </summary>
        /// <param name=""></param>
        /// <param name="args"></param>
        /// <returns></returns>
        Task<RetType> BuildAsync(InType args);

        /// <summary>
        /// When overriden in a derived type, fails the build operation with the given exception
        /// </summary>
        RetType Fail(Exception e);
    }
}
