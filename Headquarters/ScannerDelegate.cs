using HQ.Interfaces;
using HQ.Parsing;

namespace HQ
{
    /// <summary>
    /// Delegate method called when a scanner receives a line of text that matches its required pattern
    /// </summary>
    /// <param name="context">The <see cref="IContextObject"/> sent with the message that invoked the callback</param>
    /// <param name="matchedString">The message that invoked the callback</param>
    /// <param name="lwParser">A <see cref="LightweightParser"/> object for parsing the matched string into objects</param>
    /// <param name="finalizeScanner">A boolean value that determines whether the scanner should be removed after the delegate completes</param>
    /// <returns></returns>
    public delegate object ScannerDelegate(IContextObject context, string matchedString, LightweightParser lwParser, ref bool finalizeScanner);
}
