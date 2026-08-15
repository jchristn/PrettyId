using System;
using System.Collections.Generic;
using PrettyId;

namespace Test.Shared
{
    /// <summary>
    /// Shared, read-only character-set lookups and helpers used by the test descriptors
    /// to validate generated output.
    /// </summary>
    public static class CharSets
    {
        /// <summary>
        /// English alphanumeric characters (a-z, A-Z, 0-9).
        /// </summary>
        public static readonly HashSet<char> Alphanumeric = new HashSet<char>(DefaultCharacterSets.EnglishAlphanumeric);

        /// <summary>
        /// URL-safe characters (alphanumeric plus hyphen, underscore, period, tilde).
        /// </summary>
        public static readonly HashSet<char> UrlSafe = new HashSet<char>(DefaultCharacterSets.UrlSafe);

        /// <summary>
        /// Lowercase hexadecimal characters (0-9, a-f).
        /// </summary>
        public static readonly HashSet<char> HexLower = new HashSet<char>(DefaultCharacterSets.HexadecimalLowercase);

        /// <summary>
        /// Uppercase hexadecimal characters (0-9, A-F).
        /// </summary>
        public static readonly HashSet<char> HexUpper = new HashSet<char>(DefaultCharacterSets.HexadecimalUppercase);

        /// <summary>
        /// Base64 alphabet characters (alphanumeric plus '+', '/', '=').
        /// </summary>
        public static readonly HashSet<char> Base64 = new HashSet<char>(DefaultCharacterSets.Base64);

        /// <summary>
        /// Base36 lowercase characters (0-9, a-z) used for k-sortable timestamp encoding.
        /// </summary>
        public static readonly HashSet<char> Base36 = new HashSet<char>(
            "0123456789abcdefghijklmnopqrstuvwxyz".ToCharArray());

        /// <summary>
        /// Decode a lowercase base36 string into a long value.
        /// </summary>
        /// <param name="value">Base36 encoded string.</param>
        /// <returns>Decoded value.</returns>
        public static long FromBase36(string value)
        {
            const string chars = "0123456789abcdefghijklmnopqrstuvwxyz";
            long result = 0;
            foreach (char c in value)
            {
                int index = chars.IndexOf(c);
                if (index < 0)
                    throw new FormatException($"Character '{c}' is not a valid base36 digit.");
                result = (result * 36) + index;
            }
            return result;
        }
    }
}
