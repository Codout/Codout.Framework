using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Codout.Framework.Common.Extensions;

/// <summary>
///     Extensões comuns para tipos relacionadas a validações.
/// </summary>
public static class Validation
{
    #region IsAlpha

    /// <summary>
    ///     Determines whether the specified eval string contains only alpha characters.
    /// </summary>
    /// <param name="evalString">The eval string.</param>
    /// <returns>
    ///     <c>true</c> if the specified eval string is alpha; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsAlpha(this string evalString)
    {
        return !Regex.IsMatch(evalString, RegexPattern.Alpha);
    }

    #endregion

    #region IsAlphaNumeric

    /// <summary>
    ///     Determines whether the specified eval string contains only alphanumeric characters
    /// </summary>
    /// <param name="evalString">The eval string.</param>
    /// <returns>
    ///     <c>true</c> if the string is alphanumeric; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsAlphaNumeric(this string evalString)
    {
        return !Regex.IsMatch(evalString, RegexPattern.AlphaNumeric);
    }

    #endregion

    #region IsAlphaNumeric

    /// <summary>
    ///     Determines whether the specified eval string contains only alphanumeric characters
    /// </summary>
    /// <param name="evalString">The eval string.</param>
    /// <param name="allowSpaces">if set to <c>true</c> [allow spaces].</param>
    /// <returns>
    ///     <c>true</c> if the string is alphanumeric; otherwise, <c>false</c>.
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
    ///     Determines whether the specified eval string contains only numeric characters
    /// </summary>
    /// <param name="evalString">The eval string.</param>
    /// <returns>
    ///     <c>true</c> if the string is numeric; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsNumeric(this string evalString)
    {
        return !Regex.IsMatch(evalString, RegexPattern.Numeric);
    }

    #endregion

    #region IsEmail

    /// <summary>
    ///     Determines whether the specified email address string is valid based on regular expression evaluation.
    /// </summary>
    /// <param name="emailAddressString">The email address string.</param>
    /// <returns>
    ///     <c>true</c> if the specified email address is valid; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsEmail(this string emailAddressString)
    {
        return string.IsNullOrWhiteSpace(emailAddressString) || Regex.IsMatch(emailAddressString, RegexPattern.Email);
    }

    #endregion

    #region IsLowerCase

    /// <summary>
    ///     Determines whether the specified string is lower case.
    /// </summary>
    /// <param name="inputString">The input string.</param>
    /// <returns>
    ///     <c>true</c> if the specified string is lower case; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsLowerCase(this string inputString)
    {
        return Regex.IsMatch(inputString, RegexPattern.LowerCase);
    }

    #endregion

    #region IsUpperCase

    /// <summary>
    ///     Determines whether the specified string is upper case.
    /// </summary>
    /// <param name="inputString">The input string.</param>
    /// <returns>
    ///     <c>true</c> if the specified string is upper case; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsUpperCase(this string inputString)
    {
        return Regex.IsMatch(inputString, RegexPattern.UpperCase);
    }

    #endregion

    #region IsGuid

    /// <summary>
    ///     Determines whether the specified string is a valid GUID.
    /// </summary>
    /// <param name="guid">The GUID.</param>
    /// <returns>
    ///     <c>true</c> if the specified string is a valid GUID; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsGuid(this string guid)
    {
        return Regex.IsMatch(guid, RegexPattern.Guid);
    }

    #endregion

    #region IsZipCodeAny

    /// <summary>
    ///     Determines whether the specified string is a valid US Zip Code, using either 5 or 5+4 format.
    /// </summary>
    /// <param name="zipCode">The zip code.</param>
    /// <returns>
    ///     <c>true</c> if it is a valid zip code; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsZipCodeAny(this string zipCode)
    {
        return Regex.IsMatch(zipCode, RegexPattern.UsZipcodePlusFourOptional);
    }

    #endregion

    #region IsZipCodeFive

    /// <summary>
    ///     Determines whether the specified string is a valid US Zip Code, using the 5 digit format.
    /// </summary>
    /// <param name="zipCode">The zip code.</param>
    /// <returns>
    ///     <c>true</c> if it is a valid zip code; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsZipCodeFive(this string zipCode)
    {
        return Regex.IsMatch(zipCode, RegexPattern.UsZipcode);
    }

    #endregion

    #region IsZipCodeFivePlusFour

    /// <summary>
    ///     Determines whether the specified string is a valid US Zip Code, using the 5+4 format.
    /// </summary>
    /// <param name="zipCode">The zip code.</param>
    /// <returns>
    ///     <c>true</c> if it is a valid zip code; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsZipCodeFivePlusFour(this string zipCode)
    {
        return Regex.IsMatch(zipCode, RegexPattern.UsZipcodePlusFour);
    }

    #endregion

    #region IsSocialSecurityNumber

    /// <summary>
    ///     Determines whether the specified string is a valid Social Security number. Dashes are optional.
    /// </summary>
    /// <param name="socialSecurityNumber">The Social Security Number</param>
    /// <returns>
    ///     <c>true</c> if it is a valid Social Security number; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsSocialSecurityNumber(this string socialSecurityNumber)
    {
        return Regex.IsMatch(socialSecurityNumber, RegexPattern.SocialSecurity);
    }

    #endregion

    #region IsIpAddress

    /// <summary>
    ///     Determines whether the specified string is a valid IP address.
    /// </summary>
    /// <param name="ipAddress">The ip address.</param>
    /// <returns>
    ///     <c>true</c> if valid; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsIpAddress(this string ipAddress)
    {
        return Regex.IsMatch(ipAddress, RegexPattern.IpAddress);
    }

    #endregion

    #region IsUsTelephoneNumber

    /// <summary>
    ///     Determines whether the specified string is a valid US phone number using the referenced regex string.
    /// </summary>
    /// <param name="telephoneNumber">The telephone number.</param>
    /// <returns>
    ///     <c>true</c> if valid; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsUsTelephoneNumber(this string telephoneNumber)
    {
        return Regex.IsMatch(telephoneNumber, RegexPattern.UsTelephone);
    }

    #endregion

    #region IsUsCurrency

    /// <summary>
    ///     Determines whether the specified string is a valid currency string using the referenced regex string.
    /// </summary>
    /// <param name="currency">The currency string.</param>
    /// <returns>
    ///     <c>true</c> if valid; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsUsCurrency(this string currency)
    {
        return Regex.IsMatch(currency, RegexPattern.UsCurrency);
    }

    #endregion

    #region IsUrl

    /// <summary>
    ///     Determines whether the specified string is a valid URL string using the referenced regex string.
    /// </summary>
    /// <param name="url">The URL string.</param>
    /// <returns>
    ///     <c>true</c> if valid; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsUrl(this string url)
    {
        return string.IsNullOrWhiteSpace(url) || Regex.IsMatch(url, RegexPattern.Url);
    }

    #endregion

    #region IsStrongPassword

    /// <summary>
    ///     Determines whether the specified string is consider a strong password based on the supplied string.
    /// </summary>
    /// <param name="password">The password.</param>
    /// <returns>
    ///     <c>true</c> if strong; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsStrongPassword(this string password)
    {
        return Regex.IsMatch(password, RegexPattern.StrongPassword);
    }

    #endregion

    #region Cep

    /// <summary>
    ///     Verifica se o CEP está no formato correto.
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
    ///     Valida o cpf.
    /// </summary>
    /// <param name="cpf">valor a ser verificado como CPF</param>
    /// <returns>returna true caso seja CPF valido ou false em caso contrário</returns>
    public static bool IsCpf(this string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            return false;

        cpf = cpf.Replace(".", string.Empty).Replace("-", string.Empty).Trim();

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
            if (!int.TryParse(cpf[i].ToString(CultureInfo.InvariantCulture), out var d))
                return false;
            soma += d * j;
        }

        var resto = soma % 11;

        var digito = (resto < 2 ? 0 : 11 - resto).ToString(CultureInfo.InvariantCulture);
        var prefixo = cpf.Substring(0, 9) + digito;

        soma = 0;
        for (int i = 0, j = 11; i < 10; i++, j--)
            soma += int.Parse(prefixo[i].ToString(CultureInfo.InvariantCulture)) * j;

        resto = soma % 11;
        digito += (resto < 2 ? 0 : 11 - resto).ToString(CultureInfo.InvariantCulture);

        return cpf.EndsWith(digito);
    }

    #endregion

    #region IsCnpj

    /// <summary>
    ///     Valida o cnpj.
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

        var resto = soma % 11;
        if (resto < 2)
            resto = 0;
        else
            resto = 11 - resto;

        var digito = resto.ToString(CultureInfo.InvariantCulture);

        tempCnpj = tempCnpj + digito;
        soma = 0;
        for (var i = 0; i < 13; i++)
            soma += int.Parse(tempCnpj[i].ToString(CultureInfo.InvariantCulture)) * multiplicador2[i];

        resto = soma % 11;
        if (resto < 2)
            resto = 0;
        else
            resto = 11 - resto;

        digito = digito + resto;

        return cnpj.EndsWith(digito);
    }

    #endregion

    #region IsInscricaoEstadual

    /// <summary>
    ///     Valida o número da inscrição estadual
    /// </summary>
    /// <param name="pUF">UF da empresa</param>
    /// <param name="pInscr">Número da inscrição estadual</param>
    /// <returns></returns>
    public static bool IsInscricaoEstadual(string pUF, string pInscr)
    {
        var retorno = false;

        string strBase;

        string strBase2;

        string strOrigem;

        string strDigito1;

        string strDigito2;

        int intPos;

        int intValor;

        var intSoma = 0;

        int intResto;

        int intNumero;

        int intPeso;

        int intDig;

        strBase = "";

        strBase2 = "";

        strOrigem = "";

        pInscr = pInscr.Replace(".", "");
        pInscr = pInscr.Replace("/", "");
        pInscr = pInscr.Replace("-", "");

        if (pInscr.Trim().ToUpper() == "ISENTO") return true;

        for (intPos = 1; intPos <= pInscr.Trim().Length; intPos++)
            if ("0123456789P".IndexOf(pInscr.Substring(intPos - 1, 1), 0, StringComparison.OrdinalIgnoreCase) + 1
                > 0)
                strOrigem = strOrigem + pInscr.Substring(intPos - 1, 1);


        // Retira caracteres de formatação
        strOrigem = strOrigem.Replace(".", "");
        strOrigem = strOrigem.Replace("-", "");
        strOrigem = strOrigem.Replace("/", "");

        strOrigem = strOrigem.Trim();

        switch (pUF.ToUpper())
        {
            case "AC": // OK - Validado

                strBase = (strOrigem.Trim() + "00000000000").Substring(0, 11);

                intSoma = 0;

                intPeso = 2;

                for (intPos = strBase.Length; intPos >= 1; intPos--)
                {
                    intValor = int.Parse(strBase.Substring(intPos - 1, 1));

                    intValor = intValor * intPeso;

                    intSoma = intSoma + intValor;

                    intPeso = intPeso + 1;

                    if (intPeso > 9) intPeso = 2;
                }

                intResto = intSoma % 11;

                if (11 - intResto == 10 || 11 - intResto == 11)
                    strDigito1 = "0";
                else
                    strDigito1 = (11 - intResto).ToString();


                strBase2 = strBase + strDigito1;

                intSoma = 0;

                intPeso = 2;

                for (intPos = strBase2.Length; intPos >= 1; intPos--)
                {
                    intValor = int.Parse(strBase2.Substring(intPos - 1, 1));

                    intValor = intValor * intPeso;

                    intSoma = intSoma + intValor;

                    intPeso = intPeso + 1;

                    if (intPeso > 9) intPeso = 2;
                }

                intResto = intSoma % 11;

                if (11 - intResto == 10 || 11 - intResto == 11)
                    strDigito2 = "0";
                else
                    strDigito2 = (11 - intResto).ToString();

                strBase2 = strBase2 + strDigito2;


                if (strBase2 == strOrigem) retorno = true;


                break;

            case "AL": // OK - Validado

                strBase = (strOrigem.Trim() + "000000000").Substring(0, 8);

                if (strBase.Substring(0, 2) == "24" && (strBase.Substring(2, 1) == "0" ||
                                                        strBase.Substring(2, 1) == "3" ||
                                                        strBase.Substring(2, 1) == "5" ||
                                                        strBase.Substring(2, 1) == "7" ||
                                                        strBase.Substring(2, 1) == "8"))
                {
                    intSoma = 0;
                    intPeso = 2;

                    for (intPos = strBase.Length; intPos >= 1; intPos--)
                    {
                        intValor = int.Parse(strBase.Substring(intPos - 1, 1));

                        intValor = intValor * intPeso;

                        intSoma = intSoma + intValor;

                        intPeso = intPeso + 1;

                        if (intPeso > 9) intPeso = 2;
                    }

                    intSoma = intSoma * 10;

                    intResto = intSoma % 11;

                    if (intResto == 10)
                        strDigito1 = "0";
                    else
                        strDigito1 = intResto.ToString();

                    strBase2 = strBase.Substring(0, 8) + strDigito1;

                    if (strBase2 == strOrigem) retorno = true;
                }

                break;

            case "AM": // OK - Validado

                strBase = (strOrigem.Trim() + "000000000").Substring(0, 8);

                intSoma = 0;
                intPeso = 2;

                for (intPos = strBase.Length; intPos >= 1; intPos--)
                {
                    intValor = int.Parse(strBase.Substring(intPos - 1, 1));

                    intValor = intValor * intPeso;

                    intSoma = intSoma + intValor;

                    intPeso = intPeso + 1;

                    if (intPeso > 9) intPeso = 2;
                }

                if (intSoma < 11)
                {
                    strDigito1 = (11 - intSoma).ToString();
                }

                else
                {
                    intResto = intSoma % 11;

                    if (intResto <= 1)
                        strDigito1 = "0";
                    else
                        strDigito1 = (11 - intResto).ToString();
                }

                strBase2 = strBase.Substring(0, 8) + strDigito1;

                if (strBase2 == strOrigem) retorno = true;

                break;

            case "AP": // OK - Validado

                strBase = (strOrigem.Trim() + "000000000").Substring(0, 8);

                intPeso = 0;

                intDig = 0;

                if (strBase.Substring(0, 2) == "03")
                {
                    intNumero = int.Parse(strBase.Substring(0, 8));

                    if (intNumero >= 3000001
                        && intNumero <= 3017000)
                    {
                        intPeso = 5;

                        intDig = 0;
                    }

                    else if (intNumero >= 3017001
                             && intNumero <= 3019022)
                    {
                        intPeso = 9;

                        intDig = 1;
                    }

                    else if (intNumero >= 3019023)
                    {
                        intPeso = 0;

                        intDig = 0;
                    }

                    intSoma = intPeso;
                    intPeso = 2;

                    for (intPos = strBase.Length; intPos >= 1; intPos--)
                    {
                        intValor = int.Parse(strBase.Substring(intPos - 1, 1));

                        intValor = intValor * intPeso;

                        intSoma = intSoma + intValor;

                        intPeso = intPeso + 1;
                    }

                    intResto = intSoma % 11;

                    intValor = 11 - intResto;

                    if (intValor == 10)
                        intValor = 0;

                    else if (intValor == 11) intValor = intDig;

                    strDigito1 = intValor.ToString();

                    strBase2 = strBase + strDigito1;

                    if (strBase2 == strOrigem) retorno = true;
                }

                break;

            case "BA": // OK - Validado

                if (strOrigem.Length == 8)
                {
                    strBase = (strOrigem.Trim() + "00000000").Substring(0, 8);

                    if ("0123458".IndexOf(strBase.Substring(0, 1), 0, StringComparison.OrdinalIgnoreCase) + 1 > 0)
                    {
                        intSoma = 0;

                        for (intPos = 1; intPos <= 6; intPos++)
                        {
                            intValor = int.Parse(strBase.Substring(intPos - 1, 1));

                            intValor = intValor * (8 - intPos);

                            intSoma = intSoma + intValor;
                        }
                    }

                    intResto = intSoma % 10;

                    strDigito2 =
                        (intResto == 0 ? "0" : Convert.ToString(10 - intResto)).Substring(
                            (intResto == 0 ? "0" : Convert.ToString(10 - intResto)).Length - 1);

                    strBase2 = strBase.Substring(0, 6) + strDigito2;

                    intSoma = 0;

                    for (intPos = 1; intPos <= 7; intPos++)
                    {
                        intValor = int.Parse(strBase2.Substring(intPos - 1, 1));

                        intValor = intValor * (9 - intPos);

                        intSoma = intSoma + intValor;
                    }

                    intResto = intSoma % 10;

                    strDigito1 =
                        (intResto == 0 ? "0" : Convert.ToString(10 - intResto)).Substring(
                            (intResto == 0 ? "0" : Convert.ToString(10 - intResto)).Length - 1);
                }

                else
                {
                    intSoma = 0;

                    for (intPos = 1; intPos <= 6; intPos++)
                    {
                        intValor = int.Parse(strBase.Substring(intPos - 1, 1));

                        intValor = intValor * (8 - intPos);

                        intSoma = intSoma + intValor;
                    }

                    intResto = intSoma % 11;

                    strDigito2 =
                        (intResto < 2 ? "0" : Convert.ToString(11 - intResto)).Substring(
                            (intResto < 2 ? "0" : Convert.ToString(11 - intResto)).Length - 1);

                    strBase2 = strBase.Substring(0, 6) + strDigito2;

                    intSoma = 0;

                    for (intPos = 1; intPos <= 7; intPos++)
                    {
                        intValor = int.Parse(strBase2.Substring(intPos - 1, 1));

                        intValor = intValor * (9 - intPos);

                        intSoma = intSoma + intValor;
                    }

                    intResto = intSoma % 11;

                    strDigito1 =
                        (intResto < 2 ? "0" : Convert.ToString(11 - intResto)).Substring(
                            (intResto < 2 ? "0" : Convert.ToString(11 - intResto)).Length - 1);
                }

                strBase2 = strBase.Substring(0, 6)
                           + strDigito1 + strDigito2;


                if (strBase2 == strOrigem) retorno = true;

                break;

            case "CE": // OK - Validado

                strBase = (strOrigem.Trim() + "000000000").Substring(0, 9);

                intSoma = 0;

                for (intPos = 1; intPos <= 8; intPos++)
                {
                    intValor = int.Parse(strBase.Substring(intPos - 1, 1));

                    intValor = intValor * (10 - intPos);

                    intSoma = intSoma + intValor;
                }

                intResto = intSoma % 11;

                intValor = 11 - intResto;

                if (intValor > 9) intValor = 0;

                strDigito1 = Convert.ToString(intValor).Substring(Convert.ToString(intValor).Length - 1);

                strBase2 = strBase.Substring(0, 8) + strDigito1;

                if (strBase2 == strOrigem) retorno = true;

                break;

            case "DF": // OK - Validado

                strBase = Convert.ToInt64(strOrigem.Trim()).ToString("0000000000000").Substring(0, 11);

                if (strBase.Substring(0, 3) == "073")
                {
                    intSoma = 0;
                    intPeso = 2;

                    for (intPos = strBase.Length; intPos >= 1; intPos--)
                    {
                        intValor = int.Parse(strBase.Substring(intPos - 1, 1));

                        intValor = intValor * intPeso;

                        intSoma = intSoma + intValor;

                        intPeso = intPeso + 1;

                        if (intPeso > 9) intPeso = 2;
                    }

                    intResto = intSoma % 11;

                    strDigito1 =
                        (intResto < 2 ? "0" : Convert.ToString(11 - intResto)).Substring(
                            (intResto < 2 ? "0" : Convert.ToString(11 - intResto)).Length - 1);

                    strBase2 = strBase.Substring(0, 11) + strDigito1;

                    intSoma = 0;
                    intPeso = 2;

                    for (intPos = strBase2.Length; intPos >= 1; intPos--)
                    {
                        intValor = int.Parse(strBase2.Substring(intPos - 1, 1));

                        intValor = intValor * intPeso;

                        intSoma = intSoma + intValor;

                        intPeso = intPeso + 1;

                        if (intPeso > 9) intPeso = 2;
                    }


                    intResto = intSoma % 11;

                    strDigito2 =
                        (intResto < 2 ? "0" : Convert.ToString(11 - intResto)).Substring(
                            (intResto < 2 ? "0" : Convert.ToString(11 - intResto)).Length - 1);

                    strBase2 = strBase2 + strDigito2;

                    if (strBase2 == strOrigem) retorno = true;
                }

                break;

            case "ES": // OK - Validado

                strBase = Convert.ToInt64(strOrigem).ToString("000000000").Substring(0, 8);

                intSoma = 0;

                for (intPos = 1; intPos <= 8; intPos++)
                {
                    intValor = int.Parse(strBase.Substring(intPos - 1, 1));

                    intValor = intValor * (10 - intPos);

                    intSoma = intSoma + intValor;
                }

                intResto = intSoma % 11;

                strDigito1 =
                    (intResto < 2 ? "0" : Convert.ToString(11 - intResto)).Substring(
                        (intResto < 2 ? "0" : Convert.ToString(11 - intResto)).Length - 1);

                strBase2 = strBase.Substring(0, 8) + strDigito1;

                if (strBase2 == strOrigem) retorno = true;

                break;

            case "GO": // OK - Validado

                strBase = (strOrigem.Trim() + "000000000").Substring(0, 8);

                if ("10,11,15".IndexOf(strBase.Substring(0, 2), 0, StringComparison.OrdinalIgnoreCase) + 1
                    > 0)
                {
                    intSoma = 0;

                    for (intPos = 1; intPos <= 8; intPos++)
                    {
                        intValor = int.Parse(strBase.Substring(intPos - 1, 1));

                        intValor = intValor * (10 - intPos);

                        intSoma = intSoma + intValor;
                    }

                    intResto = intSoma % 11;

                    if (intResto == 0)
                    {
                        strDigito1 = "0";
                    }

                    else if (intResto == 1)
                    {
                        intNumero = int.Parse(strBase.Substring(0, 8));

                        strDigito1 = (intNumero >= 10103105
                                      && intNumero <= 10119997
                            ? "1"
                            : "0").Substring((intNumero >= 10103105
                                              && intNumero <= 10119997
                            ? "1"
                            : "0").Length - 1);
                    }

                    else
                    {
                        strDigito1 = Convert.ToString(11 - intResto)
                            .Substring(Convert.ToString(11 - intResto).Length - 1);
                    }

                    strBase2 = strBase.Substring(0, 8) + strDigito1;

                    if (strBase2 == strOrigem) retorno = true;
                }

                break;

            case "MA": // OK - Validado

                strBase = (strOrigem.Trim() + "000000000").Substring(0, 8);

                if (strBase.Substring(0, 2) == "12")
                {
                    intSoma = 0;

                    for (intPos = 1; intPos <= 8; intPos++)
                    {
                        intValor = int.Parse(strBase.Substring(intPos - 1, 1));

                        intValor = intValor * (10 - intPos);

                        intSoma = intSoma + intValor;
                    }

                    intResto = intSoma % 11;

                    strDigito1 =
                        (intResto < 2 ? "0" : Convert.ToString(11 - intResto)).Substring(
                            (intResto < 2 ? "0" : Convert.ToString(11 - intResto)).Length - 1);

                    strBase2 = strBase.Substring(0, 8) + strDigito1;

                    if (strBase2 == strOrigem) retorno = true;
                }

                break;

            case "MT": // OK - Validado

                strBase = (strOrigem.Trim() + "0000000000").Substring(0, 10);

                intSoma = 0;
                intPeso = 2;

                for (intPos = strBase.Length; intPos >= 1; intPos--)
                {
                    intValor = int.Parse(strBase.Substring(intPos - 1, 1));

                    intValor = intValor * intPeso;

                    intSoma = intSoma + intValor;

                    intPeso = intPeso + 1;

                    if (intPeso > 9) intPeso = 2;
                }

                intResto = intSoma % 11;

                strDigito1 =
                    (intResto < 2 ? "0" : Convert.ToString(11 - intResto)).Substring(
                        (intResto < 2 ? "0" : Convert.ToString(11 - intResto)).Length - 1);

                strBase2 = strBase.Substring(0, 10) + strDigito1;

                if (strBase2 == strOrigem) retorno = true;

                break;

            case "MS": // OK - Validado

                strBase = (strOrigem.Trim() + "000000000").Substring(0, 8);

                if (strBase.Substring(0, 2) == "28")
                {
                    intSoma = 0;

                    for (intPos = 1; intPos <= 8; intPos++)
                    {
                        intValor = int.Parse(strBase.Substring(intPos - 1, 1));

                        intValor = intValor * (10 - intPos);

                        intSoma = intSoma + intValor;
                    }

                    intResto = intSoma % 11;

                    strDigito1 =
                        (intResto < 2 ? "0" : Convert.ToString(11 - intResto)).Substring(
                            (intResto < 2 ? "0" : Convert.ToString(11 - intResto)).Length - 1);

                    strBase2 = strBase.Substring(0, 8) + strDigito1;

                    if (strBase2 == strOrigem) retorno = true;
                }

                break;

            case "MG":

                strBase = (strOrigem.Trim() + "0000000000000").Substring(0, 11);

                strBase2 = strBase.Substring(0, 3) + "0" + strBase.Substring(3, 8);

                intNumero = 2;
                intSoma = 0;

                for (intPos = 1; intPos <= 12; intPos++)
                {
                    intValor = int.Parse(strBase2.Substring(intPos - 1, 1));

                    intNumero = intNumero == 2 ? 1 : 2;

                    intValor = intValor * intNumero;

                    intSoma = intSoma + intNumero;

                    if (intValor > 9)
                    {
                        strDigito1 = intValor.ToString("00");

                        intValor = int.Parse(strDigito1.Substring(0, 1)) + int.Parse(strDigito1.Substring(1, 1));
                    }

                    intSoma = intSoma + intValor;
                }

                break;

            case "PA":

                strBase = (strOrigem.Trim() + "000000000").Substring(0, 9);

                if (strBase.Substring(0, 2) == "15")
                {
                    intSoma = 0;

                    for (intPos = 1; intPos <= 8; intPos++)
                    {
                        intValor = int.Parse(strBase.Substring(intPos - 1, 1));

                        intValor = intValor * (10 - intPos);

                        intSoma = intSoma + intValor;
                    }

                    intResto = intSoma % 11;

                    strDigito1 =
                        (intResto < 2 ? "0" : Convert.ToString(11 - intResto)).Substring(
                            (intResto < 2 ? "0" : Convert.ToString(11 - intResto)).Length - 1);

                    strBase2 = strBase.Substring(0, 8) + strDigito1;

                    if (strBase2 == strOrigem) retorno = true;
                }

                break;

            case "PB":

                strBase = (strOrigem.Trim() + "000000000").Substring(0, 9);

                intSoma = 0;

                for (intPos = 1; intPos <= 8; intPos++)
                {
                    intValor = int.Parse(strBase.Substring(intPos - 1, 1));

                    intValor = intValor * (10 - intPos);

                    intSoma = intSoma + intValor;
                }

                intResto = intSoma % 11;

                intValor = 11 - intResto;

                if (intValor > 9) intValor = 0;

                strDigito1 = Convert.ToString(intValor).Substring(Convert.ToString(intValor).Length - 1);

                strBase2 = strBase.Substring(0, 8) + strDigito1;

                if (strBase2 == strOrigem) retorno = true;

                break;

            case "PE":

                strBase = (strOrigem.Trim() + "00000000000000").Substring(0, 14);

                intSoma = 0;

                intPeso = 2;

                for (intPos = 13; intPos <= 1; intPos = intPos + -1)
                {
                    intValor = int.Parse(strBase.Substring(intPos - 1, 1));

                    intValor = intValor * intPeso;

                    intSoma = intSoma + intValor;

                    intPeso = intPeso + 1;

                    if (intPeso > 9) intPeso = 2;
                }

                intResto = intSoma % 11;

                intValor = 11 - intResto;

                if (intValor > 9) intValor = intValor - 10;

                strDigito1 = Convert.ToString(intValor).Substring(Convert.ToString(intValor).Length - 1);

                strBase2 = strBase.Substring(0, 13) + strDigito1;

                if (strBase2 == strOrigem) retorno = true;

                break;

            case "PI":

                strBase = (strOrigem.Trim() + "000000000").Substring(0, 9);

                intSoma = 0;

                for (intPos = 1; intPos <= 8; intPos++)
                {
                    intValor = int.Parse(strBase.Substring(intPos - 1, 1));

                    intValor = intValor * (10 - intPos);

                    intSoma = intSoma + intValor;
                }

                intResto = intSoma % 11;

                strDigito1 =
                    (intResto < 2 ? "0" : Convert.ToString(11 - intResto)).Substring(
                        (intResto < 2 ? "0" : Convert.ToString(11 - intResto)).Length - 1);

                strBase2 = strBase.Substring(0, 8) + strDigito1;

                if (strBase2 == strOrigem) retorno = true;

                break;

            case "PR":

                strBase = (strOrigem.Trim() + "0000000000").Substring(0, 10);

                intSoma = 0;

                intPeso = 2;

                for (intPos = 8; intPos <= 1; intPos = intPos + -1)
                {
                    intValor = int.Parse(strBase.Substring(intPos - 1, 1));

                    intValor = intValor * intPeso;

                    intSoma = intSoma + intValor;

                    intPeso = intPeso + 1;

                    if (intPeso > 7) intPeso = 2;
                }

                intResto = intSoma % 11;

                strDigito1 =
                    (intResto < 2 ? "0" : Convert.ToString(11 - intResto)).Substring(
                        (intResto < 2 ? "0" : Convert.ToString(11 - intResto)).Length - 1);

                strBase2 = strBase.Substring(0, 8) + strDigito1;

                intSoma = 0;

                intPeso = 2;

                for (intPos = 9; intPos <= 1; intPos = intPos + -1)
                {
                    intValor = int.Parse(strBase2.Substring(intPos - 1, 1));

                    intValor = intValor * intPeso;

                    intSoma = intSoma + intValor;

                    intPeso = intPeso + 1;

                    if (intPeso > 7) intPeso = 2;
                }

                intResto = intSoma % 11;

                strDigito2 =
                    (intResto < 2 ? "0" : Convert.ToString(11 - intResto)).Substring(
                        (intResto < 2 ? "0" : Convert.ToString(11 - intResto)).Length - 1);

                strBase2 = strBase2 + strDigito2;

                if (strBase2 == strOrigem) retorno = true;

                break;

            case "RJ":

                strBase = (strOrigem.Trim() + "00000000").Substring(0, 8);

                intSoma = 0;

                intPeso = 2;

                for (intPos = 7; intPos <= 1; intPos = intPos + -1)
                {
                    intValor = int.Parse(strBase.Substring(intPos - 1, 1));

                    intValor = intValor * intPeso;

                    intSoma = intSoma + intValor;

                    intPeso = intPeso + 1;

                    if (intPeso > 7) intPeso = 2;
                }

                intResto = intSoma % 11;

                strDigito1 =
                    (intResto < 2 ? "0" : Convert.ToString(11 - intResto)).Substring(
                        (intResto < 2 ? "0" : Convert.ToString(11 - intResto)).Length - 1);

                strBase2 = strBase.Substring(0, 7) + strDigito1;

                if (strBase2 == strOrigem) retorno = true;

                break;

            case "RN":

                strBase = (strOrigem.Trim() + "000000000").Substring(0, 9);

                if (strBase.Substring(0, 2) == "20")
                {
                    intSoma = 0;

                    for (intPos = 1; intPos <= 8; intPos++)
                    {
                        intValor = int.Parse(strBase.Substring(intPos - 1, 1));

                        intValor = intValor * (10 - intPos);

                        intSoma = intSoma + intValor;
                    }

                    intSoma = intSoma * 10;

                    intResto = intSoma % 11;

                    strDigito1 =
                        (intResto > 9 ? "0" : Convert.ToString(intResto)).Substring(
                            (intResto > 9 ? "0" : Convert.ToString(intResto)).Length - 1);

                    strBase2 = strBase.Substring(0, 8) + strDigito1;

                    if (strBase2 == strOrigem) retorno = true;
                }

                break;

            case "RO":

                strBase = (strOrigem.Trim() + "000000000").Substring(0, 9);

                strBase2 = strBase.Substring(3, 5);

                intSoma = 0;

                for (intPos = 1; intPos <= 5; intPos++)
                {
                    intValor = int.Parse(strBase2.Substring(intPos - 1, 1));

                    intValor = intValor * (7 - intPos);

                    intSoma = intSoma + intValor;
                }

                intResto = intSoma % 11;

                intValor = 11 - intResto;

                if (intValor > 9) intValor = intValor - 10;

                strDigito1 = Convert.ToString(intValor).Substring(Convert.ToString(intValor).Length - 1);

                strBase2 = strBase.Substring(0, 8) + strDigito1;

                if (strBase2 == strOrigem) retorno = true;

                break;

            case "RR":

                strBase = (strOrigem.Trim() + "000000000").Substring(0, 9);

                if (strBase.Substring(0, 2) == "24")
                {
                    intSoma = 0;

                    for (intPos = 1; intPos <= 8; intPos++)
                    {
                        intValor = int.Parse(strBase.Substring(intPos - 1, 1));

                        intValor = intValor * (10 - intPos);

                        intSoma = intSoma + intValor;
                    }

                    intResto = intSoma % 9;

                    strDigito1 = Convert.ToString(intResto).Substring(Convert.ToString(intResto).Length - 1);

                    strBase2 = strBase.Substring(0, 8) + strDigito1;

                    if (strBase2 == strOrigem) retorno = true;
                }

                break;

            case "RS":

                strBase = (strOrigem.Trim() + "0000000000").Substring(0, 10);

                intNumero = int.Parse(strBase.Substring(0, 3));

                if (intNumero > 0
                    && intNumero < 468)
                {
                    intSoma = 0;

                    intPeso = 2;

                    for (intPos = 9; intPos >= 1; intPos--)
                    {
                        intValor = int.Parse(strBase.Substring(intPos - 1, 1));
                        var temp = strBase.Substring(intPos - 1, 1);


                        intValor = intValor * intPeso;

                        intSoma = intSoma + intValor;

                        intPeso = intPeso + 1;

                        if (intPeso > 9) intPeso = 2;
                    }

                    intResto = intSoma % 11;

                    intValor = 11 - intResto;

                    if (intValor > 9) intValor = 0;

                    strDigito1 = Convert.ToString(intValor).Substring(Convert.ToString(intValor).Length - 1);

                    strBase2 = strBase.Substring(0, 9) + strDigito1;

                    if (strBase2 == strOrigem) retorno = true;
                }

                break;

            case "SC":

                strBase = (strOrigem.Trim() + "000000000").Substring(0, 9);

                intSoma = 0;

                for (intPos = 1; intPos <= 8; intPos++)
                {
                    intValor = int.Parse(strBase.Substring(intPos - 1, 1));

                    intValor = intValor * (10 - intPos);

                    intSoma = intSoma + intValor;
                }

                intResto = intSoma % 11;

                strDigito1 =
                    (intResto < 2 ? "0" : Convert.ToString(11 - intResto)).Substring(
                        (intResto < 2 ? "0" : Convert.ToString(11 - intResto)).Length - 1);

                strBase2 = strBase.Substring(0, 8) + strDigito1;

                if (strBase2 == strOrigem) retorno = true;

                break;

            case "SE":

                strBase = (strOrigem.Trim() + "000000000").Substring(0, 9);

                intSoma = 0;

                for (intPos = 1; intPos <= 8; intPos++)
                {
                    intValor = int.Parse(strBase.Substring(intPos - 1, 1));

                    intValor = intValor * (10 - intPos);

                    intSoma = intSoma + intValor;
                }

                intResto = intSoma % 11;

                intValor = 11 - intResto;

                if (intValor > 9) intValor = 0;

                strDigito1 = Convert.ToString(intValor).Substring(Convert.ToString(intValor).Length - 1);

                strBase2 = strBase.Substring(0, 8) + strDigito1;

                if (strBase2 == strOrigem) retorno = true;

                break;

            case "SP":

                if (strOrigem.Substring(0, 1) == "P")
                {
                    strBase = (strOrigem.Trim() + "0000000000000").Substring(0, 13);

                    strBase2 = strBase.Substring(1, 8);

                    intSoma = 0;

                    intPeso = 1;

                    for (intPos = 1; intPos <= 8; intPos++)
                    {
                        intValor = int.Parse(strBase.Substring(intPos - 1, 1));

                        intValor = intValor * intPeso;

                        intSoma = intSoma + intValor;

                        intPeso = intPeso + 1;

                        if (intPeso == 2) intPeso = 3;

                        if (intPeso == 9) intPeso = 10;
                    }

                    intResto = intSoma % 11;

                    strDigito1 = Convert.ToString(intResto).Substring(Convert.ToString(intResto).Length - 1);

                    strBase2 = strBase.Substring(0, 8)
                               + strDigito1 + strBase.Substring(10, 3);
                }

                else
                {
                    strBase = (strOrigem.Trim() + "000000000000").Substring(0, 12);

                    intSoma = 0;

                    intPeso = 1;

                    for (intPos = 1; intPos <= 8; intPos++)
                    {
                        intValor = int.Parse(strBase.Substring(intPos - 1, 1));

                        intValor = intValor * intPeso;

                        intSoma = intSoma + intValor;

                        intPeso = intPeso + 1;

                        if (intPeso == 2) intPeso = 3;

                        if (intPeso == 9) intPeso = 10;
                    }

                    intResto = intSoma % 11;

                    strDigito1 = Convert.ToString(intResto).Substring(Convert.ToString(intResto).Length - 1);

                    strBase2 = strBase.Substring(0, 8)
                               + strDigito1 + strBase.Substring(9, 2);

                    intSoma = 0;

                    intPeso = 2;

                    for (intPos = 11; intPos <= 1; intPos = intPos + -1)
                    {
                        intValor = int.Parse(strBase.Substring(intPos - 1, 1));

                        intValor = intValor * intPeso;

                        intSoma = intSoma + intValor;

                        intPeso = intPeso + 1;

                        if (intPeso > 10) intPeso = 2;
                    }

                    intResto = intSoma % 11;

                    strDigito2 = Convert.ToString(intResto).Substring(Convert.ToString(intResto).Length - 1);

                    strBase2 = strBase2 + strDigito2;
                }

                if (strBase2 == strOrigem) retorno = true;

                break;

            case "TO":

                strBase = (strOrigem.Trim() + "00000000000").Substring(0, 11);

                if ("01,02,03,99".IndexOf(strBase.Substring(2, 2), 0, StringComparison.OrdinalIgnoreCase) + 1
                    > 0)
                {
                    strBase2 = strBase.Substring(0, 2) + strBase.Substring(4, 6);

                    intSoma = 0;

                    for (intPos = 1; intPos <= 8; intPos++)
                    {
                        intValor = int.Parse(strBase2.Substring(intPos - 1, 1));

                        intValor = intValor * (10 - intPos);

                        intSoma = intSoma + intValor;
                    }

                    intResto = intSoma % 11;

                    strDigito1 =
                        (intResto < 2 ? "0" : Convert.ToString(11 - intResto)).Substring(
                            (intResto < 2 ? "0" : Convert.ToString(11 - intResto)).Length - 1);

                    strBase2 = strBase.Substring(0, 10) + strDigito1;

                    if (strBase2 == strOrigem) retorno = true;
                }

                break;
        }

        return retorno;
    }

    #endregion

    #region Credit Cards

    #region IsCreditCardAny

    /// <summary>
    ///     Determines whether the specified string is a valid credit, based on matching any one of the eight credit card
    ///     strings
    /// </summary>
    /// <param name="creditCard">The credit card.</param>
    /// <returns>
    ///     <c>true</c> if valid; otherwise, <c>false</c>.
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
    ///     Determines whether the specified string is an American Express, Discover, MasterCard, or Visa
    /// </summary>
    /// <param name="creditCard">The credit card.</param>
    /// <returns>
    ///     <c>true</c> if valid; otherwise, <c>false</c>.
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
    ///     Determines whether the specified string is an American Express card
    /// </summary>
    /// <param name="creditCard">The credit card.</param>
    /// <returns>
    ///     <c>true</c> if valid; otherwise, <c>false</c>.
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
    ///     Determines whether the specified string is an Carte Blanche card
    /// </summary>
    /// <param name="creditCard">The credit card.</param>
    /// <returns>
    ///     <c>true</c> if valid; otherwise, <c>false</c>.
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
    ///     Determines whether the specified string is an Diner's Club card
    /// </summary>
    /// <param name="creditCard">The credit card.</param>
    /// <returns>
    ///     <c>true</c> if valid; otherwise, <c>false</c>.
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
    ///     Determines whether the specified string is a Discover card
    /// </summary>
    /// <param name="creditCard">The credit card.</param>
    /// <returns>
    ///     <c>true</c> if valid; otherwise, <c>false</c>.
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
    ///     Determines whether the specified string is an En Route card
    /// </summary>
    /// <param name="creditCard">The credit card.</param>
    /// <returns>
    ///     <c>true</c> if valid; otherwise, <c>false</c>.
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
    ///     Determines whether the specified string is an JCB card
    /// </summary>
    /// <param name="creditCard">The credit card.</param>
    /// <returns>
    ///     <c>true</c> if valid; otherwise, <c>false</c>.
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
    ///     Determines whether the specified string is a Master Card credit card
    /// </summary>
    /// <param name="creditCard">The credit card.</param>
    /// <returns>
    ///     <c>true</c> if valid; otherwise, <c>false</c>.
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
    ///     Determines whether the specified string is Visa card.
    /// </summary>
    /// <param name="creditCard">The credit card.</param>
    /// <returns>
    ///     <c>true</c> if valid; otherwise, <c>false</c>.
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
    ///     Cleans the credit card number, returning just the numeric values.
    /// </summary>
    /// <param name="creditCard">The credit card.</param>
    /// <returns></returns>
    public static string CleanCreditCardNumber(this string creditCard)
    {
        var regex = new Regex(RegexPattern.CreditCardStripNonNumeric,
            RegexOptions.IgnoreCase | RegexOptions.Singleline);
        return regex.Replace(creditCard, string.Empty);
    }

    #endregion

    #region CreditPassesFormatCheck

    /// <summary>
    ///     Determines whether the credit card number, once cleaned, passes the Luhn algorith.
    ///     See: http://en.wikipedia.org/wiki/Luhn_algorithm
    /// </summary>
    /// <param name="creditCardNumber">The credit card number.</param>
    /// <returns></returns>
    private static bool CreditPassesFormatCheck(this string creditCardNumber)
    {
        creditCardNumber = CleanCreditCardNumber(creditCardNumber);
        if (creditCardNumber.IsInteger())
        {
            var numArray = new int[creditCardNumber.Length];
            for (var i = 0; i < numArray.Length; i++)
                numArray[i] = Convert.ToInt16(creditCardNumber[i].ToString(CultureInfo.InvariantCulture));

            return IsValidLuhn(numArray);
        }

        return false;
    }

    #endregion

    #region IsValidLuhn

    /// <summary>
    ///     Determines whether the specified int array passes the Luhn algorith
    /// </summary>
    /// <param name="digits">The int array to evaluate</param>
    /// <returns>
    ///     <c>true</c> if it validates; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsValidLuhn(this int[] digits)
    {
        var sum = 0;
        var alt = false;
        for (var i = digits.Length - 1; i >= 0; i--)
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
    ///     Determine whether the passed string is numeric, by attempting to parse it to a double
    /// </summary>
    /// <param name="str">The string to evaluated for numeric conversion</param>
    /// <returns>
    ///     <c>true</c> if the string can be converted to a number; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsStringNumeric(this string str)
    {
        return double.TryParse(str, NumberStyles.Float, NumberFormatInfo.CurrentInfo, out _);
    }

    #endregion

    #endregion
}