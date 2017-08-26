using HQ.Interfaces;
using HQ.Parsing;
using System.Threading.Tasks;

namespace HQ
{
    /// <summary>
    /// Delegate method called when a scanner receives a line of text that matches its required pattern
    /// </summary>
    /// <param name="context">The <see cref="IContextObject"/> sent with the message that invoked the callback</param>
    /// <param name="matchedString">The message that invoked the callback</param>
    /// <param name="lwParser">A <see cref="LightweightParser"/> object for parsing the matched string into objects</param>
    /// <returns></returns>
    public delegate object ScannerDelegate(IContextObject context, string matchedString, LightweightParser lwParser);

    /// <summary>
    /// Asynchronous delegate method called when a scanner receives a line of text that matches its required pattern
    /// </summary>
    /// <param name="context">The <see cref="IContextObject"/> sent with the message that invoked the callback</param>
    /// <param name="matchedString">The message that invoked the callback</param>
    /// <param name="lwParser">A <see cref="LightweightParser"/> object for parsing the matched string into objects</param>
    /// <returns></returns>
    public delegate Task<object> AsyncScannerDelegate(IContextObject context, string matchedString, LightweightParser lwParser);
}
