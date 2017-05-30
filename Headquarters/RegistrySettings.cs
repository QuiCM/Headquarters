using HQ.Interfaces;
using System.Collections.Generic;

namespace HQ
{
    public class RegistrySettings
    {
        public bool EnableDefaultConverters { get; set; } = false;
        public IEnumerable<IObjectConverter> Converters { get; set; }
    }
}
