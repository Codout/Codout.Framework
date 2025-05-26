using System;
using System.ComponentModel.DataAnnotations;

namespace Codout.Framework.Common.Annotations;

/// <summary>
///     Valida um data entre os dias especificados.
/// </summary>
public class DateRangeAttribute : RangeAttribute
{
    #region Construtores

    /// <summary>
    ///     Especifique quantos dias esta data é válida apartir de hoje.
    ///     Coloque um valor negativo para datas retroativas.
    /// </summary>
    /// <param name="startDaysFromNow">Quantidade mínima de dias.</param>
    /// <param name="endDaysFromNow">Quantidade máxima de dias.</param>
    public DateRangeAttribute(int startDaysFromNow, int endDaysFromNow)
        : base(typeof(DateTime),
            DateTime.Now.AddDays(startDaysFromNow).ToShortDateString(),
            DateTime.Now.AddDays(endDaysFromNow).ToShortDateString())
    {
    }

    #endregion
}