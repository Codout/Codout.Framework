using System;

namespace Codout.Framework.Common
{
    public class Validations
    {
        //Método que valida o CPF
        public static bool ValidaCPF(string vrCPF)
        {
            string valor = vrCPF.Replace(".", "");
            valor = valor.Replace("-", "");

            if (valor.Length != 11)
                return false;

            bool igual = true;
            for (int i = 1; i < 11 && igual; i++)
                if (valor[i] != valor[0])
                    igual = false;

            if (igual || valor == "12345678909")
                return false;

            int[] numeros = new int[11];
            for (int i = 0; i < 11; i++)
                numeros[i] = int.Parse(
                valor[i].ToString());

            int soma = 0;
            for (int i = 0; i < 9; i++)
                soma += (10 - i) * numeros[i];

            int resultado = soma % 11;
            if (resultado == 1 || resultado == 0)
            {
                if (numeros[9] != 0)
                    return false;
            }
            else if (numeros[9] != 11 - resultado)
                return false;

            soma = 0;
            for (int i = 0; i < 10; i++)
                soma += (11 - i) * numeros[i];

            resultado = soma % 11;

            if (resultado == 1 || resultado == 0)
            {
                if (numeros[10] != 0)
                    return false;

            }
            else
                if (numeros[10] != 11 - resultado)
                return false;
            return true;

        }

        //Método que valida o CNPJ 
        public static bool ValidaCNPJ(string vrCNPJ)
        {

            string CNPJ = vrCNPJ.Replace(".", "");
            CNPJ = CNPJ.Replace("/", "");
            CNPJ = CNPJ.Replace("-", "");

            int[] digitos, soma, resultado;
            int nrDig;
            string ftmt;
            bool[] CNPJOk;

            ftmt = "6543298765432";
            digitos = new int[14];
            soma = new int[2];
            soma[0] = 0;
            soma[1] = 0;
            resultado = new int[2];
            resultado[0] = 0;
            resultado[1] = 0;
            CNPJOk = new bool[2];
            CNPJOk[0] = false;
            CNPJOk[1] = false;

            try
            {
                for (nrDig = 0; nrDig < 14; nrDig++)
                {
                    digitos[nrDig] = int.Parse(
                     CNPJ.Substring(nrDig, 1));
                    if (nrDig <= 11)
                        soma[0] += (digitos[nrDig] *
                        int.Parse(ftmt.Substring(
                          nrDig + 1, 1)));
                    if (nrDig <= 12)
                        soma[1] += (digitos[nrDig] *
                        int.Parse(ftmt.Substring(
                          nrDig, 1)));
                }

                for (nrDig = 0; nrDig < 2; nrDig++)
                {
                    resultado[nrDig] = (soma[nrDig] % 11);
                    if ((resultado[nrDig] == 0) || (resultado[nrDig] == 1))
                        CNPJOk[nrDig] = (
                        digitos[12 + nrDig] == 0);

                    else
                        CNPJOk[nrDig] = (
                        digitos[12 + nrDig] == (
                        11 - resultado[nrDig]));

                }

                return (CNPJOk[0] && CNPJOk[1]);

            }
            catch
            {
                return false;
            }

        }

        //Método que valida o Cep
        public static bool ValidaCep(string cep)
        {
            if (cep.Length == 8)
            {
                cep = cep.Substring(0, 5) + "-" + cep.Substring(5, 3);
                //txt.Text = cep;
            }
            return System.Text.RegularExpressions.Regex.IsMatch(cep, ("[0-9]{5}-[0-9]{3}"));
        }

        //Método que valida o Email
        public static bool ValidaEmail(string email)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(email, ("(?<user>[^@]+)@(?<host>.+)"));
        }

        public static bool ValidaInscricaoEstadual(string pUF, string pInscr)
        {

            bool retorno = false;

            string strBase;

            string strBase2;

            string strOrigem;

            string strDigito1;

            string strDigito2;

            int intPos;

            int intValor;

            int intSoma = 0;

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

            if ((pInscr.Trim().ToUpper() == "ISENTO"))
            {

                return true;

            }

            for (intPos = 1; intPos <= pInscr.Trim().Length; intPos++)
            {

                if ((("0123456789P".IndexOf(pInscr.Substring((intPos - 1), 1), 0, System.StringComparison.OrdinalIgnoreCase) + 1)

                > 0))
                {

                    strOrigem = (strOrigem + pInscr.Substring((intPos - 1), 1));

                }

            }


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
                        intValor = int.Parse(strBase.Substring((intPos - 1), 1));

                        intValor = (intValor * intPeso);

                        intSoma = (intSoma + intValor);

                        intPeso = (intPeso + 1);

                        if ((intPeso > 9))
                        {

                            intPeso = 2;

                        }

                    }

                    intResto = (intSoma % 11);

                    if ((11 - intResto) == 10 || (11 - intResto) == 11)
                    {
                        strDigito1 = "0";
                    }
                    else
                    {
                        strDigito1 = (11 - intResto).ToString();
                    }


                    strBase2 = strBase + strDigito1;

                    intSoma = 0;

                    intPeso = 2;

                    for (intPos = strBase2.Length; intPos >= 1; intPos--)
                    {
                        intValor = int.Parse(strBase2.Substring((intPos - 1), 1));

                        intValor = (intValor * intPeso);

                        intSoma = (intSoma + intValor);

                        intPeso = (intPeso + 1);

                        if ((intPeso > 9))
                        {

                            intPeso = 2;

                        }

                    }

                    intResto = (intSoma % 11);

                    if ((11 - intResto) == 10 || (11 - intResto) == 11)
                    {
                        strDigito2 = "0";
                    }
                    else
                    {
                        strDigito2 = (11 - intResto).ToString();
                    }

                    strBase2 = strBase2 + strDigito2;


                    if ((strBase2 == strOrigem))
                    {

                        retorno = true;

                    }



                    break;

                case "AL": // OK - Validado

                    strBase = (strOrigem.Trim() + "000000000").Substring(0, 8);

                    if ((strBase.Substring(0, 2) == "24") && (strBase.Substring(2, 1) == "0" || strBase.Substring(2, 1) == "3" || strBase.Substring(2, 1) == "5" || strBase.Substring(2, 1) == "7" || strBase.Substring(2, 1) == "8"))
                    {

                        intSoma = 0;
                        intPeso = 2;

                        for (intPos = strBase.Length; intPos >= 1; intPos--)
                        {

                            intValor = int.Parse(strBase.Substring((intPos - 1), 1));

                            intValor = (intValor * intPeso);

                            intSoma = (intSoma + intValor);

                            intPeso = (intPeso + 1);

                            if ((intPeso > 9))
                            {

                                intPeso = 2;

                            }

                        }

                        intSoma = (intSoma * 10);

                        intResto = (intSoma % 11);

                        if (intResto == 10)
                        {
                            strDigito1 = "0";
                        }
                        else
                        {
                            strDigito1 = intResto.ToString();
                        }

                        strBase2 = (strBase.Substring(0, 8) + strDigito1);

                        if ((strBase2 == strOrigem))
                        {

                            retorno = true;

                        }

                    }

                    break;

                case "AM": // OK - Validado

                    strBase = (strOrigem.Trim() + "000000000").Substring(0, 8);

                    intSoma = 0;
                    intPeso = 2;

                    for (intPos = strBase.Length; intPos >= 1; intPos--)
                    {

                        intValor = int.Parse(strBase.Substring((intPos - 1), 1));

                        intValor = (intValor * intPeso);

                        intSoma = (intSoma + intValor);

                        intPeso = (intPeso + 1);

                        if ((intPeso > 9))
                        {

                            intPeso = 2;

                        }

                    }

                    if ((intSoma < 11))
                    {

                        strDigito1 = (11 - intSoma).ToString();
                    }

                    else
                    {

                        intResto = (intSoma % 11);

                        if (intResto <= 1)
                        {
                            strDigito1 = "0";
                        }
                        else
                        {
                            strDigito1 = (11 - intResto).ToString();
                        }

                    }

                    strBase2 = (strBase.Substring(0, 8) + strDigito1);

                    if ((strBase2 == strOrigem))
                    {

                        retorno = true;

                    }

                    break;

                case "AP": // OK - Validado

                    strBase = (strOrigem.Trim() + "000000000").Substring(0, 8);

                    intPeso = 0;

                    intDig = 0;

                    if ((strBase.Substring(0, 2) == "03"))
                    {

                        intNumero = int.Parse(strBase.Substring(0, 8));

                        if (((intNumero >= 3000001)

                        && (intNumero <= 3017000)))
                        {

                            intPeso = 5;

                            intDig = 0;

                        }

                        else if (((intNumero >= 3017001)

                        && (intNumero <= 3019022)))
                        {

                            intPeso = 9;

                            intDig = 1;

                        }

                        else if ((intNumero >= 3019023))
                        {

                            intPeso = 0;

                            intDig = 0;

                        }

                        intSoma = intPeso;
                        intPeso = 2;

                        for (intPos = strBase.Length; intPos >= 1; intPos--)
                        {

                            intValor = int.Parse(strBase.Substring((intPos - 1), 1));

                            intValor = (intValor * intPeso);

                            intSoma = (intSoma + intValor);

                            intPeso = (intPeso + 1);

                        }

                        intResto = (intSoma % 11);

                        intValor = (11 - intResto);

                        if ((intValor == 10))
                        {

                            intValor = 0;

                        }

                        else if ((intValor == 11))
                        {

                            intValor = intDig;

                        }

                        strDigito1 = intValor.ToString();

                        strBase2 = strBase + strDigito1;

                        if ((strBase2 == strOrigem))
                        {

                            retorno = true;

                        }

                    }

                    break;

                case "BA": // OK - Validado

                    if (strOrigem.Length == 8)
                    {
                        strBase = (strOrigem.Trim() + "00000000").Substring(0, 8);

                        if ((("0123458".IndexOf(strBase.Substring(0, 1), 0, System.StringComparison.OrdinalIgnoreCase) + 1) > 0))
                        {

                            intSoma = 0;

                            for (intPos = 1; (intPos <= 6); intPos++)
                            {

                                intValor = int.Parse(strBase.Substring((intPos - 1), 1));

                                intValor = (intValor * (8 - intPos));

                                intSoma = (intSoma + intValor);

                            }
                        }

                        intResto = (intSoma % 10);

                        strDigito2 = ((intResto == 0) ? "0" : Convert.ToString((10 - intResto))).Substring((((intResto == 0) ? "0" : Convert.ToString((10 - intResto))).Length - 1));

                        strBase2 = (strBase.Substring(0, 6) + strDigito2);

                        intSoma = 0;

                        for (intPos = 1; (intPos <= 7); intPos++)
                        {

                            intValor = int.Parse(strBase2.Substring((intPos - 1), 1));

                            intValor = (intValor * (9 - intPos));

                            intSoma = (intSoma + intValor);

                        }

                        intResto = (intSoma % 10);

                        strDigito1 = ((intResto == 0) ? "0" : Convert.ToString((10 - intResto))).Substring((((intResto == 0) ? "0" : Convert.ToString((10 - intResto))).Length - 1));

                    }

                    else
                    {

                        intSoma = 0;

                        for (intPos = 1; (intPos <= 6); intPos++)
                        {

                            intValor = int.Parse(strBase.Substring((intPos - 1), 1));

                            intValor = (intValor * (8 - intPos));

                            intSoma = (intSoma + intValor);

                        }

                        intResto = (intSoma % 11);

                        strDigito2 = ((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Substring((((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Length - 1));

                        strBase2 = (strBase.Substring(0, 6) + strDigito2);

                        intSoma = 0;

                        for (intPos = 1; (intPos <= 7); intPos++)
                        {

                            intValor = int.Parse(strBase2.Substring((intPos - 1), 1));

                            intValor = (intValor * (9 - intPos));

                            intSoma = (intSoma + intValor);

                        }

                        intResto = (intSoma % 11);

                        strDigito1 = ((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Substring((((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Length - 1));

                    }

                    strBase2 = (strBase.Substring(0, 6)

                    + (strDigito1 + strDigito2));



                    if ((strBase2 == strOrigem))
                    {

                        retorno = true;

                    }

                    break;

                case "CE": // OK - Validado

                    strBase = (strOrigem.Trim() + "000000000").Substring(0, 9);

                    intSoma = 0;

                    for (intPos = 1; (intPos <= 8); intPos++)
                    {

                        intValor = int.Parse(strBase.Substring((intPos - 1), 1));

                        intValor = (intValor * (10 - intPos));

                        intSoma = (intSoma + intValor);

                    }

                    intResto = (intSoma % 11);

                    intValor = (11 - intResto);

                    if ((intValor > 9))
                    {

                        intValor = 0;

                    }

                    strDigito1 = Convert.ToString(intValor).Substring((Convert.ToString(intValor).Length - 1));

                    strBase2 = (strBase.Substring(0, 8) + strDigito1);

                    if ((strBase2 == strOrigem))
                    {

                        retorno = true;

                    }

                    break;

                case "DF": // OK - Validado

                    strBase = Convert.ToInt64(strOrigem.Trim()).ToString("0000000000000").Substring(0, 11);

                    if ((strBase.Substring(0, 3) == "073"))
                    {

                        intSoma = 0;
                        intPeso = 2;

                        for (intPos = strBase.Length; intPos >= 1; intPos--)
                        {

                            intValor = int.Parse(strBase.Substring((intPos - 1), 1));

                            intValor = (intValor * intPeso);

                            intSoma = (intSoma + intValor);

                            intPeso = (intPeso + 1);

                            if ((intPeso > 9))
                            {

                                intPeso = 2;

                            }

                        }

                        intResto = (intSoma % 11);

                        strDigito1 = ((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Substring((((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Length - 1));

                        strBase2 = (strBase.Substring(0, 11) + strDigito1);

                        intSoma = 0;
                        intPeso = 2;

                        for (intPos = strBase2.Length; intPos >= 1; intPos--)
                        {

                            intValor = int.Parse(strBase2.Substring((intPos - 1), 1));

                            intValor = (intValor * intPeso);

                            intSoma = (intSoma + intValor);

                            intPeso = (intPeso + 1);

                            if ((intPeso > 9))
                            {

                                intPeso = 2;

                            }

                        }




                        intResto = (intSoma % 11);

                        strDigito2 = ((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Substring((((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Length - 1));

                        strBase2 = (strBase2 + strDigito2);

                        if ((strBase2 == strOrigem))
                        {

                            retorno = true;

                        }

                    }

                    break;

                case "ES": // OK - Validado

                    strBase = Convert.ToInt64(strOrigem).ToString("000000000").Substring(0, 8);

                    intSoma = 0;

                    for (intPos = 1; (intPos <= 8); intPos++)
                    {

                        intValor = int.Parse(strBase.Substring((intPos - 1), 1));

                        intValor = (intValor * (10 - intPos));

                        intSoma = (intSoma + intValor);

                    }

                    intResto = (intSoma % 11);

                    strDigito1 = ((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Substring((((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Length - 1));

                    strBase2 = (strBase.Substring(0, 8) + strDigito1);

                    if ((strBase2 == strOrigem))
                    {

                        retorno = true;

                    }

                    break;

                case "GO": // OK - Validado

                    strBase = (strOrigem.Trim() + "000000000").Substring(0, 8);

                    if ((("10,11,15".IndexOf(strBase.Substring(0, 2), 0, System.StringComparison.OrdinalIgnoreCase) + 1)

                    > 0))
                    {

                        intSoma = 0;

                        for (intPos = 1; (intPos <= 8); intPos++)
                        {

                            intValor = int.Parse(strBase.Substring((intPos - 1), 1));

                            intValor = (intValor * (10 - intPos));

                            intSoma = (intSoma + intValor);

                        }

                        intResto = (intSoma % 11);

                        if ((intResto == 0))
                        {

                            strDigito1 = "0";

                        }

                        else if ((intResto == 1))
                        {

                            intNumero = int.Parse(strBase.Substring(0, 8));

                            strDigito1 = (((intNumero >= 10103105)

                            && (intNumero <= 10119997)) ? "1" : "0").Substring(((((intNumero >= 10103105)

                            && (intNumero <= 10119997)) ? "1" : "0").Length - 1));

                        }

                        else
                        {

                            strDigito1 = Convert.ToString((11 - intResto)).Substring((Convert.ToString((11 - intResto)).Length - 1));

                        }

                        strBase2 = (strBase.Substring(0, 8) + strDigito1);

                        if ((strBase2 == strOrigem))
                        {

                            retorno = true;

                        }

                    }

                    break;

                case "MA": // OK - Validado

                    strBase = (strOrigem.Trim() + "000000000").Substring(0, 8);

                    if ((strBase.Substring(0, 2) == "12"))
                    {

                        intSoma = 0;

                        for (intPos = 1; (intPos <= 8); intPos++)
                        {

                            intValor = int.Parse(strBase.Substring((intPos - 1), 1));

                            intValor = (intValor * (10 - intPos));

                            intSoma = (intSoma + intValor);

                        }

                        intResto = (intSoma % 11);

                        strDigito1 = ((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Substring((((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Length - 1));

                        strBase2 = (strBase.Substring(0, 8) + strDigito1);

                        if ((strBase2 == strOrigem))
                        {

                            retorno = true;

                        }

                    }

                    break;

                case "MT": // OK - Validado

                    strBase = (strOrigem.Trim() + "0000000000").Substring(0, 10);

                    intSoma = 0;
                    intPeso = 2;

                    for (intPos = strBase.Length; intPos >= 1; intPos--)
                    {

                        intValor = int.Parse(strBase.Substring((intPos - 1), 1));

                        intValor = (intValor * intPeso);

                        intSoma = (intSoma + intValor);

                        intPeso = (intPeso + 1);

                        if ((intPeso > 9))
                        {

                            intPeso = 2;

                        }

                    }

                    intResto = (intSoma % 11);

                    strDigito1 = ((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Substring((((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Length - 1));

                    strBase2 = (strBase.Substring(0, 10) + strDigito1);

                    if ((strBase2 == strOrigem))
                    {

                        retorno = true;

                    }

                    break;

                case "MS": // OK - Validado

                    strBase = (strOrigem.Trim() + "000000000").Substring(0, 8);

                    if ((strBase.Substring(0, 2) == "28"))
                    {

                        intSoma = 0;

                        for (intPos = 1; (intPos <= 8); intPos++)
                        {

                            intValor = int.Parse(strBase.Substring((intPos - 1), 1));

                            intValor = (intValor * (10 - intPos));

                            intSoma = (intSoma + intValor);

                        }

                        intResto = (intSoma % 11);

                        strDigito1 = ((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Substring((((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Length - 1));

                        strBase2 = (strBase.Substring(0, 8) + strDigito1);

                        if ((strBase2 == strOrigem))
                        {

                            retorno = true;

                        }

                    }

                    break;

                case "MG":

                    strBase = (strOrigem.Trim() + "0000000000000").Substring(0, 11);

                    strBase2 = (strBase.Substring(0, 3) + ("0" + strBase.Substring(3, 8)));

                    intNumero = 2;
                    intSoma = 0;
                    //int intDigito = 0;

                    for (intPos = 1; (intPos <= 12); intPos++)
                    {

                        intValor = int.Parse(strBase2.Substring((intPos - 1), 1));

                        intNumero = ((intNumero == 2) ? 1 : 2);

                        intValor = (intValor * intNumero);

                        intSoma = intSoma + intNumero;

                        if ((intValor > 9))
                        {

                            strDigito1 = intValor.ToString("00");

                            intValor = int.Parse(strDigito1.Substring(0, 1)) + int.Parse(strDigito1.Substring(1, 1));

                        }

                        intSoma = (intSoma + intValor);

                    }

                    //intValor = intSoma;

                    //int TamTotal = 3;
                    //int SomaResultado = 0;

                    //while (intValor.ToString("000").Substring(TamTotal-1,1)!="0")
                    //{
                    //    //intValor2 = intValor2 + 1;
                    //    SomaResultado = SomaResultado + Convert.ToInt32(intValor.ToString("000").Substring(TamTotal - 1, 1));
                    //    TamTotal--;
                    //}



                    //while ((string.Format("000", intValor).Substring((string.Format("000", intValor).Length - 1)) != "0"))
                    //{

                    //    intValor = (intValor + 1);

                    //    strDigito1 = string.Format("00", (intValor - intSoma)).Substring((string.Format("00", (intValor - intSoma)).Length - 1));

                    //    strBase2 = (strBase.Substring(0, 11) + strDigito1);

                    //    intSoma = 0;

                    //    intPeso = 2;

                    //    for (intPos = 12; (intPos <= 1); intPos = (intPos + -1))
                    //    {

                    //        intValor = int.Parse(strBase2.Substring((intPos - 1), 1));

                    //        intValor = (intValor * intPeso);

                    //        intSoma = (intSoma + intValor);

                    //        intPeso = (intPeso + 1);

                    //        if ((intPeso > 11))
                    //        {

                    //            intPeso = 2;

                    //        }

                    //    }

                    //    intResto = (intSoma % 11);

                    //    strDigito2 = ((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Substring((((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Length - 1));

                    //    strBase2 = (strBase2 + strDigito2);

                    //    if ((strBase2 == strOrigem))
                    //    {

                    //        retorno = true;

                    //    }

                    //}

                    break;

                case "PA":

                    strBase = (strOrigem.Trim() + "000000000").Substring(0, 9);

                    if ((strBase.Substring(0, 2) == "15"))
                    {

                        intSoma = 0;

                        for (intPos = 1; (intPos <= 8); intPos++)
                        {

                            intValor = int.Parse(strBase.Substring((intPos - 1), 1));

                            intValor = (intValor * (10 - intPos));

                            intSoma = (intSoma + intValor);

                        }

                        intResto = (intSoma % 11);

                        strDigito1 = ((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Substring((((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Length - 1));

                        strBase2 = (strBase.Substring(0, 8) + strDigito1);

                        if ((strBase2 == strOrigem))
                        {

                            retorno = true;

                        }

                    }

                    break;

                case "PB":

                    strBase = (strOrigem.Trim() + "000000000").Substring(0, 9);

                    intSoma = 0;

                    for (intPos = 1; (intPos <= 8); intPos++)
                    {

                        intValor = int.Parse(strBase.Substring((intPos - 1), 1));

                        intValor = (intValor * (10 - intPos));

                        intSoma = (intSoma + intValor);

                    }

                    intResto = (intSoma % 11);

                    intValor = (11 - intResto);

                    if ((intValor > 9))
                    {

                        intValor = 0;

                    }

                    strDigito1 = Convert.ToString(intValor).Substring((Convert.ToString(intValor).Length - 1));

                    strBase2 = (strBase.Substring(0, 8) + strDigito1);

                    if ((strBase2 == strOrigem))
                    {

                        retorno = true;

                    }

                    break;

                case "PE":

                    strBase = (strOrigem.Trim() + "00000000000000").Substring(0, 14);

                    intSoma = 0;

                    intPeso = 2;

                    for (intPos = 13; (intPos <= 1); intPos = (intPos + -1))
                    {

                        intValor = int.Parse(strBase.Substring((intPos - 1), 1));

                        intValor = (intValor * intPeso);

                        intSoma = (intSoma + intValor);

                        intPeso = (intPeso + 1);

                        if ((intPeso > 9))
                        {

                            intPeso = 2;

                        }

                    }

                    intResto = (intSoma % 11);

                    intValor = (11 - intResto);

                    if ((intValor > 9))
                    {

                        intValor = (intValor - 10);

                    }

                    strDigito1 = Convert.ToString(intValor).Substring((Convert.ToString(intValor).Length - 1));

                    strBase2 = (strBase.Substring(0, 13) + strDigito1);

                    if ((strBase2 == strOrigem))
                    {

                        retorno = true;

                    }

                    break;

                case "PI":

                    strBase = (strOrigem.Trim() + "000000000").Substring(0, 9);

                    intSoma = 0;

                    for (intPos = 1; (intPos <= 8); intPos++)
                    {

                        intValor = int.Parse(strBase.Substring((intPos - 1), 1));

                        intValor = (intValor * (10 - intPos));

                        intSoma = (intSoma + intValor);

                    }

                    intResto = (intSoma % 11);

                    strDigito1 = ((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Substring((((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Length - 1));

                    strBase2 = (strBase.Substring(0, 8) + strDigito1);

                    if ((strBase2 == strOrigem))
                    {

                        retorno = true;

                    }

                    break;

                case "PR":

                    strBase = (strOrigem.Trim() + "0000000000").Substring(0, 10);

                    intSoma = 0;

                    intPeso = 2;

                    for (intPos = 8; (intPos <= 1); intPos = (intPos + -1))
                    {

                        intValor = int.Parse(strBase.Substring((intPos - 1), 1));

                        intValor = (intValor * intPeso);

                        intSoma = (intSoma + intValor);

                        intPeso = (intPeso + 1);

                        if ((intPeso > 7))
                        {

                            intPeso = 2;

                        }

                    }

                    intResto = (intSoma % 11);

                    strDigito1 = ((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Substring((((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Length - 1));

                    strBase2 = (strBase.Substring(0, 8) + strDigito1);

                    intSoma = 0;

                    intPeso = 2;

                    for (intPos = 9; (intPos <= 1); intPos = (intPos + -1))
                    {

                        intValor = int.Parse(strBase2.Substring((intPos - 1), 1));

                        intValor = (intValor * intPeso);

                        intSoma = (intSoma + intValor);

                        intPeso = (intPeso + 1);

                        if ((intPeso > 7))
                        {

                            intPeso = 2;

                        }

                    }

                    intResto = (intSoma % 11);

                    strDigito2 = ((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Substring((((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Length - 1));

                    strBase2 = (strBase2 + strDigito2);

                    if ((strBase2 == strOrigem))
                    {

                        retorno = true;

                    }

                    break;

                case "RJ":

                    strBase = (strOrigem.Trim() + "00000000").Substring(0, 8);

                    intSoma = 0;

                    intPeso = 2;

                    for (intPos = 7; (intPos <= 1); intPos = (intPos + -1))
                    {

                        intValor = int.Parse(strBase.Substring((intPos - 1), 1));

                        intValor = (intValor * intPeso);

                        intSoma = (intSoma + intValor);

                        intPeso = (intPeso + 1);

                        if ((intPeso > 7))
                        {

                            intPeso = 2;

                        }

                    }

                    intResto = (intSoma % 11);

                    strDigito1 = ((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Substring((((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Length - 1));

                    strBase2 = (strBase.Substring(0, 7) + strDigito1);

                    if ((strBase2 == strOrigem))
                    {

                        retorno = true;

                    }

                    break;

                case "RN":

                    strBase = (strOrigem.Trim() + "000000000").Substring(0, 9);

                    if ((strBase.Substring(0, 2) == "20"))
                    {

                        intSoma = 0;

                        for (intPos = 1; (intPos <= 8); intPos++)
                        {

                            intValor = int.Parse(strBase.Substring((intPos - 1), 1));

                            intValor = (intValor * (10 - intPos));

                            intSoma = (intSoma + intValor);

                        }

                        intSoma = (intSoma * 10);

                        intResto = (intSoma % 11);

                        strDigito1 = ((intResto > 9) ? "0" : Convert.ToString(intResto)).Substring((((intResto > 9) ? "0" : Convert.ToString(intResto)).Length - 1));

                        strBase2 = (strBase.Substring(0, 8) + strDigito1);

                        if ((strBase2 == strOrigem))
                        {

                            retorno = true;

                        }

                    }

                    break;

                case "RO":

                    strBase = (strOrigem.Trim() + "000000000").Substring(0, 9);

                    strBase2 = strBase.Substring(3, 5);

                    intSoma = 0;

                    for (intPos = 1; (intPos <= 5); intPos++)
                    {

                        intValor = int.Parse(strBase2.Substring((intPos - 1), 1));

                        intValor = (intValor * (7 - intPos));

                        intSoma = (intSoma + intValor);

                    }

                    intResto = (intSoma % 11);

                    intValor = (11 - intResto);

                    if ((intValor > 9))
                    {

                        intValor = (intValor - 10);

                    }

                    strDigito1 = Convert.ToString(intValor).Substring((Convert.ToString(intValor).Length - 1));

                    strBase2 = (strBase.Substring(0, 8) + strDigito1);

                    if ((strBase2 == strOrigem))
                    {

                        retorno = true;

                    }

                    break;

                case "RR":

                    strBase = (strOrigem.Trim() + "000000000").Substring(0, 9);

                    if ((strBase.Substring(0, 2) == "24"))
                    {

                        intSoma = 0;

                        for (intPos = 1; (intPos <= 8); intPos++)
                        {

                            intValor = int.Parse(strBase.Substring((intPos - 1), 1));

                            intValor = (intValor * (10 - intPos));

                            intSoma = (intSoma + intValor);

                        }

                        intResto = (intSoma % 9);

                        strDigito1 = Convert.ToString(intResto).Substring((Convert.ToString(intResto).Length - 1));

                        strBase2 = (strBase.Substring(0, 8) + strDigito1);

                        if ((strBase2 == strOrigem))
                        {

                            retorno = true;

                        }

                    }

                    break;

                case "RS":

                    strBase = (strOrigem.Trim() + "0000000000").Substring(0, 10);

                    intNumero = int.Parse(strBase.Substring(0, 3));

                    if (((intNumero > 0)

                    && (intNumero < 468)))
                    {

                        intSoma = 0;

                        intPeso = 2;

                        for (intPos = 9; intPos >= 1; intPos--)
                        {

                            intValor = int.Parse(strBase.Substring((intPos - 1), 1));
                            var temp = strBase.Substring((intPos - 1), 1);


                            intValor = (intValor * intPeso);

                            intSoma = (intSoma + intValor);

                            intPeso = (intPeso + 1);

                            if ((intPeso > 9))
                            {

                                intPeso = 2;

                            }

                        }

                        intResto = (intSoma % 11);

                        intValor = (11 - intResto);

                        if ((intValor > 9))
                        {

                            intValor = 0;

                        }

                        strDigito1 = Convert.ToString(intValor).Substring((Convert.ToString(intValor).Length - 1));

                        strBase2 = (strBase.Substring(0, 9) + strDigito1);

                        if ((strBase2 == strOrigem))
                        {

                            retorno = true;

                        }

                    }

                    break;

                case "SC":

                    strBase = (strOrigem.Trim() + "000000000").Substring(0, 9);

                    intSoma = 0;

                    for (intPos = 1; (intPos <= 8); intPos++)
                    {

                        intValor = int.Parse(strBase.Substring((intPos - 1), 1));

                        intValor = (intValor * (10 - intPos));

                        intSoma = (intSoma + intValor);

                    }

                    intResto = (intSoma % 11);

                    strDigito1 = ((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Substring((((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Length - 1));

                    strBase2 = (strBase.Substring(0, 8) + strDigito1);

                    if ((strBase2 == strOrigem))
                    {

                        retorno = true;

                    }

                    break;

                case "SE":

                    strBase = (strOrigem.Trim() + "000000000").Substring(0, 9);

                    intSoma = 0;

                    for (intPos = 1; (intPos <= 8); intPos++)
                    {

                        intValor = int.Parse(strBase.Substring((intPos - 1), 1));

                        intValor = (intValor * (10 - intPos));

                        intSoma = (intSoma + intValor);

                    }

                    intResto = (intSoma % 11);

                    intValor = (11 - intResto);

                    if ((intValor > 9))
                    {

                        intValor = 0;

                    }

                    strDigito1 = Convert.ToString(intValor).Substring((Convert.ToString(intValor).Length - 1));

                    strBase2 = (strBase.Substring(0, 8) + strDigito1);

                    if ((strBase2 == strOrigem))
                    {

                        retorno = true;

                    }

                    break;

                case "SP":

                    if ((strOrigem.Substring(0, 1) == "P"))
                    {

                        strBase = (strOrigem.Trim() + "0000000000000").Substring(0, 13);

                        strBase2 = strBase.Substring(1, 8);

                        intSoma = 0;

                        intPeso = 1;

                        for (intPos = 1; (intPos <= 8); intPos++)
                        {

                            intValor = int.Parse(strBase.Substring((intPos - 1), 1));

                            intValor = (intValor * intPeso);

                            intSoma = (intSoma + intValor);

                            intPeso = (intPeso + 1);

                            if ((intPeso == 2))
                            {

                                intPeso = 3;

                            }

                            if ((intPeso == 9))
                            {

                                intPeso = 10;

                            }

                        }

                        intResto = (intSoma % 11);

                        strDigito1 = Convert.ToString(intResto).Substring((Convert.ToString(intResto).Length - 1));

                        strBase2 = (strBase.Substring(0, 8)

                        + (strDigito1 + strBase.Substring(10, 3)));

                    }

                    else
                    {

                        strBase = (strOrigem.Trim() + "000000000000").Substring(0, 12);

                        intSoma = 0;

                        intPeso = 1;

                        for (intPos = 1; (intPos <= 8); intPos++)
                        {

                            intValor = int.Parse(strBase.Substring((intPos - 1), 1));

                            intValor = (intValor * intPeso);

                            intSoma = (intSoma + intValor);

                            intPeso = (intPeso + 1);

                            if ((intPeso == 2))
                            {

                                intPeso = 3;

                            }

                            if ((intPeso == 9))
                            {

                                intPeso = 10;

                            }

                        }

                        intResto = (intSoma % 11);

                        strDigito1 = Convert.ToString(intResto).Substring((Convert.ToString(intResto).Length - 1));

                        strBase2 = (strBase.Substring(0, 8)

                        + (strDigito1 + strBase.Substring(9, 2)));

                        intSoma = 0;

                        intPeso = 2;

                        for (intPos = 11; (intPos <= 1); intPos = (intPos + -1))
                        {

                            intValor = int.Parse(strBase.Substring((intPos - 1), 1));

                            intValor = (intValor * intPeso);

                            intSoma = (intSoma + intValor);

                            intPeso = (intPeso + 1);

                            if ((intPeso > 10))
                            {

                                intPeso = 2;

                            }

                        }

                        intResto = (intSoma % 11);

                        strDigito2 = Convert.ToString(intResto).Substring((Convert.ToString(intResto).Length - 1));

                        strBase2 = (strBase2 + strDigito2);

                    }

                    if ((strBase2 == strOrigem))
                    {

                        retorno = true;

                    }

                    break;

                case "TO":

                    strBase = (strOrigem.Trim() + "00000000000").Substring(0, 11);

                    if ((("01,02,03,99".IndexOf(strBase.Substring(2, 2), 0, System.StringComparison.OrdinalIgnoreCase) + 1)

                    > 0))
                    {

                        strBase2 = (strBase.Substring(0, 2) + strBase.Substring(4, 6));

                        intSoma = 0;

                        for (intPos = 1; (intPos <= 8); intPos++)
                        {

                            intValor = int.Parse(strBase2.Substring((intPos - 1), 1));

                            intValor = (intValor * (10 - intPos));

                            intSoma = (intSoma + intValor);

                        }

                        intResto = (intSoma % 11);

                        strDigito1 = ((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Substring((((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Length - 1));

                        strBase2 = (strBase.Substring(0, 10) + strDigito1);

                        if ((strBase2 == strOrigem))
                        {

                            retorno = true;

                        }

                    }

                    break;

            }

            return retorno;

        }


    }
}
