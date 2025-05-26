using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Codout.Framework.Common.Helpers;

/// <summary>
///     Método auxiliar para Enumeradores
/// </summary>
public static class EnumHelper
{
    /// <summary>
    ///     Obtem a descrição de um enumerador a partir do Attribute
    ///     <see cref="DescriptionAttribute">DescriptionAttribute</see>
    /// </summary>
    /// <param name="value">Valor do Enumerador</param>
    /// <returns>Descrição do enumerador</returns>
    public static string GetDescription(this Enum value)
    {
        if (value == null)
            return string.Empty;

        var field = value.GetType().GetField(value.ToString());

        return Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is not DescriptionAttribute attribute
            ? value.ToString()
            : attribute.Description;
    }

    /// <summary>
    ///     Obtem a descrição de um enumerador a partir do Attribute
    ///     <see cref="DescriptionAttribute">DescriptionAttribute</see>
    /// </summary>
    /// <param name="value">Tipo para Enumerador</param>
    /// <param name="name">O valor correspondente ao nome do Enumerador</param>
    /// <returns>A descrição caso exista, caso contrario retorna o name passado</returns>
    public static string GetDescription(Type value, string name)
    {
        if (value == null)
            return string.Empty;

        var field = value.GetField(name);

        var attributes = (DescriptionAttribute[])field.GetCustomAttributes(typeof(DescriptionAttribute), false);
        return attributes.Length > 0 ? attributes[0].Description : name;
    }

    /// <summary>
    ///     Obtem o valor do enumerador a partir da descrição do <see cref="DescriptionAttribute">DescriptionAttribute</see>
    /// </summary>
    /// <typeparam name="T">Tipo do enumerador</typeparam>
    /// <param name="description">Descrição do Enumerador</param>
    /// <returns>Valor do Enumerador</returns>
    public static T GetValueFromDescription<T>(string description)
    {
        var type = typeof(T);
        if (!type.IsEnum) throw new InvalidOperationException();
        foreach (var field in type.GetFields())
            if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
            {
                if (attribute.Description == description)
                    return (T)field.GetValue(null);
            }
            else
            {
                if (field.Name == description)
                    return (T)field.GetValue(null);
            }

        return default;
    }

    public static string GetLocalizedName(this Enum @enum)
    {
        if (@enum == null)
            return null;

        var description = @enum.ToString();
        var fieldInfo = @enum.GetType().GetField(description);

        var attributes =
            (DisplayAttribute[])fieldInfo.GetCustomAttributes(typeof(DisplayAttribute), false);

        if (attributes.Any())
            description = attributes[0].GetDescription();

        return description;
    }

    public static DictionaryEntry[] GetDicionary(Type enumType)
    {
        var names = Enum.GetNames(enumType);
        var de = new DictionaryEntry[names.Length];

        for (var i = 0; i < names.Length; i++)
            de[i] = new DictionaryEntry(GetEnumValue(enumType, names[i]), GetDescription(enumType, names[i]));

        return de;
    }

    /// <summary>
    ///     Gets the value of an Enum, based on it's Description Attribute or named value
    /// </summary>
    /// <param name="value">The Enum type</param>
    /// <param name="description">The description or name of the element</param>
    /// <returns>The value, or the passed in description, if it was not found</returns>
    public static object GetEnumValue(Type value, string description)
    {
        var fis = value.GetFields();

        foreach (var fi in fis)
        {
            var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attributes.Length > 0)
                if (attributes[0].Description == description)
                    return fi.GetValue(fi.Name);

            if (fi.Name == description) return fi.GetValue(fi.Name);
        }

        return description;
    }
}