using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Test.Shared;
using Touchstone.Core;
using Touchstone.NunitAdapter;

namespace Test.Nunit
{
    /// <summary>
    /// NUnit adapter for the shared Touchstone test suite. Each PrettyId test descriptor defined in
    /// Test.Shared is surfaced as an individual NUnit test case and executed through the descriptor's
    /// own delegate, so the source of truth remains a single shared definition.
    /// </summary>
    [TestFixture]
    public sealed class PrettyIdNunitTests
    {
        /// <summary>
        /// Enumerate all non-skipped test case descriptors across every suite.
        /// </summary>
        /// <returns>Test case descriptors as NUnit test case data.</returns>
        private static IEnumerable TestCases()
        {
            return new TouchstoneTestCaseSource(TestSuites.All);
        }

        /// <summary>
        /// Execute a single Touchstone test case descriptor.
        /// </summary>
        /// <param name="testCase">Test case to run.</param>
        [Test]
        [TestCaseSource(nameof(TestCases))]
        public async Task Run(TestCaseDescriptor testCase)
        {
            await testCase.ExecuteAsync(CancellationToken.None);
        }
    }
}
