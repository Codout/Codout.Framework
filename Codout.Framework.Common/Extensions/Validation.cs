using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Codout.Framework.Common.Extensions
{
    /// <summary>
    /// Extensões comuns para tipos relacionadas a validações.
    /// </summary>
    public static class Validation
    {
        #region IsAlpha
        /// <summary>
        /// Determines whether the specified eval string contains only alpha characters.
        /// </summary>
        /// <param name="evalString">The eval string.</param>
        /// <returns>
        /// 	<c>true</c> if the specified eval string is alpha; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsAlpha(this string evalString)
        {
            return !Regex.IsMatch(evalString, RegexPattern.Alpha);
        }
        #endregion

        #region IsAlphaNumeric
        /// <summary>
        /// Determines whether the specified eval string contains only alphanumeric characters
        /// </summary>
        /// <param name="evalString">The eval string.</param>
        /// <returns>
        /// 	<c>true</c> if the string is alphanumeric; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsAlphaNumeric(this string evalString)
        {
            return !Regex.IsMatch(evalString, RegexPattern.AlphaNumeric);
        }
        #endregion

        #region IsAlphaNumeric
        /// <summary>
        /// Determines whether the specified eval string contains only alphanumeric characters
        /// </summary>
        /// <param name="evalString">The eval string.</param>
        /// <param name="allowSpaces">if set to <c>true</c> [allow spaces].</param>
        /// <returns>
        /// 	<c>true</c> if the string is alphanumeric; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsAlphaNumeric(this string evalString, bool allowSpaces)
        {
            if (allowSpaces)
                return !Regex.IsMatch(evalString, RegexPattern.AlphaNumericSpace);
            return IsAlphaNumeric(evalString);
        }
        #endregion

        #region IsNumeric
        /// <summary>
        /// Determines whether the specified eval string contains only numeric characters
        /// </summary>
        /// <param name="evalString">The eval string.</param>
        /// <returns>
        /// 	<c>true</c> if the string is numeric; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNumeric(this string evalString)
        {
            return !Regex.IsMatch(evalString, RegexPattern.Numeric);
        }
        #endregion

        #region IsEmail
        /// <summary>
        /// Determines whether the specified email address string is valid based on regular expression evaluation.
        /// </summary>
        /// <param name="emailAddressString">The email address string.</param>
        /// <returns>
        /// 	<c>true</c> if the specified email address is valid; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsEmail(this string emailAddressString)
        {
            return string.IsNullOrWhiteSpace(emailAddressString) || Regex.IsMatch(emailAddressString, RegexPattern.Email);
        }
        #endregion

        #region IsLowerCase
        /// <summary>
        /// Determines whether the specified string is lower case.
        /// </summary>
        /// <param name="inputString">The input string.</param>
        /// <returns>
        /// 	<c>true</c> if the specified string is lower case; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsLowerCase(this string inputString)
        {
            return Regex.IsMatch(inputString, RegexPattern.LowerCase);
        }
        #endregion

        #region IsUpperCase
        /// <summary>
        /// Determines whether the specified string is upper case.
        /// </summary>
        /// <param name="inputString">The input string.</param>
        /// <returns>
        /// 	<c>true</c> if the specified string is upper case; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsUpperCase(this string inputString)
        {
            return Regex.IsMatch(inputString, RegexPattern.UpperCase);
        }
        #endregion

        #region IsGuid
        /// <summary>
        /// Determines whether the specified string is a valid GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <returns>
        /// 	<c>true</c> if the specified string is a valid GUID; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsGuid(this string guid)
        {
            return Regex.IsMatch(guid, RegexPattern.Guid);
        }
        #endregion

        #region IsZipCodeAny
        /// <summary>
        /// Determines whether the specified string is a valid US Zip Code, using either 5 or 5+4 format.
        /// </summary>
        /// <param name="zipCode">The zip code.</param>
        /// <returns>
        /// 	<c>true</c> if it is a valid zip code; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsZipCodeAny(this string zipCode)
        {
            return Regex.IsMatch(zipCode, RegexPattern.UsZipcodePlusFourOptional);
        }
        #endregion

        #region IsZipCodeFive
        /// <summary>
        /// Determines whether the specified string is a valid US Zip Code, using the 5 digit format.
        /// </summary>
        /// <param name="zipCode">The zip code.</param>
        /// <returns>
        /// 	<c>true</c> if it is a valid zip code; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsZipCodeFive(this string zipCode)
        {
            return Regex.IsMatch(zipCode, RegexPattern.UsZipcode);
        }
        #endregion

        #region IsZipCodeFivePlusFour
        /// <summary>
        /// Determines whether the specified string is a valid US Zip Code, using the 5+4 format.
        /// </summary>
        /// <param name="zipCode">The zip code.</param>
        /// <returns>
        /// 	<c>true</c> if it is a valid zip code; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsZipCodeFivePlusFour(this string zipCode)
        {
            return Regex.IsMatch(zipCode, RegexPattern.UsZipcodePlusFour);
        }
        #endregion

        #region IsSocialSecurityNumber
        /// <summary>
        /// Determines whether the specified string is a valid Social Security number. Dashes are optional.
        /// </summary>
        /// <param name="socialSecurityNumber">The Social Security Number</param>
        /// <returns>
        /// 	<c>true</c> if it is a valid Social Security number; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsSocialSecurityNumber(this string socialSecurityNumber)
        {
            return Regex.IsMatch(socialSecurityNumber, RegexPattern.SocialSecurity);
        }
        #endregion

        #region IsIpAddress
        /// <summary>
        /// Determines whether the specified string is a valid IP address.
        /// </summary>
        /// <param name="ipAddress">The ip address.</param>
        /// <returns>
        /// 	<c>true</c> if valid; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsIpAddress(this string ipAddress)
        {
            return Regex.IsMatch(ipAddress, RegexPattern.IpAddress);
        }
        #endregion

        #region IsUsTelephoneNumber
        /// <summary>
        /// Determines whether the specified string is a valid US phone number using the referenced regex string.
        /// </summary>
        /// <param name="telephoneNumber">The telephone number.</param>
        /// <returns>
        /// 	<c>true</c> if valid; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsUsTelephoneNumber(this string telephoneNumber)
        {
            return Regex.IsMatch(telephoneNumber, RegexPattern.UsTelephone);
        }
        #endregion

        #region IsUsCurrency
        /// <summary>
        /// Determines whether the specified string is a valid currency string using the referenced regex string.
        /// </summary>
        /// <param name="currency">The currency string.</param>
        /// <returns>
        /// 	<c>true</c> if valid; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsUsCurrency(this string currency)
        {
            return Regex.IsMatch(currency, RegexPattern.UsCurrency);
        }
        #endregion

        #region IsUrl
        /// <summary>
        /// Determines whether the specified string is a valid URL string using the referenced regex string.
        /// </summary>
        /// <param name="url">The URL string.</param>
        /// <returns>
        /// 	<c>true</c> if valid; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsUrl(this string url)
        {
            return string.IsNullOrWhiteSpace(url) || Regex.IsMatch(url, RegexPattern.Url);
        }
        #endregion

        #region IsStrongPassword
        /// <summary>
        /// Determines whether the specified string is consider a strong password based on the supplied string.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <returns>
        /// 	<c>true</c> if strong; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsStrongPassword(this string password)
        {
            return Regex.IsMatch(password, RegexPattern.StrongPassword);
        }
        #endregion

        #region Credit Cards

        #region IsCreditCardAny
        /// <summary>
        /// Determines whether the specified string is a valid credit, based on matching any one of the eight credit card strings
        /// </summary>
        /// <param name="creditCard">The credit card.</param>
        /// <returns>
        /// 	<c>true</c> if valid; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsCreditCardAny(this string creditCard)
        {
            if (CreditPassesFormatCheck(creditCard))
            {
                creditCard = CleanCreditCardNumber(creditCard);
                return Regex.IsMatch(creditCard, RegexPattern.CreditCardAmericanExpress) ||
                       Regex.IsMatch(creditCard, RegexPattern.CreditCardCarteBlanche) ||
                       Regex.IsMatch(creditCard, RegexPattern.CreditCardDinersClub) ||
                       Regex.IsMatch(creditCard, RegexPattern.CreditCardDiscover) ||
                       Regex.IsMatch(creditCard, RegexPattern.CreditCardEnRoute) ||
                       Regex.IsMatch(creditCard, RegexPattern.CreditCardJcb) ||
                       Regex.IsMatch(creditCard, RegexPattern.CreditCardMasterCard) ||
                       Regex.IsMatch(creditCard, RegexPattern.CreditCardVisa);
            }
            return false;
        }
        #endregion

        #region IsCreditCardBigFour
        /// <summary>
        /// Determines whether the specified string is an American Express, Discover, MasterCard, or Visa
        /// </summary>
        /// <param name="creditCard">The credit card.</param>
        /// <returns>
        /// 	<c>true</c> if valid; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsCreditCardBigFour(this string creditCard)
        {
            if (CreditPassesFormatCheck(creditCard))
            {
                creditCard = CleanCreditCardNumber(creditCard);
                return Regex.IsMatch(creditCard, RegexPattern.CreditCardAmericanExpress) ||
                       Regex.IsMatch(creditCard, RegexPattern.CreditCardDiscover) ||
                       Regex.IsMatch(creditCard, RegexPattern.CreditCardMasterCard) ||
                       Regex.IsMatch(creditCard, RegexPattern.CreditCardVisa);
            }
            return false;
        }
        #endregion

        #region IsCreditCardAmericanExpress
        /// <summary>
        /// Determines whether the specified string is an American Express card
        /// </summary>
        /// <param name="creditCard">The credit card.</param>
        /// <returns>
        /// 	<c>true</c> if valid; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsCreditCardAmericanExpress(this string creditCard)
        {
            if (CreditPassesFormatCheck(creditCard))
            {
                creditCard = CleanCreditCardNumber(creditCard);
                return Regex.IsMatch(creditCard, RegexPattern.CreditCardAmericanExpress);
            }
            return false;
        }
        #endregion

        #region IsCreditCardCarteBlanche
        /// <summary>
        /// Determines whether the specified string is an Carte Blanche card
        /// </summary>
        /// <param name="creditCard">The credit card.</param>
        /// <returns>
        /// 	<c>true</c> if valid; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsCreditCardCarteBlanche(this string creditCard)
        {
            if (CreditPassesFormatCheck(creditCard))
            {
                creditCard = CleanCreditCardNumber(creditCard);
                return Regex.IsMatch(creditCard, RegexPattern.CreditCardCarteBlanche);
            }
            return false;
        }
        #endregion

        #region IsCreditCardDinersClub
        /// <summary>
        /// Determines whether the specified string is an Diner's Club card
        /// </summary>
        /// <param name="creditCard">The credit card.</param>
        /// <returns>
        /// 	<c>true</c> if valid; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsCreditCardDinersClub(this string creditCard)
        {
            if (CreditPassesFormatCheck(creditCard))
            {
                creditCard = CleanCreditCardNumber(creditCard);
                return Regex.IsMatch(creditCard, RegexPattern.CreditCardDinersClub);
            }
            return false;
        }
        #endregion

        #region IsCreditCardDiscover
        /// <summary>
        /// Determines whether the specified string is a Discover card
        /// </summary>
        /// <param name="creditCard">The credit card.</param>
        /// <returns>
        /// 	<c>true</c> if valid; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsCreditCardDiscover(this string creditCard)
        {
            if (CreditPassesFormatCheck(creditCard))
            {
                creditCard = CleanCreditCardNumber(creditCard);
                return Regex.IsMatch(creditCard, RegexPattern.CreditCardDiscover);
            }
            return false;
        }
        #endregion

        #region IsCreditCardEnRoute
        /// <summary>
        /// Determines whether the specified string is an En Route card
        /// </summary>
        /// <param name="creditCard">The credit card.</param>
        /// <returns>
        /// 	<c>true</c> if valid; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsCreditCardEnRoute(this string creditCard)
        {
            if (CreditPassesFormatCheck(creditCard))
            {
                creditCard = CleanCreditCardNumber(creditCard);
                return Regex.IsMatch(creditCard, RegexPattern.CreditCardEnRoute);
            }
            return false;
        }
        #endregion

        #region IsCreditCardJcb
        /// <summary>
        /// Determines whether the specified string is an JCB card
        /// </summary>
        /// <param name="creditCard">The credit card.</param>
        /// <returns>
        /// 	<c>true</c> if valid; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsCreditCardJcb(this string creditCard)
        {
            if (CreditPassesFormatCheck(creditCard))
            {
                creditCard = CleanCreditCardNumber(creditCard);
                return Regex.IsMatch(creditCard, RegexPattern.CreditCardJcb);
            }
            return false;
        }
        #endregion

        #region IsCreditCardMasterCard
        /// <summary>
        /// Determines whether the specified string is a Master Card credit card
        /// </summary>
        /// <param name="creditCard">The credit card.</param>
        /// <returns>
        /// 	<c>true</c> if valid; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsCreditCardMasterCard(this string creditCard)
        {
            if (CreditPassesFormatCheck(creditCard))
            {
                creditCard = CleanCreditCardNumber(creditCard);
                return Regex.IsMatch(creditCard, RegexPattern.CreditCardMasterCard);
            }
            return false;
        }
        #endregion

        #region IsCreditCardVisa
        /// <summary>
        /// Determines whether the specified string is Visa card.
        /// </summary>
        /// <param name="creditCard">The credit card.</param>
        /// <returns>
        /// 	<c>true</c> if valid; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsCreditCardVisa(this string creditCard)
        {
            if (CreditPassesFormatCheck(creditCard))
            {
                creditCard = CleanCreditCardNumber(creditCard);
                return Regex.IsMatch(creditCard, RegexPattern.CreditCardVisa);
            }
            return false;
        }
        #endregion

        #region CleanCreditCardNumber
        /// <summary>
        /// Cleans the credit card number, returning just the numeric values.
        /// </summary>
        /// <param name="creditCard">The credit card.</param>
        /// <returns></returns>
        public static string CleanCreditCardNumber(this string creditCard)
        {
            var regex = new Regex(RegexPattern.CreditCardStripNonNumeric, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            return regex.Replace(creditCard, String.Empty);
        }
        #endregion

        #region CreditPassesFormatCheck
        /// <summary>
        /// Determines whether the credit card number, once cleaned, passes the Luhn algorith.
        /// See: http://en.wikipedia.org/wiki/Luhn_algorithm
        /// </summary>
        /// <param name="creditCardNumber">The credit card number.</param>
        /// <returns></returns>
        private static bool CreditPassesFormatCheck(this string creditCardNumber)
        {
            creditCardNumber = CleanCreditCardNumber(creditCardNumber);
            if (creditCardNumber.IsInteger())
            {
                var numArray = new int[creditCardNumber.Length];
                for (int i = 0; i < numArray.Length; i++)
                    numArray[i] = Convert.ToInt16(creditCardNumber[i].ToString(CultureInfo.InvariantCulture));

                return IsValidLuhn(numArray);
            }
            return false;
        }
        #endregion

        #region IsValidLuhn
        /// <summary>
        /// Determines whether the specified int array passes the Luhn algorith
        /// </summary>
        /// <param name="digits">The int array to evaluate</param>
        /// <returns>
        /// 	<c>true</c> if it validates; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsValidLuhn(this int[] digits)
        {
            int sum = 0;
            bool alt = false;
            for (int i = digits.Length - 1; i >= 0; i--)
            {
                if (alt)
                {
                    digits[i] *= 2;
                    if (digits[i] > 9)
                        digits[i] -= 9; // equivalent to adding the value of digits
                }
                sum += digits[i];
                alt = !alt;
            }
            return sum % 10 == 0;
        }
        #endregion

        #region IsStringNumeric
        /// <summary>
        /// Determine whether the passed string is numeric, by attempting to parse it to a double
        /// </summary>
        /// <param name="str">The string to evaluated for numeric conversion</param>
        /// <returns>
        /// 	<c>true</c> if the string can be converted to a number; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsStringNumeric(this string str)
        {
            double result;
            return (double.TryParse(str, NumberStyles.Float, NumberFormatInfo.CurrentInfo, out result));
        }
        #endregion

        #endregion

        #region Cep
        /// <summary>
        /// Verifica se o CEP está no formato correto.
        /// </summary>
        /// <param name="input">Número do cep</param>
        /// <returns>Verdadeiro se estiver no formato correto</returns>
        public static bool IsCep(this string input)
        {
            const string pattern = @"^\d{2}[\.]?\d{3}-?\d{3}$";
            return Regex.IsMatch(input, pattern);
        }

        #endregion

        #region IsCpf
        /// <summary>
        /// Valida o cpf.
        /// </summary>
        /// <param name="cpf">valor a ser verificado como CPF</param>
        /// <returns>returna true caso seja CPF valido ou false em caso contrário</returns>
        public static bool IsCpf(this string cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf))
                return false;

            cpf = cpf.Replace(".", String.Empty).Replace("-", String.Empty).Trim();

            if (cpf.Length != 11)
                return false;

            switch (cpf)
            {
                case "00000000000":
                case "11111111111":
                case "22222222222":
                case "33333333333":
                case "44444444444":
                case "55555555555":
                case "66666666666":
                case "77777777777":
                case "88888888888":
                case "99999999999":
                    return false;
            }

            var soma = 0;
            for (int i = 0, j = 10; i < 9; i++, j--)
            {
                int d;
                if (!Int32.TryParse(cpf[i].ToString(CultureInfo.InvariantCulture), out d))
                    return false;
                soma += d * j;
            }

            var resto = soma % 11;

            var digito = (resto < 2 ? 0 : 11 - resto).ToString(CultureInfo.InvariantCulture);
            var prefixo = cpf.Substring(0, 9) + digito;

            soma = 0;
            for (int i = 0, j = 11; i < 10; i++, j--)
                soma += Int32.Parse(prefixo[i].ToString(CultureInfo.InvariantCulture)) * j;

            resto = soma % 11;
            digito += (resto < 2 ? 0 : 11 - resto).ToString(CultureInfo.InvariantCulture);

            return cpf.EndsWith(digito);
        }
        #endregion

        #region IsCnpj
        /// <summary>
        /// Valida o cnpj.
        /// </summary>
        /// <param name="cnpj">valor a ser verificado como CNPJ</param>
        /// <returns>returna true caso seja CNPJ valido ou false em caso contrário</returns>
        public static bool IsCnpj(this string cnpj)
        {
            var multiplicador1 = new[] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            var multiplicador2 = new[] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

            cnpj = cnpj.Trim();
            cnpj = cnpj.Replace(".", "").Replace("-", "").Replace("/", "");

            if (cnpj.Length != 14)
                return false;

            var tempCnpj = cnpj.Substring(0, 12);

            var soma = 0;
            for (var i = 0; i < 12; i++)
                soma += int.Parse(tempCnpj[i].ToString(CultureInfo.InvariantCulture)) * multiplicador1[i];

            var resto = (soma % 11);
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            var digito = resto.ToString(CultureInfo.InvariantCulture);

            tempCnpj = tempCnpj + digito;
            soma = 0;
            for (var i = 0; i < 13; i++)
                soma += int.Parse(tempCnpj[i].ToString(CultureInfo.InvariantCulture)) * multiplicador2[i];

            resto = (soma % 11);
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            digito = digito + resto;

            return cnpj.EndsWith(digito);
        }
        #endregion
    }
}
