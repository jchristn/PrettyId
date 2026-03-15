using System;
using System.Collections.Generic;
using System.Text;

namespace PrettyId
{
    /// <summary>
    /// Default character sets.
    /// </summary>
    public static class DefaultCharacterSets
    {
        /// <summary>
        /// English alphanumeric.
        /// </summary>
        public static readonly char[] EnglishAlphanumeric = new char[]
        {
            'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z',
            'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z',
            '0','1','2','3','4','5','6','7','8','9'
        };

        /// <summary>
        /// Base64 characters.
        /// </summary>
        public static readonly char[] Base64 = new char[]
        {
            'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z',
            'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z',
            '0','1','2','3','4','5','6','7','8','9','+','/','='
        };

        /// <summary>
        /// Keys from a standard US keyboard.
        /// </summary>
        public static readonly char[] USKeyboard = new char[]
        {
            'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z',
            'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z',
            '0','1','2','3','4','5','6','7','8','9',
            '`','~','!','@','#','$','%','^','&','*','(',')','-','_','+','=','[','{',']','}','\\','|',';',':',
            '\'','\"',
            ',','<','.','>','/','?'
        };

        /// <summary>
        /// Hexadecimal characters, using uppercase letters.
        /// </summary>
        public static readonly char[] HexadecimalUppercase = new char[]
        {
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
            'A', 'B', 'C', 'D', 'E', 'F'
        };

        /// <summary>
        /// Hexadecimal characters, using lowercase letters.
        /// </summary>
        public static readonly char[] HexadecimalLowercase = new char[]
        {
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
            'a', 'b', 'c', 'd', 'e', 'f'
        };

        /// <summary>
        /// URL-safe characters (RFC 3986 unreserved characters).
        /// Includes: a-z, A-Z, 0-9, hyphen, underscore, period, and tilde.
        /// </summary>
        public static readonly char[] UrlSafe = new char[]
        {
            'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z',
            'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z',
            '0','1','2','3','4','5','6','7','8','9',
            '-','_','.','\u007e' // hyphen, underscore, period, tilde
        };
    }
}
