using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libEDSsharp
{
    /// <summary>
    /// Logger class used to report problems with import/export
    /// </summary>
    public static class Warnings
    {
        /// <summary>
        /// type of warnings
        /// </summary>
        public enum warning_class
        {
            /// <summary>
            /// Generic warning
            /// </summary>
            WARNING_GENERIC = 0x01,
            /// <summary>
            /// index/subindex rename warnings
            /// </summary>
            WARNING_RENAME = 0x02,
            /// <summary>
            /// problem with index/subindexes that are needed to make canopennode functions work
            /// </summary>
            WARNING_BUILD = 0x04,
            /// <summary>
            /// Problem with strings variable export
            /// </summary>
            WARNING_STRING = 0x08,
            /// <summary>
            /// Problem with struct/record export
            /// </summary>
            WARNING_STRUCT = 0x10,
        }

        /// <summary>
        /// List of warnings
        /// </summary>
        public static List<string> warning_list = new List<string>();
        /// <summary>
        /// bit mask used to stop messages being added to the list
        /// </summary>
        public static UInt32 warning_mask = 0xffff;

        /// <summary>
        /// Add warning to the list of warnings
        /// </summary>
        /// <param name="warning">string to report</param>
        /// <param name="c">type of warning (filter usage)</param>
        public static void AddWarning(string warning,warning_class c = warning_class.WARNING_GENERIC)
        {
            if (((UInt32)c & warning_mask) != 0)
            {
                warning_list.Add(warning);
            }
        }
    }

   

}
