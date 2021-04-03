using System;
using System.Collections.Generic;
using System.Text;

namespace PiBorgSharp
{
    public static class Utilities
    {
        /// <summary>
        /// Helper routine to build a string for the logger; n >= 0 is forward; n < 0 is reverse
        /// </summary>
        /// <param name="value">Value from which to interpret string</param>
        /// <returns>A string of "forward" if value is >= 0; "reverse" if < 0</returns>
        public static string ParseDirection(int value)
        {
            string tempReturn = string.Empty;

            if (value >= 0)
            {
                tempReturn = "forward";
            }
            else
            {
                tempReturn = "reverse";
            }

            return tempReturn;
        }

        /// <summary>
        /// Helper routine to output the contents of a byte array as a hexadecimal string
        /// </summary>
        /// <param name="buffer">Byte array to parse into a string</param>
        /// <returns>String representing hexadecimal bytes in array</returns>
        public static string BytesToString(byte[] buffer)
        {
            string tempReturn = string.Empty;

            for (int i = 0; i < buffer.Length; i++)
            {
                tempReturn += buffer[i].ToString("X2") + " ";
            }

            return tempReturn;
        }
    }
}
