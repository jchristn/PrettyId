using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PrettyId;

namespace Test
{
    class Program
    {
        private static int _totalTests = 0;
        private static int _passedTests = 0;
        private static int _failedTests = 0;

        static int Main(string[] args)
        {
            Console.WriteLine("================================================================================");
            Console.WriteLine("PrettyId Library - Automated Test Suite");
            Console.WriteLine("================================================================================\n");

            // Run all test categories
            TestBasicGeneration();
            TestPrefixGeneration();
            TestCustomCharacterSets();
            TestUrlSafeGeneration();
            TestKSortableGeneration();
            TestBase64EncodedGeneration();
            TestCryptographicRngGeneration();
            TestConfigurationOptions();
            TestEdgeCases();
            TestErrorHandling();
            TestThreadSafety();
            TestAdditionalCoverage();

            // Print summary
            Console.WriteLine("\n" + "=".PadRight(80, '='));
            Console.WriteLine("Test Summary");
            Console.WriteLine("=".PadRight(80, '='));
            Console.WriteLine($"Total Tests:  {_totalTests}");
            Console.WriteLine($"Passed:       {_passedTests} ({(_totalTests > 0 ? (_passedTests * 100.0 / _totalTests) : 0):F1}%)");
            Console.WriteLine($"Failed:       {_failedTests} ({(_totalTests > 0 ? (_failedTests * 100.0 / _totalTests) : 0):F1}%)");
            Console.WriteLine("=".PadRight(80, '='));

            if (_failedTests == 0)
            {
                Console.WriteLine("\n✓ ALL TESTS PASSED!");
                return 0;
            }
            else
            {
                Console.WriteLine($"\n✗ {_failedTests} TEST(S) FAILED");
                return 1;
            }
        }

        static void TestBasicGeneration()
        {
            Console.WriteLine("[Basic Generation Tests]");

            IdGenerator generator = new IdGenerator();

            // Test default generation
            RunTestWithSample("Default generation produces 32 char ID", () =>
            {
                string id = generator.Generate();
                return id.Length == 32 ? id : null;
            });

            // Test specific length generation
            RunTestWithSample("Generate 64 char ID", () =>
            {
                string id = generator.Generate(64);
                return id.Length == 64 ? id : null;
            });

            // Test short IDs
            RunTestWithSample("Generate 8 char ID", () =>
            {
                string id = generator.Generate(8);
                return id.Length == 8 ? id : null;
            });

            // Test generated IDs contain only alphanumeric characters
            RunTestWithSample("Generated ID contains only alphanumeric chars", () =>
            {
                string id = generator.Generate();
                return id.All(c => char.IsLetterOrDigit(c)) ? id : null;
            });

            // Test uniqueness (statistical - generate 1000 IDs, all should be unique)
            RunTest("1000 generated IDs are unique", () =>
            {
                HashSet<string> ids = new HashSet<string>();
                for (int i = 0; i < 1000; i++)
                {
                    ids.Add(generator.Generate());
                }
                return ids.Count == 1000;
            });

            Console.WriteLine();
        }

        static void TestPrefixGeneration()
        {
            Console.WriteLine("[Prefix Generation Tests]");

            IdGenerator generator = new IdGenerator();

            // Test with prefix
            RunTestWithSample("Generate with 'user_' prefix (32 total)", () =>
            {
                string id = generator.Generate("user_", 32);
                return id.Length == 32 && id.StartsWith("user_") ? id : null;
            });

            // Test prefix length calculation
            RunTestWithSample("Generate with 'data_' prefix (24 total)", () =>
            {
                string id = generator.Generate("data_", 24);
                return id.Length == 24 && id.StartsWith("data_") ? id : null;
            });

            // Test with long prefix
            RunTestWithSample("Generate with long prefix", () =>
            {
                string id = generator.Generate("very_long_prefix_", 32);
                return id.Length == 32 && id.StartsWith("very_long_prefix_") ? id : null;
            });

            // Test empty prefix
            RunTestWithSample("Generate with empty prefix", () =>
            {
                string id = generator.Generate("", 32);
                return id.Length == 32 ? id : null;
            });

            // Test null prefix
            RunTestWithSample("Generate with null prefix", () =>
            {
                string id = generator.Generate(null, 32);
                return id.Length == 32 ? id : null;
            });

            Console.WriteLine();
        }

        static void TestCustomCharacterSets()
        {
            Console.WriteLine("[Custom Character Set Tests]");

            // Test hexadecimal lowercase
            RunTestWithSample("Hexadecimal lowercase character set", () =>
            {
                IdGenerator generator = new IdGenerator(DefaultCharacterSets.HexadecimalLowercase);
                string id = generator.Generate(32);
                string validChars = "0123456789abcdef";
                return id.Length == 32 && id.All(c => validChars.Contains(c)) ? id : null;
            });

            // Test hexadecimal uppercase
            RunTestWithSample("Hexadecimal uppercase character set", () =>
            {
                IdGenerator generator = new IdGenerator(DefaultCharacterSets.HexadecimalUppercase);
                string id = generator.Generate(32);
                string validChars = "0123456789ABCDEF";
                return id.Length == 32 && id.All(c => validChars.Contains(c)) ? id : null;
            });

            // Test base64 character set
            RunTestWithSample("Base64 character set", () =>
            {
                IdGenerator generator = new IdGenerator(DefaultCharacterSets.Base64);
                string id = generator.Generate(32);
                string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789+/=";
                return id.Length == 32 && id.All(c => validChars.Contains(c)) ? id : null;
            });

            // Test URL-safe character set
            RunTestWithSample("URL-safe character set", () =>
            {
                IdGenerator generator = new IdGenerator(DefaultCharacterSets.UrlSafe);
                string id = generator.Generate(32);
                string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~";
                return id.Length == 32 && id.All(c => validChars.Contains(c)) ? id : null;
            });

            // Test numeric-only character set (too few chars, should throw)
            RunTest("Numeric-only character set throws due to insufficient entropy", () =>
            {
                try
                {
                    char[] numericChars = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
                    IdGenerator generator = new IdGenerator(numericChars);
                    return false; // Should have thrown
                }
                catch (ArgumentException)
                {
                    return true;
                }
            });

            Console.WriteLine();
        }

        static void TestUrlSafeGeneration()
        {
            Console.WriteLine("[URL-Safe Generation Tests]");

            IdGenerator generator = new IdGenerator();

            // Test URL-safe generation
            RunTestWithSample("GenerateUrlSafe produces valid characters", () =>
            {
                string id = generator.GenerateUrlSafe(null, 32);
                string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~";
                return id.Length == 32 && id.All(c => validChars.Contains(c)) ? id : null;
            });

            // Test URL-safe with prefix
            RunTestWithSample("GenerateUrlSafe with 'api_' prefix", () =>
            {
                string id = generator.GenerateUrlSafe("api_", 32);
                return id.Length == 32 && id.StartsWith("api_") ? id : null;
            });

            // Test URL-safe short codes
            RunTestWithSample("GenerateUrlSafe short code (12 chars)", () =>
            {
                string id = generator.GenerateUrlSafe(null, 12);
                return id.Length == 12 ? id : null;
            });

            Console.WriteLine();
        }

        static void TestKSortableGeneration()
        {
            Console.WriteLine("[K-Sortable Generation Tests]");

            IdGenerator generator = new IdGenerator();

            // Test k-sortable format
            RunTestWithSample("GenerateKSortable has timestamp prefix", () =>
            {
                string id = generator.GenerateKSortable(null, 48);
                string[] parts = id.Split('_');
                return parts.Length == 2 && parts[0].Length > 0 && parts[1].Length > 0 ? id : null;
            });

            // Test k-sortable with custom prefix
            RunTestWithSample("GenerateKSortable with 'order_' prefix", () =>
            {
                string id = generator.GenerateKSortable("order_", 48);
                return id.StartsWith("order_") && id.IndexOf('_', 6) > 0 ? id : null;
            });

            // Test k-sortable chronological ordering
            RunTest("GenerateKSortable IDs are chronologically ordered", () =>
            {
                string id1 = generator.GenerateKSortable(null, 48);
                System.Threading.Thread.Sleep(10);
                string id2 = generator.GenerateKSortable(null, 48);
                return string.CompareOrdinal(id1, id2) < 0;
            });

            // Test k-sortable total length
            RunTestWithSample("GenerateKSortable respects max length", () =>
            {
                string id = generator.GenerateKSortable("inv_", 64);
                return id.Length == 64 ? id : null;
            });

            Console.WriteLine();
        }

        static void TestBase64EncodedGeneration()
        {
            Console.WriteLine("[Base64-Encoded Generation Tests]");

            IdGenerator generator = new IdGenerator();

            // Test base64 encoding
            RunTestWithSample("GenerateAndEncodeBase64 produces valid base64", () =>
            {
                string id = generator.GenerateAndEncodeBase64(null, 48);
                string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789+/=";
                return id.All(c => validChars.Contains(c)) ? id : null;
            });

            // Test base64 with prefix
            RunTestWithSample("GenerateAndEncodeBase64 with 'b64_' prefix", () =>
            {
                string id = generator.GenerateAndEncodeBase64("b64_", 64);
                return id.Length == 64 && id.StartsWith("b64_") ? id : null;
            });

            // Test base64 padding compliance
            RunTestWithSample("GenerateAndEncodeBase64 has valid length", () =>
            {
                string id = generator.GenerateAndEncodeBase64(null, 48);
                return id.Length == 48 ? id : null;
            });

            Console.WriteLine();
        }

        static void TestCryptographicRngGeneration()
        {
            Console.WriteLine("[Cryptographic RNG Tests]");

            IdGenerator generator = new IdGenerator();
            generator.UseCryptographicRng = true;

            // Test cryptographic RNG generation
            RunTestWithSample("UseCryptographicRng generates valid IDs", () =>
            {
                string id = generator.Generate(32);
                return id.Length == 32 ? id : null;
            });

            // Test cryptographic RNG uniqueness
            RunTest("UseCryptographicRng produces unique IDs", () =>
            {
                HashSet<string> ids = new HashSet<string>();
                for (int i = 0; i < 100; i++)
                {
                    ids.Add(generator.Generate());
                }
                return ids.Count == 100;
            });

            // Test cryptographic RNG with prefix
            RunTestWithSample("UseCryptographicRng with 'secure_' prefix", () =>
            {
                string id = generator.Generate("secure_", 32);
                return id.Length == 32 && id.StartsWith("secure_") ? id : null;
            });

            Console.WriteLine();
        }

        static void TestConfigurationOptions()
        {
            Console.WriteLine("[Configuration Options Tests]");

            // Test MaximumLength property
            RunTest("MaximumLength property affects default generation", () =>
            {
                IdGenerator generator = new IdGenerator();
                generator.MaximumLength = 48;
                string id = generator.Generate();
                return id.Length == 48;
            });

            // Test ValidCharacters property
            RunTest("ValidCharacters property can be changed", () =>
            {
                IdGenerator generator = new IdGenerator();
                generator.ValidCharacters = new HashSet<char>(DefaultCharacterSets.HexadecimalLowercase);
                string id = generator.Generate(32);
                string validChars = "0123456789abcdef";
                return id.All(c => validChars.Contains(c));
            });

            // Test MaxIterations property
            RunTest("MaxIterations property can be set", () =>
            {
                IdGenerator generator = new IdGenerator();
                generator.MaxIterations = 128;
                string id = generator.Generate(32);
                return id.Length == 32;
            });

            Console.WriteLine();
        }

        static void TestEdgeCases()
        {
            Console.WriteLine("[Edge Case Tests]");

            // Test very short IDs
            RunTest("Generate 1 character ID", () =>
            {
                IdGenerator generator = new IdGenerator();
                string id = generator.Generate(1);
                return id.Length == 1;
            });

            // Test very long IDs
            RunTest("Generate 256 character ID", () =>
            {
                IdGenerator generator = new IdGenerator();
                string id = generator.Generate(256);
                return id.Length == 256;
            });

            // Test with very small character set (should throw due to insufficient entropy)
            RunTest("Generate with 2-character set throws due to insufficient entropy", () =>
            {
                try
                {
                    char[] twoChars = new char[] { 'A', 'B' };
                    IdGenerator generator = new IdGenerator(twoChars);
                    return false; // Should have thrown
                }
                catch (ArgumentException)
                {
                    return true;
                }
            });

            Console.WriteLine();
        }

        static void TestErrorHandling()
        {
            Console.WriteLine("[Error Handling Tests]");

            // Test invalid max length (0)
            RunTest("Generate with maxLen=0 throws exception", () =>
            {
                IdGenerator generator = new IdGenerator();
                try
                {
                    string id = generator.Generate(0);
                    return false; // Should have thrown
                }
                catch (ArgumentException)
                {
                    return true;
                }
            });

            // Test invalid max length (negative)
            RunTest("Generate with negative maxLen throws exception", () =>
            {
                IdGenerator generator = new IdGenerator();
                try
                {
                    string id = generator.Generate(-5);
                    return false; // Should have thrown
                }
                catch (ArgumentException)
                {
                    return true;
                }
            });

            // Test prefix longer than max length
            RunTest("Prefix longer than maxLen throws exception", () =>
            {
                IdGenerator generator = new IdGenerator();
                try
                {
                    string id = generator.Generate("this_is_a_very_long_prefix", 10);
                    return false; // Should have thrown
                }
                catch (ArgumentException)
                {
                    return true;
                }
            });

            // Test null character array in constructor
            RunTest("Null character array in constructor throws exception", () =>
            {
                try
                {
                    IdGenerator generator = new IdGenerator((char[])null);
                    return false; // Should have thrown
                }
                catch (ArgumentNullException)
                {
                    return true;
                }
            });

            // Test empty character array in constructor
            RunTest("Empty character array in constructor throws exception", () =>
            {
                try
                {
                    IdGenerator generator = new IdGenerator(new char[] { });
                    return false; // Should have thrown
                }
                catch (ArgumentException)
                {
                    return true;
                }
            });

            // Test null HashSet in constructor
            RunTest("Null HashSet in constructor throws exception", () =>
            {
                try
                {
                    IdGenerator generator = new IdGenerator((HashSet<char>)null);
                    return false; // Should have thrown
                }
                catch (ArgumentNullException)
                {
                    return true;
                }
            });

            // Test invalid MaximumLength property
            RunTest("Setting MaximumLength to 0 throws exception", () =>
            {
                IdGenerator generator = new IdGenerator();
                try
                {
                    generator.MaximumLength = 0;
                    return false; // Should have thrown
                }
                catch (ArgumentException)
                {
                    return true;
                }
            });

            // Test invalid MaxIterations property
            RunTest("Setting MaxIterations to 0 throws exception", () =>
            {
                IdGenerator generator = new IdGenerator();
                try
                {
                    generator.MaxIterations = 0;
                    return false; // Should have thrown
                }
                catch (ArgumentException)
                {
                    return true;
                }
            });

            // Test MaxIterations exceeding limit
            RunTest("Setting MaxIterations >256 throws exception", () =>
            {
                IdGenerator generator = new IdGenerator();
                try
                {
                    generator.MaxIterations = 300;
                    return false; // Should have thrown
                }
                catch (ArgumentException)
                {
                    return true;
                }
            });

            Console.WriteLine();
        }

        static void TestThreadSafety()
        {
            Console.WriteLine("[Thread Safety Tests]");

            // Concurrent generation from a single shared instance produces unique, valid IDs.
            RunTest("Concurrent Generate() across threads produces unique valid IDs", () =>
            {
                IdGenerator generator = new IdGenerator();
                ConcurrentBag<string> ids = new ConcurrentBag<string>();
                Parallel.For(0, 10000, i =>
                {
                    ids.Add(generator.Generate(32));
                });

                if (ids.Count != 10000) return false;
                if (ids.Any(id => id.Length != 32)) return false;
                HashSet<string> unique = new HashSet<string>(ids);
                return unique.Count == 10000;
            });

            // Concurrent generation with the cryptographic RNG path is also safe and unique.
            RunTest("Concurrent Generate() with UseCryptographicRng produces unique valid IDs", () =>
            {
                IdGenerator generator = new IdGenerator();
                generator.UseCryptographicRng = true;
                ConcurrentBag<string> ids = new ConcurrentBag<string>();
                Parallel.For(0, 10000, i =>
                {
                    ids.Add(generator.Generate(32));
                });

                if (ids.Count != 10000) return false;
                if (ids.Any(id => id.Length != 32)) return false;
                HashSet<string> unique = new HashSet<string>(ids);
                return unique.Count == 10000;
            });

            // Mixed concurrent use of the thread-safe helper methods does not corrupt output.
            RunTest("Concurrent mixed generation methods produce correctly-shaped IDs", () =>
            {
                IdGenerator generator = new IdGenerator();
                ConcurrentBag<bool> results = new ConcurrentBag<bool>();
                Parallel.For(0, 5000, i =>
                {
                    string urlSafe = generator.GenerateUrlSafe(null, 24);
                    string kSortable = generator.GenerateKSortable(null, 48);
                    string b64 = generator.GenerateAndEncodeBase64(null, 48);
                    results.Add(urlSafe.Length == 24 && kSortable.Length == 48 && b64.Length == 48);
                });

                return results.Count == 5000 && results.All(r => r);
            });

            Console.WriteLine();
        }

        static void TestAdditionalCoverage()
        {
            Console.WriteLine("[Additional Coverage Tests]");

            // --- Positive cases ---

            // Constructor with char[] and custom maximum length drives parameterless Generate().
            RunTestWithSample("char[] constructor honors custom maximumLength", () =>
            {
                char[] chars = "abcdefghijklmnop".ToCharArray();
                IdGenerator generator = new IdGenerator(chars, 20);
                string id = generator.Generate();
                return id.Length == 20 && id.All(c => chars.Contains(c)) ? id : null;
            });

            // Constructor with HashSet and custom maximum length drives parameterless Generate().
            RunTestWithSample("HashSet constructor honors custom maximumLength", () =>
            {
                HashSet<char> chars = new HashSet<char>(DefaultCharacterSets.HexadecimalLowercase);
                IdGenerator generator = new IdGenerator(chars, 40);
                string id = generator.Generate();
                return id.Length == 40 ? id : null;
            });

            // GenerateUrlSafe with all default parameters yields a 32-char URL-safe ID.
            RunTestWithSample("GenerateUrlSafe with default parameters", () =>
            {
                IdGenerator generator = new IdGenerator();
                string id = generator.GenerateUrlSafe();
                string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~";
                return id.Length == 32 && id.All(c => validChars.Contains(c)) ? id : null;
            });

            // GenerateAndEncodeBase64 output decodes back to valid bytes (true base64 round-trip).
            RunTestWithSample("GenerateAndEncodeBase64 output is decodable base64", () =>
            {
                IdGenerator generator = new IdGenerator();
                string id = generator.GenerateAndEncodeBase64(null, 48);
                byte[] decoded = Convert.FromBase64String(id);
                return decoded.Length > 0 ? id : null;
            });

            // The random portion covers the entire character set over enough draws (no dead chars).
            RunTest("Generation exercises every character in a hex set", () =>
            {
                IdGenerator generator = new IdGenerator(DefaultCharacterSets.HexadecimalLowercase);
                HashSet<char> seen = new HashSet<char>();
                for (int i = 0; i < 200; i++)
                {
                    foreach (char c in generator.Generate(32))
                        seen.Add(c);
                }
                string expected = "0123456789abcdef";
                return expected.All(c => seen.Contains(c));
            });

            // A GUID-based and a crypto-RNG generator using the same charset both stay in-alphabet.
            RunTest("Both RNG modes respect the configured character set", () =>
            {
                IdGenerator guidGen = new IdGenerator(DefaultCharacterSets.UrlSafe);
                IdGenerator cryptoGen = new IdGenerator(DefaultCharacterSets.UrlSafe);
                cryptoGen.UseCryptographicRng = true;
                string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~";
                string a = guidGen.Generate(64);
                string b = cryptoGen.Generate(64);
                return a.All(c => validChars.Contains(c)) && b.All(c => validChars.Contains(c));
            });

            // ValidCharacters setter accepts a valid replacement set and it takes effect.
            RunTest("ValidCharacters setter replaces the active character set", () =>
            {
                IdGenerator generator = new IdGenerator();
                generator.ValidCharacters = new HashSet<char>(DefaultCharacterSets.HexadecimalUppercase);
                string id = generator.Generate(32);
                string validChars = "0123456789ABCDEF";
                return id.All(c => validChars.Contains(c));
            });

            // --- Negative cases ---

            // GenerateUrlSafe rejects a prefix that is longer than the requested length.
            RunTest("GenerateUrlSafe with prefix longer than maxLen throws", () =>
            {
                IdGenerator generator = new IdGenerator();
                try
                {
                    generator.GenerateUrlSafe("this_prefix_is_too_long", 10);
                    return false;
                }
                catch (ArgumentException)
                {
                    return true;
                }
            });

            // GenerateUrlSafe rejects a non-positive maximum length.
            RunTest("GenerateUrlSafe with maxLen=0 throws", () =>
            {
                IdGenerator generator = new IdGenerator();
                try
                {
                    generator.GenerateUrlSafe(null, 0);
                    return false;
                }
                catch (ArgumentException)
                {
                    return true;
                }
            });

            // GenerateKSortable rejects a length too small to hold the timestamp prefix.
            RunTest("GenerateKSortable with too-short maxLen throws", () =>
            {
                IdGenerator generator = new IdGenerator();
                try
                {
                    generator.GenerateKSortable(null, 4);
                    return false;
                }
                catch (ArgumentException)
                {
                    return true;
                }
            });

            // GenerateAndEncodeBase64 rejects a non-positive maximum length.
            RunTest("GenerateAndEncodeBase64 with maxLen=0 throws", () =>
            {
                IdGenerator generator = new IdGenerator();
                try
                {
                    generator.GenerateAndEncodeBase64(null, 0);
                    return false;
                }
                catch (ArgumentException)
                {
                    return true;
                }
            });

            // GenerateAndEncodeBase64 requires at least 4 chars of headroom beyond the prefix.
            RunTest("GenerateAndEncodeBase64 with prefix too close to maxLen throws", () =>
            {
                IdGenerator generator = new IdGenerator();
                try
                {
                    generator.GenerateAndEncodeBase64("prefix_", 8);
                    return false;
                }
                catch (ArgumentException)
                {
                    return true;
                }
            });

            // ValidCharacters setter rejects null.
            RunTest("Setting ValidCharacters to null throws ArgumentNullException", () =>
            {
                IdGenerator generator = new IdGenerator();
                try
                {
                    generator.ValidCharacters = null;
                    return false;
                }
                catch (ArgumentNullException)
                {
                    return true;
                }
            });

            // ValidCharacters setter rejects an empty set.
            RunTest("Setting ValidCharacters to empty set throws ArgumentException", () =>
            {
                IdGenerator generator = new IdGenerator();
                try
                {
                    generator.ValidCharacters = new HashSet<char>();
                    return false;
                }
                catch (ArgumentException)
                {
                    return true;
                }
            });

            // MaxIterations setter rejects negative values.
            RunTest("Setting MaxIterations to a negative value throws", () =>
            {
                IdGenerator generator = new IdGenerator();
                try
                {
                    generator.MaxIterations = -1;
                    return false;
                }
                catch (ArgumentException)
                {
                    return true;
                }
            });

            // char[] constructor rejects a non-positive maximum length.
            RunTest("char[] constructor with maximumLength=0 throws", () =>
            {
                try
                {
                    IdGenerator generator = new IdGenerator(DefaultCharacterSets.EnglishAlphanumeric.ToArray(), 0);
                    return false;
                }
                catch (ArgumentException)
                {
                    return true;
                }
            });

            // HashSet constructor rejects a non-positive maximum length.
            RunTest("HashSet constructor with maximumLength=0 throws", () =>
            {
                try
                {
                    IdGenerator generator = new IdGenerator(new HashSet<char>(DefaultCharacterSets.EnglishAlphanumeric), 0);
                    return false;
                }
                catch (ArgumentException)
                {
                    return true;
                }
            });

            Console.WriteLine();
        }

        static void RunTest(string testName, Func<bool> testFunc)
        {
            _totalTests++;
            try
            {
                bool result = testFunc();
                if (result)
                {
                    _passedTests++;
                    Console.WriteLine($"  ✓ PASS: {testName}");
                }
                else
                {
                    _failedTests++;
                    Console.WriteLine($"  ✗ FAIL: {testName}");
                }
            }
            catch (Exception ex)
            {
                _failedTests++;
                Console.WriteLine($"  ✗ FAIL: {testName} (Exception: {ex.Message})");
            }
        }

        static void RunTestWithSample(string testName, Func<string> testFunc)
        {
            _totalTests++;
            try
            {
                string sample = testFunc();
                if (sample != null)
                {
                    _passedTests++;
                    Console.WriteLine($"  ✓ PASS: {testName}");
                    Console.WriteLine($"          Sample: {sample}");
                }
                else
                {
                    _failedTests++;
                    Console.WriteLine($"  ✗ FAIL: {testName}");
                }
            }
            catch (Exception ex)
            {
                _failedTests++;
                Console.WriteLine($"  ✗ FAIL: {testName} (Exception: {ex.Message})");
            }
        }
    }
}
