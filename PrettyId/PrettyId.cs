using System;
using System.Linq;
using System.Text;

namespace PrettyId
{
    /// <summary>
    /// ID generator.
    /// </summary>
    public class IdGenerator
    {
        #region Public-Members

        /// <summary>
        /// Maximum length used when generating without specifying a maximum length.
        /// </summary>
        public static int MaximumLength
        {
            get
            {
                return _MaximumLength;
            }
            set
            {
                if (value < 1) throw new ArgumentException("Maximum length must be greater than zero.");
                _MaximumLength = value;
            }
        }

        /// <summary>
        /// Valid characters.
        /// </summary>
        public static char[] ValidCharacters
        {
            get
            {
                return _ValidCharacters;
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(ValidCharacters));
                if (value.Length < 1) throw new ArgumentException("At least one valid character must be supplied.");
                _ValidCharacters = value;
            }
        }
            
        /// <summary>
        /// The maximum number of iterations allowed when generating a unique ID.
        /// This value may need to be larger if using a restrictive set of allowed characters.
        /// Default is 32.  Minimum is 1.  Maximum is 64.
        /// </summary>
        public static int MaxIterations
        {
            get
            {
                return _MaxIterations;
            }
            set
            {
                if (value < 1) throw new ArgumentException("Maximum iterations must be greater than zero.");
                if (value > 64) throw new ArgumentException("Maximum iterations must be 64 or less.");
            }
        }

        #endregion

        #region Private-Members

        private static char[] _ValidCharacters = new char[]
        {
            'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z',
            'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z',
            '0','1','2','3','4','5','6','7','8','9'
        };

        private static int _MaximumLength = 32;
        private static short _MaxIterations = 64;

        #endregion

        #region Constructors-and-Factories
         
        #endregion

        #region Public-Methods

        /// <summary>
        /// Generate an ID.
        /// </summary>
        /// <returns>String.</returns>
        public static string Generate()
        {
            return Generate(null, _MaximumLength);
        }

        /// <summary>
        /// Generate an ID.
        /// </summary>
        /// <param name="len">Length.</param>
        /// <returns>String.</returns>
        public static string Generate(int len)
        {
            return Generate(null, len);
        }

        /// <summary>
        /// Generate an ID.
        /// </summary>
        /// <param name="prefix">Prefix.</param>
        /// <param name="maxLen">Maximum length.</param>
        /// <returns>String.</returns>
        public static string Generate(string prefix = null, int maxLen = 32)
        {
            if (maxLen < 1) throw new ArgumentException("Maximum length must be greater than zero.");
            if (!String.IsNullOrEmpty(prefix))
            {
                if (prefix.Length >= maxLen)
                {
                    throw new ArgumentException("Maximum length must be greater than the length of the supplied prefix.");
                }
            }

            int actualLen = GetActualLength(prefix, maxLen);
            int iteration = 0;
            string ret = "";
            if (!String.IsNullOrEmpty(prefix)) ret += prefix;
            int position = ret.Length;
            bool complete = false;

            while (iteration < _MaxIterations)
            {
                string base64 = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
                foreach (char c in base64)
                {
                    if (position >= (actualLen - 1))
                    {
                        complete = true;
                        break;
                    }

                    if (_ValidCharacters.Contains(c))
                    {
                        ret += c;
                        position++;
                    }
                }

                if (complete)
                {
                    break;
                }

                iteration++;
            }

            if (!complete) throw new InvalidOperationException("Unable to generate an ID with the supplied constraints.");

            return ret;
        }

        #endregion

        #region Private-Methods

        private static int GetActualLength(string prefix, int maxLen)
        {
            int actualLen = maxLen;
            if (!String.IsNullOrEmpty(prefix)) actualLen -= prefix.Length;
            return actualLen;
        }

        private static byte[] InitializeByteArray(byte[] bytes)
        {
            for (int i = 0; i < bytes.Length; i++)
                bytes[i] = 0x00;
            return bytes;
        }

        #endregion
    }
}
