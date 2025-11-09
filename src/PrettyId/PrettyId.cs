using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace PrettyId
{
    /// <summary>
    /// ID generator for creating uniform, readable IDs.
    /// </summary>
    public class IdGenerator
    {
        #region Public-Members

        /// <summary>
        /// Maximum length used when generating without specifying a maximum length.
        /// Default is 32. Must be greater than zero.
        /// </summary>
        public int MaximumLength
        {
            get => _MaximumLength;
            set
            {
                if (value < 1) throw new ArgumentException("Maximum length must be greater than zero.");
                _MaximumLength = value;
            }
        }

        /// <summary>
        /// Valid characters to use when generating IDs.
        /// Must contain at least one character.
        /// </summary>
        public HashSet<char> ValidCharacters
        {
            get => _ValidCharacters;
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(ValidCharacters));
                if (value.Count < 1) throw new ArgumentException("At least one valid character must be supplied.");
                _ValidCharacters = value;
            }
        }

        /// <summary>
        /// The maximum number of iterations allowed when generating a unique ID.
        /// This value may need to be larger if using a restrictive set of allowed characters.
        /// Default is 64. Minimum is 1. Maximum is 256.
        /// </summary>
        public short MaxIterations
        {
            get => _MaxIterations;
            set
            {
                if (value < 1) throw new ArgumentException("Maximum iterations must be greater than zero.");
                if (value > 256) throw new ArgumentException("Maximum iterations must be 256 or less.");
                _MaxIterations = value;
            }
        }

        /// <summary>
        /// Whether to use cryptographically secure random number generation.
        /// Default is false. When true, uses RandomNumberGenerator instead of GUID-based randomness.
        /// </summary>
        public bool UseCryptographicRng { get; set; }

        #endregion

        #region Private-Members

        private HashSet<char> _ValidCharacters;
        private int _MaximumLength = 32;
        private short _MaxIterations = 64;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate an ID generator with default settings.
        /// Uses English alphanumeric characters (a-z, A-Z, 0-9) and a default maximum length of 32.
        /// </summary>
        public IdGenerator()
        {
            _ValidCharacters = new HashSet<char>(DefaultCharacterSets.EnglishAlphanumeric);
        }

        /// <summary>
        /// Instantiate an ID generator with a specific character set.
        /// </summary>
        /// <param name="validCharacters">Array of valid characters to use for ID generation.</param>
        /// <param name="maximumLength">Default maximum length for generated IDs.</param>
        public IdGenerator(char[] validCharacters, int maximumLength = 32)
        {
            if (validCharacters == null) throw new ArgumentNullException(nameof(validCharacters));
            if (validCharacters.Length < 1) throw new ArgumentException("At least one valid character must be supplied.");
            if (maximumLength < 1) throw new ArgumentException("Maximum length must be greater than zero.");

            _ValidCharacters = new HashSet<char>(validCharacters);
            _MaximumLength = maximumLength;

            WarnIfRestrictiveCharacterSet();
        }

        /// <summary>
        /// Instantiate an ID generator with a specific character set.
        /// </summary>
        /// <param name="validCharacters">HashSet of valid characters to use for ID generation.</param>
        /// <param name="maximumLength">Default maximum length for generated IDs.</param>
        public IdGenerator(HashSet<char> validCharacters, int maximumLength = 32)
        {
            if (validCharacters == null) throw new ArgumentNullException(nameof(validCharacters));
            if (validCharacters.Count < 1) throw new ArgumentException("At least one valid character must be supplied.");
            if (maximumLength < 1) throw new ArgumentException("Maximum length must be greater than zero.");

            _ValidCharacters = validCharacters;
            _MaximumLength = maximumLength;

            WarnIfRestrictiveCharacterSet();
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Generate an ID using the configured maximum length.
        /// </summary>
        /// <returns>Generated ID string.</returns>
        public string Generate()
        {
            return Generate(null, _MaximumLength);
        }

        /// <summary>
        /// Generate an ID with a specific length.
        /// </summary>
        /// <param name="length">Length of the ID to generate.</param>
        /// <returns>Generated ID string.</returns>
        public string Generate(int length)
        {
            return Generate(null, length);
        }

        /// <summary>
        /// Generate an ID with a prefix and specific maximum length.
        /// </summary>
        /// <param name="prefix">Prefix to prepend to the ID. Can be null or empty.</param>
        /// <param name="maxLen">Maximum total length including the prefix.</param>
        /// <returns>Generated ID string.</returns>
        /// <exception cref="ArgumentException">Thrown when maxLen is invalid or prefix is too long.</exception>
        /// <exception cref="InvalidOperationException">Thrown when unable to generate an ID within the iteration limit.</exception>
        public string Generate(string prefix, int maxLen)
        {
            if (maxLen < 1)
                throw new ArgumentException("Maximum length must be greater than zero.", nameof(maxLen));

            if (!string.IsNullOrEmpty(prefix))
            {
                if (prefix.Length >= maxLen)
                {
                    throw new ArgumentException(
                        $"Maximum length ({maxLen}) must be greater than prefix length ({prefix.Length}).",
                        nameof(maxLen));
                }
            }

            int keyLen = GetActualLength(prefix, maxLen);
            int iteration = 0;
            StringBuilder sb = new StringBuilder(maxLen);

            if (!string.IsNullOrEmpty(prefix))
                sb.Append(prefix);

            int position = 0;
            bool complete = false;

            while (iteration < _MaxIterations)
            {
                string randomSource = GetRandomSource();

                foreach (char c in randomSource)
                {
                    if (position >= keyLen)
                    {
                        complete = true;
                        break;
                    }

                    if (_ValidCharacters.Contains(c))
                    {
                        sb.Append(c);
                        position++;
                    }
                }

                if (complete)
                    break;

                iteration++;
            }

            if (!complete)
            {
                throw new InvalidOperationException(
                    $"Unable to generate an ID with the supplied constraints after {_MaxIterations} iterations. " +
                    $"Consider using a larger character set or increasing MaxIterations.");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Generate a URL-safe ID (uses only characters safe for URLs without encoding).
        /// Uses: a-z, A-Z, 0-9, hyphen, and underscore.
        /// </summary>
        /// <param name="prefix">Prefix to prepend to the ID. Can be null or empty.</param>
        /// <param name="maxLen">Maximum total length including the prefix.</param>
        /// <returns>URL-safe ID string.</returns>
        public string GenerateUrlSafe(string prefix = null, int maxLen = 32)
        {
            var originalCharSet = _ValidCharacters;
            try
            {
                _ValidCharacters = new HashSet<char>(DefaultCharacterSets.UrlSafe);
                return Generate(prefix, maxLen);
            }
            finally
            {
                _ValidCharacters = originalCharSet;
            }
        }

        /// <summary>
        /// Generate a k-sortable ID with a timestamp prefix for chronological sorting.
        /// Format: {timestamp}_{random} where timestamp is Unix milliseconds in base36.
        /// </summary>
        /// <param name="prefix">Optional prefix to prepend before the timestamp.</param>
        /// <param name="maxLen">Maximum total length including all parts.</param>
        /// <returns>K-sortable ID string.</returns>
        public string GenerateKSortable(string prefix = null, int maxLen = 32)
        {
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            string timestampEncoded = ToBase36(timestamp);

            string combinedPrefix = string.IsNullOrEmpty(prefix)
                ? timestampEncoded + "_"
                : prefix + timestampEncoded + "_";

            if (combinedPrefix.Length >= maxLen)
            {
                throw new ArgumentException(
                    $"Maximum length ({maxLen}) is too short for k-sortable ID with prefix. " +
                    $"Combined prefix length is {combinedPrefix.Length}.",
                    nameof(maxLen));
            }

            return Generate(combinedPrefix, maxLen);
        }

        /// <summary>
        /// Generate an ID and encode it as base64.
        /// This method generates a random string, then base64-encodes it.
        /// Note: The result will be longer than the input due to base64 encoding overhead.
        /// </summary>
        /// <param name="prefix">Prefix to prepend to the final encoded result.</param>
        /// <param name="maxLen">Maximum total length including the prefix.</param>
        /// <returns>Base64-encoded ID string.</returns>
        public string GenerateAndEncodeBase64(string prefix = null, int maxLen = 32)
        {
            if (maxLen < 1)
                throw new ArgumentException("Maximum length must be greater than zero.", nameof(maxLen));

            if (!string.IsNullOrEmpty(prefix))
            {
                if (prefix.Length >= maxLen - 3)
                {
                    throw new ArgumentException(
                        $"Maximum length ({maxLen}) must be greater than prefix length ({prefix.Length}) " +
                        $"by at least 4 characters for base64 encoding.",
                        nameof(maxLen));
                }
            }

            int keyLen = GetActualLength(prefix, maxLen);

            // Adjust for base64 padding (must be divisible by 4)
            while (keyLen % 4 > 0) keyLen--;

            // Calculate the original string length needed
            keyLen = GetStringLengthFromEncodedLength(keyLen);

            // Generate the random string
            var originalCharSet = _ValidCharacters;
            try
            {
                _ValidCharacters = new HashSet<char>(DefaultCharacterSets.USKeyboard);
                string key = Generate(null, keyLen);
                string base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(key));

                StringBuilder result = new StringBuilder(maxLen);
                if (!string.IsNullOrEmpty(prefix))
                    result.Append(prefix);
                result.Append(base64);

                return result.ToString();
            }
            finally
            {
                _ValidCharacters = originalCharSet;
            }
        }

        #endregion

        #region Private-Methods

        private string GetRandomSource()
        {
            if (UseCryptographicRng)
            {
                // Use cryptographically secure random bytes
                byte[] randomBytes = new byte[16];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(randomBytes);
                }
                return Convert.ToBase64String(randomBytes);
            }
            else
            {
                // Use GUID-based randomness (default)
                return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            }
        }

        private static int GetEncodedLengthFromStringLength(int len)
        {
            return (len / 3) * 4;
        }

        private static int GetStringLengthFromEncodedLength(int len)
        {
            return (len / 4) * 3;
        }

        private static int GetActualLength(string prefix, int maxLen)
        {
            int actualLen = maxLen;
            if (!string.IsNullOrEmpty(prefix))
                actualLen -= prefix.Length;
            return actualLen;
        }

        private static string ToBase36(long value)
        {
            const string chars = "0123456789abcdefghijklmnopqrstuvwxyz";
            StringBuilder result = new StringBuilder();

            while (value > 0)
            {
                result.Insert(0, chars[(int)(value % 36)]);
                value /= 36;
            }

            return result.Length > 0 ? result.ToString() : "0";
        }

        /// <summary>
        /// Warns (via console) if the character set is very restrictive and may cause performance issues.
        /// A character set is considered restrictive if it has fewer than 16 characters.
        /// </summary>
        private void WarnIfRestrictiveCharacterSet()
        {
            if (_ValidCharacters.Count < 16)
            {
                Console.WriteLine(
                    $"Warning: Character set contains only {_ValidCharacters.Count} characters. " +
                    $"This may require many iterations to generate IDs. " +
                    $"Consider using a larger character set or increasing MaxIterations.");
            }
        }

        #endregion
    }
}
