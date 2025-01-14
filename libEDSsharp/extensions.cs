using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libEDSsharp
{
    /// <summary>
    /// Helper functions to convert integral values into hexadecimal string
    /// </summary>
    public static class extensions
    {
        /// <summary>
        /// returns a string containing the value as hexadecimal
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>hexadecimal string representing the value</returns>
        public static string ToHexString(this byte val)
        {
            return String.Format("0x{0:x}", val);
        }
        /// <summary>
        /// returns a string containing the value as hexadecimal
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>hexadecimal string representing the value</returns>
        public static string ToHexString(this UInt16 val)
        {
            return String.Format("0x{0:x}",val);
        }
        /// <summary>
        /// returns a string containing the value as hexadecimal
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>hexadecimal string representing the value</returns>
        public static string ToHexString(this UInt32 val)
        {
            return String.Format("0x{0:x}", val);
        }

    }

    /// <summary>
    /// String extension methodes
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Performs a case insensitive Contain function
        /// </summary>
        /// <param name="str">the string to look in</param>
        /// <param name="substring">the string to look for</param>
        /// <param name="comp">comparison methode </param>
        /// <returns>true if substring is found in str</returns>
        /// <exception cref="ArgumentNullException">substring was null</exception>
        /// <exception cref="ArgumentException">comp methode was not a valid argument</exception>
        /// <remarks>This can be replaced with native .net function in .net core</remarks>
        /// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/system.string.contains?view=netframework-4.8.1#system-string-contains(system-string-system-stringcomparison)"/>
        public static bool Contains(this String str, String substring,
                                    StringComparison comp)
        {
            if (substring == null)
                throw new ArgumentNullException("substring",
                                             "substring cannot be null.");
            else if (!Enum.IsDefined(typeof(StringComparison), comp))
                throw new ArgumentException("comp is not a member of StringComparison",
                                         "comp");

            return str.IndexOf(substring, comp) >= 0;
        }
    }
}
