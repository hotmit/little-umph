using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

using System.Globalization;

namespace LittleUmph
{
    #region [ Number Format Type ]
    /// <summary>
    /// Number Format
    /// </summary>
    public enum NumberFormatType
    {
        /// <summary>
        /// Thousands separator and no significant digit when there is no fraction amount.
        /// </summary>
        Normal,
        /// <summary>
        /// Money
        /// </summary>
        Money,
        /// <summary>
        /// Temperature in Celcius
        /// </summary>
        TempC,
        /// <summary>
        /// Temperature in Fahrenheit
        /// </summary>
        TempF,
        /// <summary>
        /// Percentage
        /// </summary>
        Percent,
        /// <summary>
        /// Volume in Liters
        /// </summary>
        Volume
    }
    #endregion

    /// <summary>
    /// Collection of functions to validate and manipulate number.
    /// </summary>
    public class Num
    {
        #region [ Number Format ]
        /// <summary>
        /// Format the number based on the type
        /// </summary>
        /// <param name="number"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string Format(double number, NumberFormatType type)
        {
            if (type == NumberFormatType.Money)
            {
                return string.Format("{0:c}", number);
            }
            if (type == NumberFormatType.Percent)
            {
                return string.Format("{0:0%}", number / 100.0);
            }
            if (type == NumberFormatType.TempC)
            {
                return string.Format("{0:0} °C", number);
            }
            if (type == NumberFormatType.TempF)
            {
                return string.Format("{0:0} °F", number);
            }
            if (type == NumberFormatType.Volume)
            {
                return string.Format("{0:#,0} L", number);
            }
            return string.Format("{0:#,0}", number);
        }

        /// <summary>
        /// Formats the specified number.
        /// </summary>
        /// <param name="number">The number.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static string Format(string number, NumberFormatType type)
        {
            double value = Str.DoubleVal(number);
            return Format(value, type);
        }
        #endregion

        #region [ Random Number ]
        private readonly static Random _randomGenerator = new Random();

        /// <summary>
        /// Generate RandNum number
        /// </summary>
        /// <returns></returns>
        public static int Random()
        {
            return _randomGenerator.Next();
        }

        /// <summary>
        /// Generate number from [min] to [max] inclusive (example 1 to 5, valid output are 1, 2, 3, 4, 5)
        /// </summary>
        /// <param name="min">Inclusive (example 1 to 5, valid output are 1, 2, 3, 4, 5)</param>
        /// <param name="max">Inclusive (example 1 to 5, valid output are 1, 2, 3, 4, 5)</param>
        /// <returns></returns>
        public static int Random(int min, int max)
        {
            return _randomGenerator.Next(min, max);
        }

        /// <summary>
        /// Generate RandNum number from [min] to [max], using the seed specified in the generation logic.
        /// Do not use this when you need more than a few numbers. Because it is very slow.
        /// </summary>
        /// <param name="min">Inclusive (example 1 to 5, valid output are 1, 2, 3, 4, 5)</param>
        /// <param name="max">Inclusive (example 1 to 5, valid output are 1, 2, 3, 4, 5)</param>
        /// <param name="seed"></param>
        /// <returns></returns>
        public static int Random(int min, int max, int seed)
        {
            Random r = new Random(seed);
            return r.Next(min, max);
        }

        /// <summary>
        /// Generate a random number within the range (inclusive) and then pad the left with zeros.
        /// </summary>
        /// <param name="min">Inclusive (example 1 to 5, valid output are 1, 2, 3, 4, 5)</param>
        /// <param name="max">Inclusive (example 1 to 5, valid output are 1, 2, 3, 4, 5)</param>
        /// <param name="totalWidth">The total width.</param>
        /// <returns></returns>
        public static string RandZeroPad(int min, int max, int totalWidth)
        {
            string number = Random(min, max).ToString();
            return number.PadLeft(totalWidth, '0');
        }
        #endregion

        #region [ Filter Min + Max Range ]
        /// <summary>
        /// Filter the number. If the number is smaller than min or bigger than max
        /// then return the appropriate default value. Else return the passed value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="inclusiveMin">The inclusive min (good even if it equal to the min value).</param>
        /// <param name="exlusiveMax">The exlusive max.</param>
        /// <returns></returns>
        public static int Filter(int value, int inclusiveMin, int exlusiveMax)
        {
            int defaultMin = inclusiveMin;
            int defaultMax = MinFilter(exlusiveMax - 1, inclusiveMin);
            return Filter(value, inclusiveMin, exlusiveMax, defaultMin, defaultMax);
        }

        /// <summary>
        /// Filter the number. If the number is smaller than min or bigger than max
        /// then return the appropriate default value. Else return the passed value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="inclusiveMin">The inclusive min (good even if it equal to the min value).</param>
        /// <param name="exclusiveMax">The exclusive max (if value equal to max, it is no good).</param>
        /// <param name="defaultMin">The default min.</param>
        /// <param name="defaultMax">The default max.</param>
        /// <returns></returns>
        public static int Filter(int value, int inclusiveMin, int exclusiveMax, int defaultMin, int defaultMax)
        {
            if (value < inclusiveMin)
            {
                return defaultMin;
            }
            if (value >= exclusiveMax)
            {
                return defaultMax;
            }

            return value;
        }
        #endregion

        #region [ Increase & Decrease ]
        /// <summary>
        /// Increases the specified value upto the (limit - 1).
        /// </summary>
        /// <param name="currentValue">The current value.</param>
        /// <param name="upperLimit">The upper limit.</param>
        /// <param name="increment">The increment.</param>
        /// <returns></returns>
        public int Increase(int currentValue, int upperLimit, int increment = 1)
        {
            return Increase(currentValue, upperLimit, upperLimit - 1, increment);
        }

        /// <summary>
        /// Increases the specified value upto the (limit - 1).
        /// If it is exceeded the limit return the specified execeeded value.
        /// </summary>
        /// <param name="currentValue">The current value.</param>
        /// <param name="upperLimit">The upper limit.</param>
        /// <param name="valueExceededLimit">The value exceeded limit.</param>
        /// <param name="increment">The increment.</param>
        /// <returns></returns>
        public int Increase(int currentValue, int upperLimit, int valueExceededLimit, int increment = 1)
        {
            currentValue += increment;
            if (currentValue >= upperLimit)
            {
                return valueExceededLimit;
            }
            return currentValue;
        }

        /// <summary>
        /// Decrease the value down to the lower limit (ie if limit zero, zero is valid result).
        /// </summary>
        /// <param name="currentValue">The current value.</param>
        /// <param name="lowerLimitInclusive">The lower limit inclusive.</param>
        /// <param name="decrement">The decrement.</param>
        /// <returns></returns>
        public int Decrease(int currentValue, int lowerLimitInclusive, int decrement = 1)
        {
            return Decrease(currentValue, lowerLimitInclusive, lowerLimitInclusive, decrement);
        }

        /// <summary>
        /// Decreases the specified current value.
        /// </summary>
        /// <param name="currentValue">The current value.</param>
        /// <param name="lowerLimitInclusive">The lower limit inclusive.</param>
        /// <param name="valueExceededLimit">The value exceeded limit.</param>
        /// <param name="decrement">The decrement.</param>
        /// <returns></returns>
        public int Decrease(int currentValue, int lowerLimitInclusive, int valueExceededLimit, int decrement = 1)
        {
            currentValue -= decrement;
            if (currentValue < lowerLimitInclusive)
            {
                return valueExceededLimit;
            }
            return currentValue;
        }
        #endregion

        #region [ IsBetween ]
        /// <summary>
        /// Determines whether the specified number is between the number range.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="minInclusive">The min inclusive (ex: min is 10: 4 is invalid, 10 and 12 are valid).</param>
        /// <param name="maxInclusive">The max inclusive (ex: max is 100: 105 is invalid, 99, 100 is valid).</param>
        /// <returns>
        /// 	<c>true</c> if the specified number is between the two numbers; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsBetween(int value, int minInclusive, int maxInclusive)
        {
            if (value == maxInclusive + 1)
            {
                return false;
            }

            if (value >= minInclusive && value <= maxInclusive)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Determines whether the specified number is between the number range.
        /// </summary>
        /// <param name="number">The number.</param>
        /// <param name="minInclusive">The min inclusive (ex: min is 10: 4 is invalid, 10 and 12 are valid).</param>
        /// <param name="maxInclusive">The max inclusive (ex: max is 100: 105 is invalid, 99, 100 is valid).</param>
        /// <returns>
        /// 	<c>true</c> if the specified number is between the two numbers; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsBetween(string number, int minInclusive, int maxInclusive)
        {
            bool isInt = Str.IsInteger(number);

            if (!isInt)
            {
                return false;
            }

            int value = Str.IntVal(number, maxInclusive + 1);
            return Num.IsBetween(value, minInclusive, maxInclusive);
        }
        #endregion

        #region [ Min | Max Filter ]
        /// <summary>
        /// Make sure the number do not goes below the minimum limit.
        /// </summary>
        /// <param name="number">The number.</param>
        /// <param name="minLimitInclusive">The min limit inclusive.</param>
        /// <returns>Return min limit value if the number is below the limit</returns>
        public static int MinFilter(int number, int minLimitInclusive)
        {
            return MinFilter(number, minLimitInclusive, minLimitInclusive);
        }

        /// <summary>
        /// Make sure the number do not goes below the minimum limit.
        /// </summary>
        /// <param name="number">The number.</param>
        /// <param name="minLimitInclusive">The min limit inclusive.</param>
        /// <param name="valueBeyondLimit">If the number goes below the limit return this number.</param>
        /// <returns></returns>
        public static int MinFilter(int number, int minLimitInclusive, int valueBeyondLimit)
        {
            if (number < minLimitInclusive)
            {
                return valueBeyondLimit;
            }

            return number;
        }

        /// <summary>
        /// Make sure the number do not exceed the limit (good for array index filter)
        /// </summary>
        /// <param name="number">The number.</param>
        /// <param name="maxLimitExclusive">The max limit (not inclusive).</param>
        /// <returns>Return maxLimit-1 beyond the maxLimit</returns>
        public static int MaxFilter(int number, int maxLimitExclusive)
        {
            return MaxFilter(number, maxLimitExclusive, maxLimitExclusive - 1);
        }

        /// <summary>
        /// Make sure the number do not exceed the limit (good for array index filter)
        /// </summary>
        /// <param name="number">The number.</param>
        /// <param name="maxLimitExclusive">The max limit (not inclusive).</param>
        /// <param name="valueBeyondLimit">The value beyond limit.</param>
        /// <returns></returns>
        public static int MaxFilter(int number, int maxLimitExclusive, int valueBeyondLimit)
        {
            if (number >= maxLimitExclusive)
            {
                return valueBeyondLimit;
            }

            return number;
        }
        #endregion

    }
}
