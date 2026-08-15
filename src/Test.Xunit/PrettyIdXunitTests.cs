using System.Threading;
using System.Threading.Tasks;
using Test.Shared;
using Touchstone.Core;
using Touchstone.XunitAdapter;
using Xunit;

namespace Test.Xunit
{
    /// <summary>
    /// xUnit adapter for the shared Touchstone test suite. Each PrettyId test descriptor defined in
    /// Test.Shared is surfaced as an individual xUnit theory row and executed through the descriptor's
    /// own delegate, so the source of truth remains a single shared definition.
    /// </summary>
    public sealed class PrettyIdXunitTests
    {
        /// <summary>
        /// Theory data yielding one row per non-skipped test case descriptor across all suites.
        /// </summary>
        public static TouchstoneTheoryData TestCases => new TouchstoneTheoryData(TestSuites.All);

        /// <summary>
        /// Execute a single Touchstone test case descriptor.
        /// </summary>
        /// <param name="testCase">Test case to run.</param>
        [Theory]
        [MemberData(nameof(TestCases))]
        public async Task Run(TestCaseDescriptor testCase)
        {
            await testCase.ExecuteAsync(CancellationToken.None);
        }
    }
}
