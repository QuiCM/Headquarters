using System.Collections.Generic;

namespace Headquarters.Communications
{
    public interface IPublication
    {
        int Length { get; }
        IEnumerable<string> Message { get; }
    }
}
