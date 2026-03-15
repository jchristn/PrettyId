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
        /// Retained for backward compatibility. The current generation algorithm uses direct
        /// indexing and does not require multiple iterations.
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

        private static readonly RandomNumberGenerator _CryptoRng = RandomNumberGenerator.Create();

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
        /// <exception cref="ArgumentNullException">Thrown when validCharacters is null.</exception>
        /// <exception cref="ArgumentException">Thrown when validCharacters has fewer than 16 characters or maximumLength is invalid.</exception>
        public IdGenerator(char[] validCharacters, int maximumLength = 32)
        {
            if (validCharacters == null) throw new ArgumentNullException(nameof(validCharacters));
            if (validCharacters.Length < 1) throw new ArgumentException("At least one valid character must be supplied.");
            if (maximumLength < 1) throw new ArgumentException("Maximum length must be greater than zero.");

            _ValidCharacters = new HashSet<char>(validCharacters);
            _MaximumLength = maximumLength;

            ThrowIfRestrictiveCharacterSet();
        }

        /// <summary>
        /// Instantiate an ID generator with a specific character set.
        /// </summary>
        /// <param name="validCharacters">HashSet of valid characters to use for ID generation.</param>
        /// <param name="maximumLength">Default maximum length for generated IDs.</param>
        /// <exception cref="ArgumentNullException">Thrown when validCharacters is null.</exception>
        /// <exception cref="ArgumentException">Thrown when validCharacters has fewer than 16 characters or maximumLength is invalid.</exception>
        public IdGenerator(HashSet<char> validCharacters, int maximumLength = 32)
        {
            if (validCharacters == null) throw new ArgumentNullException(nameof(validCharacters));
            if (validCharacters.Count < 1) throw new ArgumentException("At least one valid character must be supplied.");
            if (maximumLength < 1) throw new ArgumentException("Maximum length must be greater than zero.");

            _ValidCharacters = validCharacters;
            _MaximumLength = maximumLength;

            ThrowIfRestrictiveCharacterSet();
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
        public string Generate(string prefix, int maxLen)
        {
            return GenerateInternal(prefix, maxLen, _ValidCharacters.ToArray());
        }

        /// <summary>
        /// Generate a URL-safe ID (uses only characters safe for URLs without encoding).
        /// Uses: a-z, A-Z, 0-9, hyphen, underscore, period, and tilde.
        /// This method is thread-safe and does not modify instance state.
        /// </summary>
        /// <param name="prefix">Prefix to prepend to the ID. Can be null or empty.</param>
        /// <param name="maxLen">Maximum total length including the prefix.</param>
        /// <returns>URL-safe ID string.</returns>
        public string GenerateUrlSafe(string prefix = null, int maxLen = 32)
        {
            return GenerateInternal(prefix, maxLen, DefaultCharacterSets.UrlSafe);
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

            return GenerateInternal(combinedPrefix, maxLen, _ValidCharacters.ToArray());
        }

        /// <summary>
        /// Generate an ID and encode it as base64.
        /// This method generates a random string, then base64-encodes it.
        /// Note: The result will be longer than the input due to base64 encoding overhead.
        /// This method is thread-safe and does not modify instance state.
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

            // Generate the random string using USKeyboard charset directly (no instance mutation)
            string key = GenerateInternal(null, keyLen, DefaultCharacterSets.USKeyboard);
            string base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(key));

            StringBuilder result = new StringBuilder(maxLen);
            if (!string.IsNullOrEmpty(prefix))
                result.Append(prefix);
            result.Append(base64);

            return result.ToString();
        }

        #endregion

        #region Private-Methods

        /// <summary>
        /// Core generation method. Produces a random string from the given character set
        /// using direct modular indexing over random bytes. Thread-safe — no instance state is mutated.
        /// </summary>
        private string GenerateInternal(string prefix, int maxLen, char[] charSet)
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

            StringBuilder sb = new StringBuilder(maxLen);
            if (!string.IsNullOrEmpty(prefix))
                sb.Append(prefix);

            byte[] randomBytes = GetRandomBytes(keyLen);
            for (int i = 0; i < keyLen; i++)
            {
                sb.Append(charSet[randomBytes[i] % charSet.Length]);
            }

            return sb.ToString();
        }

        private byte[] GetRandomBytes(int count)
        {
            byte[] bytes = new byte[count];

            if (UseCryptographicRng)
            {
                _CryptoRng.GetBytes(bytes);
            }
            else
            {
                // Use GUID-based randomness (default)
                int offset = 0;
                while (offset < count)
                {
                    byte[] guidBytes = Guid.NewGuid().ToByteArray();
                    int toCopy = Math.Min(guidBytes.Length, count - offset);
                    Buffer.BlockCopy(guidBytes, 0, bytes, offset, toCopy);
                    offset += toCopy;
                }
            }

            return bytes;
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
        /// Throws an ArgumentException if the character set has fewer than 16 unique characters,
        /// as this provides insufficient entropy for generating unique IDs.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when the character set contains fewer than 16 characters.</exception>
        private void ThrowIfRestrictiveCharacterSet()
        {
            if (_ValidCharacters.Count < 16)
            {
                throw new ArgumentException(
                    $"Character set contains only {_ValidCharacters.Count} unique character(s). " +
                    $"A minimum of 16 unique characters is required for sufficient ID entropy.");
            }
        }

        #endregion
    }
}
