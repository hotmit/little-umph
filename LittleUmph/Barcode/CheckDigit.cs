using System;
using System.Collections.Generic;
using System.Text;

namespace LittleUmph
{
    /// <summary>
    /// Barcode check digit
    /// </summary>
    public class CheckDigit
    {
        /// <summary>
        /// Gets the check digit.
        /// </summary>
        /// <param name="barcode">The barcode without the check digit.</param>
        /// <returns></returns>
        public static int GetCheckDigit(string barcode)
        {
            if (barcode.Length % 2 == 0)
            {
                throw new Exception("Invalid input, the code length must be an odd number.");
            }

            int odd = 0, even = 0;            
            for (int i = 0; i < barcode.Length; i++)
            {
                int value = ((int)barcode[i]) - 48;
                if (value < 0 || value > 9)
                {
                    throw new Exception("Invalid input, the code must be number only.");
                }

                if (i % 2 == 0)
                {
                    odd += value;
                }
                else
                {
                    even += value;
                }
            }

            int checkdigit = 10 - (((odd * 3) + even) % 10);
            return checkdigit;
        }

        /// <summary>
        /// Validates the check digit.
        /// </summary>
        /// <param name="barcode">The barcode.</param>
        /// <returns></returns>
        public static bool ValidateCheckDigit(string barcode)
        {
            if (barcode.Length % 2 == 1)
            {
                throw new Exception("Invalid input, the code length must include the check digit at the end.");
            }

            int checkdigit = ((int)barcode[barcode.Length - 1]) -48;
            int digit = GetCheckDigit(barcode.Substring(0, barcode.Length - 1));

            return checkdigit == digit;
        }
    }
}
