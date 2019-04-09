using System.Threading.Tasks;

namespace Headquarters_Core.Builders.Parameters
{
    /// <summary>
    /// Interface definition for a class intended to construct a parameter's type
    /// </summary>
    public abstract class ParameterBuilder<TType> : IBuilder<TType, string[]>
    {
        public abstract IBuilderContext BuilderContext { get; set; }
        public BuildState<TType> BuildState { get; set; } = new BuildState<TType>();

        /// <summary>
        /// Sets the builder's <see cref="BuildState"/> to <see cref="BuildState.Status.Faulted"/>
        /// </summary>
        /// <param name="e"></param>
        public TType Fail(System.Exception e) => BuildState.Fail(e);
        /// <summary>
        /// Sets the builder's <see cref="BuildState"/> to <see cref="BuildState.Status.Success"/>
        /// </summary>
        public TType Succeed(TType result) => BuildState.Succeed(result);
        /// <summary>
        /// Sets the builder's <see cref="BuildState"/> to <see cref="BuildState.Status.Warning"/>
        /// </summary>
        public void Warn(string message) => BuildState.Warn(message);

        public abstract Task<TType> BuildAsync(params string[] args);

        public ParameterBuilder(ParameterBuilderContext context)
        {
            BuilderContext = context;
        }
    }
}