using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace Codout.Framework.NetStandard.Commom.Extensions
{
    /// <summary>
    /// Extensões comuns para tipos relacionadas a números.
    /// </summary>
    public static class Numeric
    {
        #region IsNaturalNumber
        /// <summary>
        /// Determines whether a number is a natural number (positive, non-decimal)
        /// </summary>
        /// <param name="sItem">The s item.</param>
        /// <returns>
        /// 	<c>true</c> if [is natural number] [the specified s item]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNaturalNumber(this string sItem)
        {
            var notNaturalPattern = new Regex("[^0-9]");
            var naturalPattern = new Regex("0*[1-9][0-9]*");

            return !notNaturalPattern.IsMatch(sItem) && naturalPattern.IsMatch(sItem);
        }
        #endregion

        #region IsWholeNumber
        /// <summary>
        /// Determines whether [is whole number] [the specified s item].
        /// </summary>
        /// <param name="sItem">The s item.</param>
        /// <returns>
        /// 	<c>true</c> if [is whole number] [the specified s item]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsWholeNumber(this string sItem)
        {
            var notWholePattern = new Regex("[^0-9]");
            return !notWholePattern.IsMatch(sItem);
        }
        #endregion

        #region IsInteger
        /// <summary>
        /// Determines whether the specified s item is integer.
        /// </summary>
        /// <param name="sItem">The s item.</param>
        /// <returns>
        /// 	<c>true</c> if the specified s item is integer; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsInteger(this string sItem)
        {
            var notIntPattern = new Regex("[^0-9-]");
            var intPattern = new Regex("^-[0-9]+$|^[0-9]+$");

            return !notIntPattern.IsMatch(sItem) && intPattern.IsMatch(sItem);
        }
        #endregion

        #region IsNumber
        /// <summary>
        /// Determines whether the specified s item is number.
        /// </summary>
        /// <param name="sItem">The s item.</param>
        /// <returns>
        /// 	<c>true</c> if the specified s item is number; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNumber(this string sItem)
        {
            double result;
            return (double.TryParse(sItem, NumberStyles.Float, NumberFormatInfo.CurrentInfo, out result));
        }
        #endregion

        #region IsEven
        /// <summary>
        /// Determines whether the specified value is an even number.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// 	<c>true</c> if the specified value is even; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsEven(this int value)
        {
            return ((value & 1) == 0);
        }
        #endregion

        #region IsOdd
        /// <summary>
        /// Determines whether the specified value is an odd number.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// 	<c>true</c> if the specified value is odd; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsOdd(this int value)
        {
            return ((value & 1) == 1);
        }
        #endregion

        #region Random
        /// <summary>
        /// Generates a random number with an upper bound
        /// </summary>
        /// <param name="high">The high.</param>
        /// <returns></returns>
        public static int Random(int high)
        {
            var random = new Byte[4];
            new RNGCryptoServiceProvider().GetBytes(random);
            int randomNumber = BitConverter.ToInt32(random, 0);

            return Math.Abs(randomNumber % high);
        }
        #endregion

        #region Random
        /// <summary>
        /// Generates a random number between the specified bounds
        /// </summary>
        /// <param name="low">The low.</param>
        /// <param name="high">The high.</param>
        /// <returns></returns>
        public static int Random(int low, int high)
        {
            return new Random().Next(low, high);
        }
        #endregion

        #region Random
        /// <summary>
        /// Generates a random double
        /// </summary>
        /// <returns></returns>
        public static double Random()
        {
            return new Random().NextDouble();
        }
        #endregion

        #region Truncate
        /// <summary>
        /// Trunca um valor especificando a quantidade de casas decimais.
        /// </summary>
        /// <param name="value">Valor a ser truncado.</param>
        /// <param name="precision">Quantidade de casas decimais.</param>
        /// <returns>O valor truncado.</returns>
        public static double Truncate(this double value, int precision)
        {
            var step = Math.Pow(10, precision);
            var tmp = Math.Truncate(step * value);
            return tmp / step;
        }
        #endregion
    }
}
