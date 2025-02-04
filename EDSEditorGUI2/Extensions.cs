using System;

namespace EDSEditorGUI2
{
    public static class StringExtensions
    {
        /// <summary>
        /// Convert different types of hex/dec string to integer
        /// </summary>
        public static UInt16 ToInteger(this String val)
        {
            return (UInt16)Convert.ToInt32(val, 16);
        }
    }
}
