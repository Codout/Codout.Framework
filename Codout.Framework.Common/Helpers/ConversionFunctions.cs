using System;

namespace Codout.Framework.Common.Helpers
{
    public class ConversionFunctions
    {

        /// <summary>
        /// Converte um valor double em uma string, formatando com 2 casas decimais por padrão
        /// </summary>
        /// <param name="valor">valor a ser convertido</param>
        /// <param name="emReais">indica se deve incluir o símbolo da moeda R$</param>
        /// <returns>String formatada</returns>
        public static string ConverteDoubleToString(double valor, bool emReais)
        {
            return ConverteDoubleToString(valor, emReais, 2);
        }

        /// <summary>
        /// Converte um valor double em uma string, formantando com casas decimais
        /// </summary>
        /// <param name="valor">Valor a ser convertido</param>
        /// <param name="emReais">indica se deve incluir o símbolo da moeda R$</param>
        /// <param name="casasDecimais">indica a quantidade de casas decimais</param>
        /// <returns>String formatada</returns>
        public static string ConverteDoubleToString(double valor, bool emReais, int casasDecimais)
        {
            try
            {
                string aux = "".PadRight(casasDecimais, '0');
                if (emReais)
                    return valor.ToString("R$ ###,###,##0." + aux);
                else
                    return valor.ToString("###,###,##0." + aux);
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// Converto um string para um inteiro, retornando zero, caso tenha erro na conversão.
        /// </summary>
        /// <param name="valor">dado a ser convertido</param>
        /// <returns>dado convertido</returns>
        public static int ConverteStringToInt(string valor)
        {
            int retorno;
            try
            {
                retorno = Convert.ToInt32(valor);
            }
            catch
            {
                retorno = 0;
            }
            return retorno;
        }

        /// <summary>
        /// Converto um string para um double, retornando zero, caso tenha erro na conversão.
        /// </summary>
        /// <param name="valor">dado a ser convertido</param>
        /// <returns>dado convertido</returns>
        public static double ConverteStringToDouble(string valor)
        {
            double retorno;
            try
            {
                valor = valor.Replace(",", ".");
                retorno = Convert.ToDouble(valor);
            }
            catch
            {
                retorno = 0.00;
            }
            return retorno;
        }

        /// <summary>
        /// Converto um string para um inteiro, retornando zero, caso tenha erro na conversão.
        /// </summary>
        /// <param name="valor">dado a ser convertido</param>
        /// <returns>dado convertido</returns>
        public static short ConverteStringToShort(string valor)
        {
            short retorno;
            try
            {
                retorno = Convert.ToInt16(valor);
            }
            catch
            {
                retorno = 0;
            }
            return retorno;
        }

        /// <summary>
        /// Converto um string para um inteiro longo, retornando zero, caso tenha erro na conversão.
        /// </summary>
        /// <param name="valor">dado a ser convertido</param>
        /// <returns>dado convertido</returns>
        public static long ConverteStringToLong(string valor)
        {
            long retorno;
            try
            {
                retorno = Convert.ToInt64(valor);
            }
            catch
            {
                retorno = 0;
            }
            return retorno;
        }

    }
}
