using System;
using System.ComponentModel.DataAnnotations;
using Codout.Framework.Common.Extensions;

namespace Codout.Framework.Common.Annotations;

/// <summary>
///     Valida um CPF ou CNPJ.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class CpfCnpjAttribute : ValidationAttribute
{
    #region IsValid

    /// <summary>
    ///     Determina se o valor especificado do objeto é válido.
    /// </summary>
    /// <returns>
    ///     true se o valor especificado é válido; Caso contrário, false.
    /// </returns>
    /// <param name="value">O valor do objeto a ser validado. </param>
    public override bool IsValid(object value)
    {
        var cpfCnpj = value as string;

        if (string.IsNullOrEmpty(cpfCnpj))
            return true;

        cpfCnpj = cpfCnpj.Replace(".", string.Empty)
            .Replace("/", string.Empty)
            .Replace("-", string.Empty)
            .Replace("_", string.Empty)
            .Replace(" ", string.Empty);

        switch (cpfCnpj.Length)
        {
            case 11:
                return cpfCnpj.IsCpf();
            case 14:
                return cpfCnpj.IsCnpj();
            default:
                return false;
        }
    }

    #endregion
}