using System;

namespace Headquarters_Core
{
    [AttributeUsage(AttributeTargets.Method)]
    public class HQCommandAttribute : Attribute
    {
        /// <summary>
        /// User-friendly command name
        /// </summary>
        public string Name { get; set; }
    }
}
