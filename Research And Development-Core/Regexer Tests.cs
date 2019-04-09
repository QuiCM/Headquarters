using Headquarters_Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.RegularExpressions;
using System.Linq;
using System;
using System.Threading.Tasks;

namespace Research_And_Development_Core
{
    [TestClass]
    public class RegexerTests
    {
        static readonly (string cmdString, int expectedMatches)[] commandData = new[]
        {
            ("Test", 0),
            ("Test command", 0),
            ("Test command \"test string\"", 2),
            ("Test command 1", 2),
            ("Test command \"test string\" 1", 3),
            ("Test command \"test string\" \"another\"", 3),
            ("Test command \"test string\" \"another\" 1", 4),
            ("Test command \"test string\" 1 \"another\"", 4),
            ("Test command \"test string\" \"more\" 1 \"another\"", 5)
        };

        static readonly (string cmdString, int expectedMatches)[] flaggedCommandData = new[]
        {
            ("Test", 0),
            ("Test flagged command", 0),
            ("Test flagged command -d \"test\"", 2),
            ("Test flagged command -d 1", 2),
            ("Test flagged command -d \"test string\" 1", 3),
            ("Test flagged command -d \"test string\" \"another\"", 3),
            ("Test flagged command -d \"test string\" \"another\" 1", 4),
            ("Test flagged command -d \"test string\" 1 \"another\"", 4),
            ("Test flagged command -d \"test string\" \"more\" 1 \"another\"", 5)
        };

        [TestMethod]
        public async Task TestMatching()
        {
            CommandMetadata metadatum = (await TestContext.Builder.BuildAsync(typeof(RegexerTests))).First(m => m.Name == "Test command");

            foreach ((string cmdString, int expectedMatches) in commandData)
            {
                Match match = metadatum.Matcher.Match(cmdString);
                // -1 to account for complete match (which isn't present if there's no matches, hence the Math.Max(0, x) call)
                Assert.AreEqual(expectedMatches, Math.Max(0, match.Groups.Count(g => g.Value != null && g.Value.Length > 0) - 1));
            }
        }

        [TestMethod]
        public async Task TestFlaggedMatching()
        {
            CommandMetadata metadatum = (await TestContext.Builder.BuildAsync(typeof(RegexerTests))).First(m => m.Name == "Test flagged command");

            foreach ((string cmdString, int expectedMatches) in flaggedCommandData)
            {
                Match match = metadatum.Matcher.Match(cmdString);
                // -1 to account for complete match (which isn't present if there's no matches, hence the Math.Max(0, x) call)
                Assert.AreEqual(expectedMatches, Math.Max(0, match.Groups.Count(g => g.Value != null && g.Value.Length > 0) - 1));
            }
        }

        [HQCommand(Name = "Test command")]
        public void TestCommand([HQCommandParameter(Name = "parameter one", Required = true)] string param1,
                                string param2,
                                [HQCommandParameter(Name = "Number", Required = false)] int param3,
                                string param4)
        {
        }

        [HQCommand(Name = "Test flagged command")]
        public void TestCustomMatchersCommand([HQCommandParameter(Name = "Custome one", Required = true, CustomMatcher = "-d {default}")] string flaggedParam1,
                                              string param2,
                                              [HQCommandParameter(Name = "Number", Required = false)] int param3,
                                              string param4)
        {
        }
    }
}
