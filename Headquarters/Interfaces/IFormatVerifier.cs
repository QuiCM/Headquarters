using System.Collections.Generic;
using System.Reflection;

namespace HQ.Interfaces
{
    /// <summary>
    /// Defines the methods a command format verifier will need to implement
    /// </summary>
    public interface IFormatVerifier
    {
        void Run();
        void CheckClassAttribute();
        void CheckExecutor();
        void CheckMethodStructure(MethodInfo info);
        void CheckSubExecutorMethodStructures();
        void CheckParameterStructure(IEnumerable<ParameterInfo> parameters);
    }
}
