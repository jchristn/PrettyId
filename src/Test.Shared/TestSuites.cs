using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PrettyId;
using Touchstone.Core;

namespace Test.Shared
{
    /// <summary>
    /// Central source of truth for the PrettyId test suite. Every test case is defined once here
    /// as a Touchstone descriptor and executed through the console runner (Test.Automated),
    /// the xUnit adapter (Test.Xunit), and the NUnit adapter (Test.Nunit).
    /// </summary>
    public static class TestSuites
    {
        #region Public-Members

        /// <summary>
        /// All test suites exposed to every Touchstone runner.
        /// </summary>
        public static IReadOnlyList<TestSuiteDescriptor> All { get; } = Build();

        #endregion

        #region Build

        private static IReadOnlyList<TestSuiteDescriptor> Build()
        {
            List<TestSuiteDescriptor> suites = new List<TestSuiteDescriptor>();
            suites.Add(ConstructionSuite());
            suites.Add(BasicGenerationSuite());
            suites.Add(PrefixGenerationSuite());
            suites.Add(UrlSafeSuite());
            suites.Add(KSortableSuite());
            suites.Add(Base64Suite());
            suites.Add(RngModeSuite());
            suites.Add(ConfigurationSuite());
            suites.Add(DefaultCharacterSetsSuite());
            suites.Add(EdgeCaseSuite());
            suites.Add(ErrorHandlingSuite());
            suites.Add(ThreadSafetySuite());
            suites.Add(DistributionSuite());
            return suites;
        }

        #endregion

        #region Suite-Builders

        private static TestSuiteDescriptor ConstructionSuite()
        {
            const string s = "Construction";
            List<TestCaseDescriptor> cases = new List<TestCaseDescriptor>();

            cases.Add(Case(s, "default-charset", "Default constructor uses English alphanumeric (62 chars)", () =>
            {
                IdGenerator g = new IdGenerator();
                Assert.Equal(62, g.ValidCharacters.Count, "default valid character count");
                foreach (char c in DefaultCharacterSets.EnglishAlphanumeric)
                    Assert.True(g.ValidCharacters.Contains(c), "default set contains " + c);
            }));

            cases.Add(Case(s, "default-maxlength", "Default constructor sets MaximumLength to 32", () =>
            {
                IdGenerator g = new IdGenerator();
                Assert.Equal(32, g.MaximumLength, "default MaximumLength");
            }));

            cases.Add(Case(s, "default-crypto-false", "Default constructor leaves UseCryptographicRng false", () =>
            {
                IdGenerator g = new IdGenerator();
                Assert.False(g.UseCryptographicRng, "default UseCryptographicRng");
            }));

            cases.Add(Case(s, "chararray-valid", "char[] constructor accepts a valid 16+ character set", () =>
            {
                IdGenerator g = new IdGenerator("0123456789abcdef".ToCharArray());
                string id = g.Generate(32);
                Assert.Length(32, id, "generated length");
                Assert.AllCharsIn(id, CharSets.HexLower, "hex chars only");
            }));

            cases.Add(Case(s, "chararray-custom-maxlength", "char[] constructor honors custom maximumLength via Generate()", () =>
            {
                char[] chars = "abcdefghijklmnop".ToCharArray();
                IdGenerator g = new IdGenerator(chars, 20);
                string id = g.Generate();
                Assert.Length(20, id, "generated length equals custom maximumLength");
                Assert.AllCharsIn(id, new HashSet<char>(chars), "custom chars only");
            }));

            cases.Add(Case(s, "hashset-valid", "HashSet constructor accepts a valid 16+ character set", () =>
            {
                IdGenerator g = new IdGenerator(new HashSet<char>(DefaultCharacterSets.HexadecimalUppercase));
                string id = g.Generate(32);
                Assert.Length(32, id, "generated length");
                Assert.AllCharsIn(id, CharSets.HexUpper, "hex upper chars only");
            }));

            cases.Add(Case(s, "hashset-custom-maxlength", "HashSet constructor honors custom maximumLength via Generate()", () =>
            {
                IdGenerator g = new IdGenerator(new HashSet<char>(DefaultCharacterSets.HexadecimalLowercase), 40);
                string id = g.Generate();
                Assert.Length(40, id, "generated length equals custom maximumLength");
            }));

            cases.Add(Case(s, "chararray-exactly-16", "char[] constructor accepts exactly 16 unique characters (boundary)", () =>
            {
                IdGenerator g = new IdGenerator("0123456789abcdef".ToCharArray());
                Assert.Equal(16, g.ValidCharacters.Count, "16 unique chars accepted");
            }));

            cases.Add(Case(s, "chararray-15-throws", "char[] constructor with 15 unique characters throws (boundary)", () =>
            {
                Assert.Throws<ArgumentException>(
                    () => { IdGenerator g = new IdGenerator("0123456789abcde".ToCharArray()); },
                    "15 unique chars rejected");
            }));

            cases.Add(Case(s, "chararray-dupes-below-16-throws", "char[] constructor with duplicates reducing unique count below 16 throws", () =>
            {
                char[] chars = "aabbccddeeffgghh".ToCharArray(); // 8 unique
                Assert.Throws<ArgumentException>(
                    () => { IdGenerator g = new IdGenerator(chars); },
                    "duplicate-inflated set with 8 unique chars rejected");
            }));

            cases.Add(Case(s, "chararray-null-throws", "char[] constructor with null throws ArgumentNullException", () =>
            {
                Assert.Throws<ArgumentNullException>(
                    () => { IdGenerator g = new IdGenerator((char[])null); },
                    "null char[] rejected");
            }));

            cases.Add(Case(s, "chararray-empty-throws", "char[] constructor with empty array throws ArgumentException", () =>
            {
                Assert.Throws<ArgumentException>(
                    () => { IdGenerator g = new IdGenerator(new char[] { }); },
                    "empty char[] rejected");
            }));

            cases.Add(Case(s, "chararray-maxlength-zero-throws", "char[] constructor with maximumLength 0 throws", () =>
            {
                Assert.Throws<ArgumentException>(
                    () => { IdGenerator g = new IdGenerator(DefaultCharacterSets.EnglishAlphanumeric, 0); },
                    "maximumLength 0 rejected");
            }));

            cases.Add(Case(s, "chararray-maxlength-negative-throws", "char[] constructor with negative maximumLength throws", () =>
            {
                Assert.Throws<ArgumentException>(
                    () => { IdGenerator g = new IdGenerator(DefaultCharacterSets.EnglishAlphanumeric, -5); },
                    "negative maximumLength rejected");
            }));

            cases.Add(Case(s, "hashset-null-throws", "HashSet constructor with null throws ArgumentNullException", () =>
            {
                Assert.Throws<ArgumentNullException>(
                    () => { IdGenerator g = new IdGenerator((HashSet<char>)null); },
                    "null HashSet rejected");
            }));

            cases.Add(Case(s, "hashset-empty-throws", "HashSet constructor with empty set throws ArgumentException", () =>
            {
                Assert.Throws<ArgumentException>(
                    () => { IdGenerator g = new IdGenerator(new HashSet<char>()); },
                    "empty HashSet rejected");
            }));

            cases.Add(Case(s, "hashset-below-16-throws", "HashSet constructor with fewer than 16 characters throws", () =>
            {
                Assert.Throws<ArgumentException>(
                    () => { IdGenerator g = new IdGenerator(new HashSet<char>("abcdef".ToCharArray())); },
                    "6-char HashSet rejected");
            }));

            cases.Add(Case(s, "hashset-maxlength-zero-throws", "HashSet constructor with maximumLength 0 throws", () =>
            {
                Assert.Throws<ArgumentException>(
                    () => { IdGenerator g = new IdGenerator(new HashSet<char>(DefaultCharacterSets.EnglishAlphanumeric), 0); },
                    "maximumLength 0 rejected");
            }));

            return Suite(s, "Construction and Validation", cases);
        }

        private static TestSuiteDescriptor BasicGenerationSuite()
        {
            const string s = "BasicGeneration";
            List<TestCaseDescriptor> cases = new List<TestCaseDescriptor>();

            cases.Add(Case(s, "default-length-32", "Generate() produces a 32 character ID", () =>
            {
                IdGenerator g = new IdGenerator();
                Assert.Length(32, g.Generate(), "default length");
            }));

            int[] lengths = new int[] { 1, 2, 8, 16, 32, 64, 128, 256, 512, 1024 };
            foreach (int len in lengths)
            {
                int captured = len;
                cases.Add(Case(s, "length-" + captured, "Generate(" + captured + ") produces an ID of that length", () =>
                {
                    IdGenerator g = new IdGenerator();
                    string id = g.Generate(captured);
                    Assert.Length(captured, id, "requested length " + captured);
                    Assert.AllCharsIn(id, CharSets.Alphanumeric, "alphanumeric only");
                }));
            }

            cases.Add(Case(s, "alphanumeric-only", "Generated IDs contain only alphanumeric characters", () =>
            {
                IdGenerator g = new IdGenerator();
                for (int i = 0; i < 50; i++)
                    Assert.AllCharsIn(g.Generate(), CharSets.Alphanumeric, "alphanumeric only");
            }));

            cases.Add(Case(s, "uniqueness-1000", "1000 generated IDs are all unique", () =>
            {
                IdGenerator g = new IdGenerator();
                HashSet<string> ids = new HashSet<string>();
                for (int i = 0; i < 1000; i++)
                    ids.Add(g.Generate());
                Assert.Equal(1000, ids.Count, "unique count");
            }));

            cases.Add(Case(s, "maxlen-zero-throws", "Generate(0) throws ArgumentException", () =>
            {
                IdGenerator g = new IdGenerator();
                Assert.Throws<ArgumentException>(() => g.Generate(0), "length 0 rejected");
            }));

            cases.Add(Case(s, "maxlen-negative-throws", "Generate(-1) throws ArgumentException", () =>
            {
                IdGenerator g = new IdGenerator();
                Assert.Throws<ArgumentException>(() => g.Generate(-1), "negative length rejected");
            }));

            return Suite(s, "Basic Generation", cases);
        }

        private static TestSuiteDescriptor PrefixGenerationSuite()
        {
            const string s = "PrefixGeneration";
            List<TestCaseDescriptor> cases = new List<TestCaseDescriptor>();

            cases.Add(Case(s, "with-prefix", "Generate with 'user_' prefix yields correct length and prefix", () =>
            {
                IdGenerator g = new IdGenerator();
                string id = g.Generate("user_", 32);
                Assert.Length(32, id, "total length includes prefix");
                Assert.StartsWith("user_", id, "prefix preserved");
                Assert.AllCharsIn(id.Substring(5), CharSets.Alphanumeric, "random portion alphanumeric");
            }));

            cases.Add(Case(s, "prefix-reduces-random", "Prefix counts toward the total maximum length", () =>
            {
                IdGenerator g = new IdGenerator();
                string id = g.Generate("data_", 24);
                Assert.Length(24, id, "total length");
                Assert.StartsWith("data_", id, "prefix preserved");
            }));

            cases.Add(Case(s, "long-prefix", "Generate with a long prefix keeps prefix and length", () =>
            {
                IdGenerator g = new IdGenerator();
                string id = g.Generate("very_long_prefix_", 32);
                Assert.Length(32, id, "total length");
                Assert.StartsWith("very_long_prefix_", id, "prefix preserved");
            }));

            cases.Add(Case(s, "empty-prefix", "Generate with empty prefix behaves like no prefix", () =>
            {
                IdGenerator g = new IdGenerator();
                string id = g.Generate("", 32);
                Assert.Length(32, id, "length");
                Assert.AllCharsIn(id, CharSets.Alphanumeric, "alphanumeric only");
            }));

            cases.Add(Case(s, "null-prefix", "Generate with null prefix behaves like no prefix", () =>
            {
                IdGenerator g = new IdGenerator();
                string id = g.Generate(null, 32);
                Assert.Length(32, id, "length");
            }));

            cases.Add(Case(s, "prefix-one-less-than-maxlen", "Prefix of length maxLen-1 leaves a single random character", () =>
            {
                IdGenerator g = new IdGenerator();
                string id = g.Generate("abc", 4);
                Assert.Length(4, id, "length");
                Assert.StartsWith("abc", id, "prefix preserved");
            }));

            cases.Add(Case(s, "prefix-equals-maxlen-throws", "Prefix length equal to maxLen throws (no room for randomness)", () =>
            {
                IdGenerator g = new IdGenerator();
                Assert.Throws<ArgumentException>(() => g.Generate("abcd", 4), "prefix == maxLen rejected");
            }));

            cases.Add(Case(s, "prefix-longer-than-maxlen-throws", "Prefix longer than maxLen throws", () =>
            {
                IdGenerator g = new IdGenerator();
                Assert.Throws<ArgumentException>(
                    () => g.Generate("this_is_a_very_long_prefix", 10),
                    "prefix longer than maxLen rejected");
            }));

            return Suite(s, "Prefix Generation", cases);
        }

        private static TestSuiteDescriptor UrlSafeSuite()
        {
            const string s = "UrlSafe";
            List<TestCaseDescriptor> cases = new List<TestCaseDescriptor>();

            cases.Add(Case(s, "default-params", "GenerateUrlSafe() with defaults yields 32 URL-safe characters", () =>
            {
                IdGenerator g = new IdGenerator();
                string id = g.GenerateUrlSafe();
                Assert.Length(32, id, "default length");
                Assert.AllCharsIn(id, CharSets.UrlSafe, "url-safe chars only");
            }));

            cases.Add(Case(s, "valid-chars", "GenerateUrlSafe produces only URL-safe characters", () =>
            {
                IdGenerator g = new IdGenerator();
                for (int i = 0; i < 50; i++)
                    Assert.AllCharsIn(g.GenerateUrlSafe(null, 32), CharSets.UrlSafe, "url-safe chars only");
            }));

            cases.Add(Case(s, "with-prefix", "GenerateUrlSafe with 'api_' prefix keeps prefix and length", () =>
            {
                IdGenerator g = new IdGenerator();
                string id = g.GenerateUrlSafe("api_", 32);
                Assert.Length(32, id, "length");
                Assert.StartsWith("api_", id, "prefix preserved");
            }));

            cases.Add(Case(s, "short-code", "GenerateUrlSafe short code (12 chars)", () =>
            {
                IdGenerator g = new IdGenerator();
                Assert.Length(12, g.GenerateUrlSafe(null, 12), "short length");
            }));

            cases.Add(Case(s, "no-instance-mutation", "GenerateUrlSafe does not mutate the instance character set", () =>
            {
                IdGenerator g = new IdGenerator();
                g.GenerateUrlSafe(null, 32);
                for (int i = 0; i < 50; i++)
                    Assert.AllCharsIn(g.Generate(64), CharSets.Alphanumeric, "Generate still alphanumeric after UrlSafe call");
            }));

            cases.Add(Case(s, "maxlen-zero-throws", "GenerateUrlSafe with maxLen 0 throws", () =>
            {
                IdGenerator g = new IdGenerator();
                Assert.Throws<ArgumentException>(() => g.GenerateUrlSafe(null, 0), "maxLen 0 rejected");
            }));

            cases.Add(Case(s, "prefix-too-long-throws", "GenerateUrlSafe with prefix longer than maxLen throws", () =>
            {
                IdGenerator g = new IdGenerator();
                Assert.Throws<ArgumentException>(
                    () => g.GenerateUrlSafe("this_prefix_is_too_long", 10),
                    "over-long prefix rejected");
            }));

            return Suite(s, "URL-Safe Generation", cases);
        }

        private static TestSuiteDescriptor KSortableSuite()
        {
            const string s = "KSortable";
            List<TestCaseDescriptor> cases = new List<TestCaseDescriptor>();

            cases.Add(Case(s, "timestamp-prefix", "GenerateKSortable has a timestamp segment and a random segment", () =>
            {
                IdGenerator g = new IdGenerator();
                string id = g.GenerateKSortable(null, 48);
                string[] parts = id.Split('_');
                Assert.Equal(2, parts.Length, "two underscore-separated segments");
                Assert.NotEmpty(parts[0], "timestamp segment");
                Assert.NotEmpty(parts[1], "random segment");
                Assert.AllCharsIn(parts[0], CharSets.Base36, "timestamp is base36");
                Assert.AllCharsIn(parts[1], CharSets.Alphanumeric, "random is alphanumeric");
            }));

            cases.Add(Case(s, "default-maxlen", "GenerateKSortable() with default length 32 succeeds", () =>
            {
                IdGenerator g = new IdGenerator();
                string id = g.GenerateKSortable();
                Assert.Length(32, id, "default length");
            }));

            cases.Add(Case(s, "with-prefix", "GenerateKSortable with 'order_' prefix keeps prefix", () =>
            {
                IdGenerator g = new IdGenerator();
                string id = g.GenerateKSortable("order_", 48);
                Assert.StartsWith("order_", id, "prefix preserved");
                string[] parts = id.Split('_');
                Assert.Equal(3, parts.Length, "prefix, timestamp, and random segments");
                Assert.AllCharsIn(parts[1], CharSets.Base36, "timestamp is base36");
                Assert.AllCharsIn(parts[2], CharSets.Alphanumeric, "random is alphanumeric");
            }));

            cases.Add(Case(s, "respects-maxlen", "GenerateKSortable respects the maximum length", () =>
            {
                IdGenerator g = new IdGenerator();
                Assert.Length(64, g.GenerateKSortable("inv_", 64), "total length");
            }));

            cases.Add(Case(s, "chronological-order", "GenerateKSortable timestamps are non-decreasing over time", () =>
            {
                IdGenerator g = new IdGenerator();
                string id1 = g.GenerateKSortable(null, 48);
                Thread.Sleep(5);
                string id2 = g.GenerateKSortable(null, 48);
                long t1 = CharSets.FromBase36(id1.Split('_')[0]);
                long t2 = CharSets.FromBase36(id2.Split('_')[0]);
                Assert.True(t2 >= t1, "second timestamp not earlier than first");
            }));

            cases.Add(Case(s, "too-short-throws", "GenerateKSortable with too-short maxLen throws", () =>
            {
                IdGenerator g = new IdGenerator();
                Assert.Throws<ArgumentException>(() => g.GenerateKSortable(null, 4), "maxLen too short rejected");
            }));

            return Suite(s, "K-Sortable Generation", cases);
        }

        private static TestSuiteDescriptor Base64Suite()
        {
            const string s = "Base64";
            List<TestCaseDescriptor> cases = new List<TestCaseDescriptor>();

            cases.Add(Case(s, "valid-base64-chars", "GenerateAndEncodeBase64 produces only base64 alphabet characters", () =>
            {
                IdGenerator g = new IdGenerator();
                string id = g.GenerateAndEncodeBase64(null, 48);
                Assert.AllCharsIn(id, CharSets.Base64, "base64 chars only");
            }));

            cases.Add(Case(s, "decodable", "GenerateAndEncodeBase64 output round-trips through Convert.FromBase64String", () =>
            {
                IdGenerator g = new IdGenerator();
                string id = g.GenerateAndEncodeBase64(null, 48);
                byte[] decoded = Convert.FromBase64String(id);
                Assert.True(decoded.Length > 0, "decoded to non-empty bytes");
            }));

            cases.Add(Case(s, "length-null-prefix", "GenerateAndEncodeBase64 length equals maxLen (divisible by 4)", () =>
            {
                IdGenerator g = new IdGenerator();
                Assert.Length(48, g.GenerateAndEncodeBase64(null, 48), "length");
            }));

            cases.Add(Case(s, "with-prefix", "GenerateAndEncodeBase64 with 'b64_' prefix keeps prefix and length", () =>
            {
                IdGenerator g = new IdGenerator();
                string id = g.GenerateAndEncodeBase64("b64_", 64);
                Assert.Length(64, id, "length");
                Assert.StartsWith("b64_", id, "prefix preserved");
                byte[] decoded = Convert.FromBase64String(id.Substring(4));
                Assert.True(decoded.Length > 0, "decoded to non-empty bytes");
            }));

            cases.Add(Case(s, "maxlen-zero-throws", "GenerateAndEncodeBase64 with maxLen 0 throws", () =>
            {
                IdGenerator g = new IdGenerator();
                Assert.Throws<ArgumentException>(() => g.GenerateAndEncodeBase64(null, 0), "maxLen 0 rejected");
            }));

            cases.Add(Case(s, "prefix-insufficient-headroom-throws", "GenerateAndEncodeBase64 requires 4+ chars of headroom beyond the prefix", () =>
            {
                IdGenerator g = new IdGenerator();
                Assert.Throws<ArgumentException>(() => g.GenerateAndEncodeBase64("prefix_", 8), "insufficient headroom rejected");
            }));

            return Suite(s, "Base64-Encoded Generation", cases);
        }

        private static TestSuiteDescriptor RngModeSuite()
        {
            const string s = "RngMode";
            List<TestCaseDescriptor> cases = new List<TestCaseDescriptor>();

            cases.Add(Case(s, "crypto-valid-length", "UseCryptographicRng generates IDs of the requested length", () =>
            {
                IdGenerator g = new IdGenerator();
                g.UseCryptographicRng = true;
                Assert.Length(32, g.Generate(32), "length");
            }));

            cases.Add(Case(s, "crypto-uniqueness", "UseCryptographicRng produces unique IDs", () =>
            {
                IdGenerator g = new IdGenerator();
                g.UseCryptographicRng = true;
                HashSet<string> ids = new HashSet<string>();
                for (int i = 0; i < 500; i++)
                    ids.Add(g.Generate());
                Assert.Equal(500, ids.Count, "unique count");
            }));

            cases.Add(Case(s, "crypto-with-prefix", "UseCryptographicRng honors prefixes", () =>
            {
                IdGenerator g = new IdGenerator();
                g.UseCryptographicRng = true;
                string id = g.Generate("secure_", 32);
                Assert.Length(32, id, "length");
                Assert.StartsWith("secure_", id, "prefix preserved");
            }));

            cases.Add(Case(s, "both-modes-respect-charset", "Both GUID and crypto RNG modes stay within the configured character set", () =>
            {
                IdGenerator guidGen = new IdGenerator(DefaultCharacterSets.UrlSafe);
                IdGenerator cryptoGen = new IdGenerator(DefaultCharacterSets.UrlSafe);
                cryptoGen.UseCryptographicRng = true;
                Assert.AllCharsIn(guidGen.Generate(64), CharSets.UrlSafe, "guid mode url-safe");
                Assert.AllCharsIn(cryptoGen.Generate(64), CharSets.UrlSafe, "crypto mode url-safe");
            }));

            return Suite(s, "Random Number Generation Modes", cases);
        }

        private static TestSuiteDescriptor ConfigurationSuite()
        {
            const string s = "Configuration";
            List<TestCaseDescriptor> cases = new List<TestCaseDescriptor>();

            cases.Add(Case(s, "maxlength-affects-default", "MaximumLength property drives parameterless Generate()", () =>
            {
                IdGenerator g = new IdGenerator();
                g.MaximumLength = 48;
                Assert.Length(48, g.Generate(), "length follows MaximumLength");
            }));

            cases.Add(Case(s, "maxlength-one", "MaximumLength of 1 is accepted (boundary)", () =>
            {
                IdGenerator g = new IdGenerator();
                g.MaximumLength = 1;
                Assert.Length(1, g.Generate(), "length 1");
            }));

            cases.Add(Case(s, "maxlength-zero-throws", "Setting MaximumLength to 0 throws", () =>
            {
                IdGenerator g = new IdGenerator();
                Assert.Throws<ArgumentException>(() => g.MaximumLength = 0, "MaximumLength 0 rejected");
            }));

            cases.Add(Case(s, "maxlength-negative-throws", "Setting MaximumLength to a negative value throws", () =>
            {
                IdGenerator g = new IdGenerator();
                Assert.Throws<ArgumentException>(() => g.MaximumLength = -3, "negative MaximumLength rejected");
            }));

            cases.Add(Case(s, "validchars-replace", "ValidCharacters setter replaces the active character set", () =>
            {
                IdGenerator g = new IdGenerator();
                g.ValidCharacters = new HashSet<char>(DefaultCharacterSets.HexadecimalUppercase);
                Assert.AllCharsIn(g.Generate(32), CharSets.HexUpper, "hex upper only after replacement");
            }));

            cases.Add(Case(s, "validchars-small-set-allowed", "ValidCharacters setter accepts a small set (no 16-char minimum on the setter)", () =>
            {
                IdGenerator g = new IdGenerator();
                g.ValidCharacters = new HashSet<char>(new char[] { 'x', 'y' });
                string id = g.Generate(20);
                Assert.Length(20, id, "length");
                Assert.AllCharsIn(id, new HashSet<char>(new char[] { 'x', 'y' }), "only x and y");
            }));

            cases.Add(Case(s, "validchars-null-throws", "Setting ValidCharacters to null throws ArgumentNullException", () =>
            {
                IdGenerator g = new IdGenerator();
                Assert.Throws<ArgumentNullException>(() => g.ValidCharacters = null, "null rejected");
            }));

            cases.Add(Case(s, "validchars-empty-throws", "Setting ValidCharacters to an empty set throws ArgumentException", () =>
            {
                IdGenerator g = new IdGenerator();
                Assert.Throws<ArgumentException>(() => g.ValidCharacters = new HashSet<char>(), "empty rejected");
            }));

            cases.Add(Case(s, "maxiterations-set", "MaxIterations can be set within range and does not affect output shape", () =>
            {
                IdGenerator g = new IdGenerator();
                g.MaxIterations = 128;
                Assert.Length(32, g.Generate(32), "length unaffected by MaxIterations");
            }));

            cases.Add(Case(s, "maxiterations-min-boundary", "MaxIterations of 1 is accepted (boundary)", () =>
            {
                IdGenerator g = new IdGenerator();
                g.MaxIterations = 1;
                Assert.Length(32, g.Generate(32), "generation succeeds");
            }));

            cases.Add(Case(s, "maxiterations-max-boundary", "MaxIterations of 256 is accepted (boundary)", () =>
            {
                IdGenerator g = new IdGenerator();
                g.MaxIterations = 256;
                Assert.Length(32, g.Generate(32), "generation succeeds");
            }));

            cases.Add(Case(s, "maxiterations-zero-throws", "Setting MaxIterations to 0 throws", () =>
            {
                IdGenerator g = new IdGenerator();
                Assert.Throws<ArgumentException>(() => g.MaxIterations = 0, "0 rejected");
            }));

            cases.Add(Case(s, "maxiterations-negative-throws", "Setting MaxIterations to a negative value throws", () =>
            {
                IdGenerator g = new IdGenerator();
                Assert.Throws<ArgumentException>(() => g.MaxIterations = -1, "negative rejected");
            }));

            cases.Add(Case(s, "maxiterations-over-256-throws", "Setting MaxIterations above 256 throws", () =>
            {
                IdGenerator g = new IdGenerator();
                Assert.Throws<ArgumentException>(() => g.MaxIterations = 257, "257 rejected");
            }));

            return Suite(s, "Configuration Options", cases);
        }

        private static TestSuiteDescriptor DefaultCharacterSetsSuite()
        {
            const string s = "DefaultCharacterSets";
            List<TestCaseDescriptor> cases = new List<TestCaseDescriptor>();

            cases.Add(Case(s, "alphanumeric-count", "EnglishAlphanumeric has 62 unique characters", () =>
            {
                Assert.Equal(62, DefaultCharacterSets.EnglishAlphanumeric.Length, "array length");
                Assert.Equal(62, new HashSet<char>(DefaultCharacterSets.EnglishAlphanumeric).Count, "unique count");
            }));

            cases.Add(Case(s, "alphanumeric-members", "EnglishAlphanumeric contains expected boundary members", () =>
            {
                HashSet<char> set = CharSets.Alphanumeric;
                Assert.True(set.Contains('a') && set.Contains('z'), "lowercase a and z");
                Assert.True(set.Contains('A') && set.Contains('Z'), "uppercase A and Z");
                Assert.True(set.Contains('0') && set.Contains('9'), "digits 0 and 9");
                Assert.False(set.Contains('-'), "no hyphen");
            }));

            cases.Add(Case(s, "hex-upper", "HexadecimalUppercase has 16 chars 0-9 and A-F", () =>
            {
                Assert.Equal(16, DefaultCharacterSets.HexadecimalUppercase.Length, "length");
                Assert.True(CharSets.HexUpper.Contains('0') && CharSets.HexUpper.Contains('F'), "contains 0 and F");
                Assert.False(CharSets.HexUpper.Contains('G'), "no G");
                Assert.False(CharSets.HexUpper.Contains('a'), "no lowercase");
            }));

            cases.Add(Case(s, "hex-lower", "HexadecimalLowercase has 16 chars 0-9 and a-f", () =>
            {
                Assert.Equal(16, DefaultCharacterSets.HexadecimalLowercase.Length, "length");
                Assert.True(CharSets.HexLower.Contains('0') && CharSets.HexLower.Contains('f'), "contains 0 and f");
                Assert.False(CharSets.HexLower.Contains('g'), "no g");
                Assert.False(CharSets.HexLower.Contains('A'), "no uppercase");
            }));

            cases.Add(Case(s, "base64-members", "Base64 set contains the alphanumeric alphabet plus '+', '/', '='", () =>
            {
                Assert.Equal(65, DefaultCharacterSets.Base64.Length, "length");
                Assert.True(CharSets.Base64.Contains('+'), "contains +");
                Assert.True(CharSets.Base64.Contains('/'), "contains /");
                Assert.True(CharSets.Base64.Contains('='), "contains =");
            }));

            cases.Add(Case(s, "urlsafe-members", "UrlSafe set contains alphanumeric plus '-', '_', '.', '~'", () =>
            {
                Assert.Equal(66, DefaultCharacterSets.UrlSafe.Length, "length");
                Assert.True(CharSets.UrlSafe.Contains('-'), "contains -");
                Assert.True(CharSets.UrlSafe.Contains('_'), "contains _");
                Assert.True(CharSets.UrlSafe.Contains('.'), "contains .");
                Assert.True(CharSets.UrlSafe.Contains('~'), "contains ~");
            }));

            cases.Add(Case(s, "uskeyboard-members", "USKeyboard set contains letters, digits, and common symbols", () =>
            {
                HashSet<char> set = new HashSet<char>(DefaultCharacterSets.USKeyboard);
                Assert.True(DefaultCharacterSets.USKeyboard.Length > 62, "more than just alphanumeric");
                Assert.True(set.Contains('@') && set.Contains('#') && set.Contains('!'), "contains @ # !");
                Assert.True(set.Contains('a') && set.Contains('9'), "contains letters and digits");
            }));

            cases.Add(Case(s, "no-duplicate-chars", "All default character sets are free of duplicate characters", () =>
            {
                CheckNoDuplicates("EnglishAlphanumeric", DefaultCharacterSets.EnglishAlphanumeric);
                CheckNoDuplicates("Base64", DefaultCharacterSets.Base64);
                CheckNoDuplicates("USKeyboard", DefaultCharacterSets.USKeyboard);
                CheckNoDuplicates("HexadecimalUppercase", DefaultCharacterSets.HexadecimalUppercase);
                CheckNoDuplicates("HexadecimalLowercase", DefaultCharacterSets.HexadecimalLowercase);
                CheckNoDuplicates("UrlSafe", DefaultCharacterSets.UrlSafe);
            }));

            return Suite(s, "Default Character Sets", cases);
        }

        private static TestSuiteDescriptor EdgeCaseSuite()
        {
            const string s = "EdgeCases";
            List<TestCaseDescriptor> cases = new List<TestCaseDescriptor>();

            cases.Add(Case(s, "single-char-id", "Generate(1) produces a single character", () =>
            {
                IdGenerator g = new IdGenerator();
                Assert.Length(1, g.Generate(1), "length 1");
            }));

            cases.Add(Case(s, "very-long-id", "Generate(2048) produces a very long ID", () =>
            {
                IdGenerator g = new IdGenerator();
                string id = g.Generate(2048);
                Assert.Length(2048, id, "length 2048");
                Assert.AllCharsIn(id, CharSets.Alphanumeric, "alphanumeric only");
            }));

            cases.Add(Case(s, "two-char-set-ctor-throws", "Constructing with a 2-character set throws (insufficient entropy)", () =>
            {
                Assert.Throws<ArgumentException>(
                    () => { IdGenerator g = new IdGenerator(new char[] { 'A', 'B' }); },
                    "2-char set rejected by constructor");
            }));

            cases.Add(Case(s, "restrictive-set-via-setter-works", "A restrictive set applied via the setter still generates valid IDs", () =>
            {
                IdGenerator g = new IdGenerator();
                g.ValidCharacters = new HashSet<char>(new char[] { 'A', 'B', 'C' });
                string id = g.Generate(64);
                Assert.Length(64, id, "length");
                Assert.AllCharsIn(id, new HashSet<char>(new char[] { 'A', 'B', 'C' }), "only A, B, C");
            }));

            return Suite(s, "Edge Cases", cases);
        }

        private static TestSuiteDescriptor ErrorHandlingSuite()
        {
            const string s = "ErrorHandling";
            List<TestCaseDescriptor> cases = new List<TestCaseDescriptor>();

            cases.Add(Case(s, "generate-zero", "Generate(0) throws ArgumentException", () =>
            {
                IdGenerator g = new IdGenerator();
                Assert.Throws<ArgumentException>(() => g.Generate(0), "0 rejected");
            }));

            cases.Add(Case(s, "generate-negative", "Generate(-5) throws ArgumentException", () =>
            {
                IdGenerator g = new IdGenerator();
                Assert.Throws<ArgumentException>(() => g.Generate(-5), "negative rejected");
            }));

            cases.Add(Case(s, "prefix-too-long", "Generate with a prefix longer than maxLen throws ArgumentException", () =>
            {
                IdGenerator g = new IdGenerator();
                Assert.Throws<ArgumentException>(() => g.Generate("this_is_a_very_long_prefix", 10), "over-long prefix rejected");
            }));

            cases.Add(Case(s, "null-char-array", "Null char[] constructor argument throws ArgumentNullException", () =>
            {
                Assert.Throws<ArgumentNullException>(() => { IdGenerator g = new IdGenerator((char[])null); }, "null char[] rejected");
            }));

            cases.Add(Case(s, "null-hashset", "Null HashSet constructor argument throws ArgumentNullException", () =>
            {
                Assert.Throws<ArgumentNullException>(() => { IdGenerator g = new IdGenerator((HashSet<char>)null); }, "null HashSet rejected");
            }));

            return Suite(s, "Error Handling", cases);
        }

        private static TestSuiteDescriptor ThreadSafetySuite()
        {
            const string s = "ThreadSafety";
            List<TestCaseDescriptor> cases = new List<TestCaseDescriptor>();

            cases.Add(Case(s, "concurrent-generate", "Concurrent Generate() across threads produces unique valid IDs", () =>
            {
                IdGenerator g = new IdGenerator();
                ConcurrentBag<string> ids = new ConcurrentBag<string>();
                Parallel.For(0, 10000, i => ids.Add(g.Generate(32)));
                Assert.Equal(10000, ids.Count, "generated count");
                Assert.True(ids.All(id => id.Length == 32), "all length 32");
                Assert.Equal(10000, new HashSet<string>(ids).Count, "all unique");
            }));

            cases.Add(Case(s, "concurrent-generate-crypto", "Concurrent Generate() with UseCryptographicRng produces unique valid IDs", () =>
            {
                IdGenerator g = new IdGenerator();
                g.UseCryptographicRng = true;
                ConcurrentBag<string> ids = new ConcurrentBag<string>();
                Parallel.For(0, 10000, i => ids.Add(g.Generate(32)));
                Assert.Equal(10000, ids.Count, "generated count");
                Assert.True(ids.All(id => id.Length == 32), "all length 32");
                Assert.Equal(10000, new HashSet<string>(ids).Count, "all unique");
            }));

            cases.Add(Case(s, "concurrent-mixed-methods", "Concurrent mixed generation methods produce correctly-shaped IDs", () =>
            {
                IdGenerator g = new IdGenerator();
                ConcurrentBag<bool> results = new ConcurrentBag<bool>();
                Parallel.For(0, 5000, i =>
                {
                    string urlSafe = g.GenerateUrlSafe(null, 24);
                    string kSortable = g.GenerateKSortable(null, 48);
                    string b64 = g.GenerateAndEncodeBase64(null, 48);
                    results.Add(urlSafe.Length == 24 && kSortable.Length == 48 && b64.Length == 48);
                });
                Assert.Equal(5000, results.Count, "result count");
                Assert.True(results.All(r => r), "all correctly shaped");
            }));

            return Suite(s, "Thread Safety", cases);
        }

        private static TestSuiteDescriptor DistributionSuite()
        {
            const string s = "Distribution";
            List<TestCaseDescriptor> cases = new List<TestCaseDescriptor>();

            cases.Add(Case(s, "hex-covers-all", "Generation exercises every character in a hex set", () =>
            {
                IdGenerator g = new IdGenerator(DefaultCharacterSets.HexadecimalLowercase);
                HashSet<char> seen = new HashSet<char>();
                for (int i = 0; i < 200; i++)
                    foreach (char c in g.Generate(32))
                        seen.Add(c);
                foreach (char c in "0123456789abcdef")
                    Assert.True(seen.Contains(c), "saw hex char " + c);
            }));

            cases.Add(Case(s, "alphanumeric-covers-all", "Generation exercises every character in the alphanumeric set", () =>
            {
                IdGenerator g = new IdGenerator();
                HashSet<char> seen = new HashSet<char>();
                for (int i = 0; i < 1000; i++)
                    foreach (char c in g.Generate(32))
                        seen.Add(c);
                foreach (char c in DefaultCharacterSets.EnglishAlphanumeric)
                    Assert.True(seen.Contains(c), "saw char " + c);
            }));

            return Suite(s, "Character Distribution", cases);
        }

        #endregion

        #region Helpers

        private static void CheckNoDuplicates(string setName, char[] chars)
        {
            HashSet<char> seen = new HashSet<char>();
            foreach (char c in chars)
            {
                if (!seen.Add(c))
                    throw new AssertionException($"Character set '{setName}' contains duplicate character '{c}'.");
            }
        }

        private static TestCaseDescriptor Case(string suiteId, string caseId, string displayName, Action body)
        {
            Func<CancellationToken, Task> exec = delegate (CancellationToken ct)
            {
                body();
                return Task.CompletedTask;
            };
            return new TestCaseDescriptor(suiteId, caseId, displayName, exec, null, false, null);
        }

        private static TestSuiteDescriptor Suite(string suiteId, string displayName, List<TestCaseDescriptor> cases)
        {
            return new TestSuiteDescriptor(suiteId, displayName, cases, null, null);
        }

        #endregion
    }
}
