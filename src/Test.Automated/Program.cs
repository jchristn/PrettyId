using System.Threading;
using System.Threading.Tasks;
using Test.Shared;
using Touchstone.Cli;

namespace Test.Automated
{
    /// <summary>
    /// Touchstone CLI runner for the PrettyId test suite. Executes every descriptor defined in
    /// Test.Shared and writes a colored tabular summary to the console, returning a non-zero exit
    /// code when any test fails.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Entry point.
        /// </summary>
        /// <param name="args">Command-line arguments (unused).</param>
        /// <returns>Exit code: 0 when all tests pass, 1 when any test fails.</returns>
        public static async Task<int> Main(string[] args)
        {
            return await ConsoleRunner.RunAsync(TestSuites.All, null, null, CancellationToken.None);
        }
    }
}
