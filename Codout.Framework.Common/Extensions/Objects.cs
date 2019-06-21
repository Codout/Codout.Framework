using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace Codout.Framework.Common.Extensions
{
    /// <summary>
    /// Extensões comuns para tipos relacionadas a objetos.
    /// </summary>
    public static class Objects
    {
        #region ChangeTypeTo<T>
        /// <summary>
        /// Returns an Object with the specified Type and whose value is equivalent to the specified object.
        /// </summary>
        /// <param name="value">An Object that implements the IConvertible interface.</param>
        /// <returns>
        /// An object whose Type is conversionType (or conversionType's underlying type if conversionType
        /// is Nullable&lt;&gt;) and whose value is equivalent to value. -or- a null reference, if value is a null
        /// reference and conversionType is not a value type.
        /// </returns>
        /// <remarks>
        /// This method exists as a workaround to System.Convert.ChangeType(Object, Type) which does not handle
        /// nullables as of version 2.0 (2.0.50727.42) of the .NET Framework. The idea is that this method will
        /// be deleted once Convert.ChangeType is updated in a future version of the .NET Framework to handle
        /// nullable types, so we want this to behave as closely to Convert.ChangeType as possible.
        /// This method was written by Peter Johnson at:
        /// http://aspalliance.com/author.aspx?uId=1026.
        /// </remarks>
        /// 
        public static object ChangeTypeTo<T>(this object value)
        {
            var conversionType = typeof(T);
            return ChangeTypeTo(value, conversionType);
        }
        #endregion

        #region ChangeTypeTo
        /// <summary>
        /// Returns an Object with the specified Type and whose value is equivalent to the specified object.
        /// </summary>
        /// <param name="value">An Object that implements the IConvertible interface.</param>
        /// <param name="conversionType"></param>
        /// <returns>
        /// An object whose Type is conversionType (or conversionType's underlying type if conversionType
        /// is Nullable&lt;&gt;) and whose value is equivalent to value. -or- a null reference, if value is a null
        /// reference and conversionType is not a value type.
        /// </returns>
        public static object ChangeTypeTo(this object value, Type conversionType)
        {
            // Note: This if block was taken from Convert.ChangeType as is, and is needed here since we're
            // checking properties on conversionType below.
            if (conversionType == null)
                throw new ArgumentNullException("conversionType");

            // If it's not a nullable type, just pass through the parameters to Convert.ChangeType

            if (conversionType.IsGenericType && conversionType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                // It's a nullable type, so instead of calling Convert.ChangeType directly which would throw a
                // InvalidCastException (per http://weblogs.asp.net/pjohnson/archive/2006/02/07/437631.aspx),
                // determine what the underlying type is
                // If it's null, it won't convert to the underlying type, but that's fine since nulls don't really
                // have a type--so just return null
                // Note: We only do this check if we're converting to a nullable type, since doing it outside
                // would diverge from Convert.ChangeType's behavior, which throws an InvalidCastException if
                // value is null and conversionType is a value type.
                if (value == null)
                    return null;

                // It's a nullable type, and not null, so that means it can be converted to its underlying type,
                // so overwrite the passed-in conversion type with this underlying type
                var nullableConverter = new NullableConverter(conversionType);
                conversionType = nullableConverter.UnderlyingType;
            }
            else if (conversionType == typeof(Guid))
            {
                return new Guid(value.ToString());

            }
            else if (conversionType == typeof(Int64) && value is int)
            {
                //there is an issue with SQLite where the PK is ALWAYS int64. If this conversion type is Int64
                //we need to throw here - suggesting that they need to use LONG instead


                throw new InvalidOperationException("Can't convert an Int64 (long) to Int32(int). If you're using SQLite - this is probably due to your PK being an INTEGER, which is 64bit. You'll need to set your key to long.");
            }

            // Now that we've guaranteed conversionType is something Convert.ChangeType can handle (i.e. not a
            // nullable type), pass the call on to Convert.ChangeType
            return Convert.ChangeType(value, conversionType);
        }
        #endregion

        #region ToDictionary
        /// <summary>
        /// Converte as propriedades de um objeto para um dicionário contendo o nome da propriedade e valor.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Dictionary<string, object> ToDictionary(this object value)
        {
            var result = new Dictionary<string, object>();
            PropertyInfo[] props = value.GetType().GetProperties();
            foreach (PropertyInfo pi in props)
            {
                try
                {
                    result.Add(pi.Name, pi.GetValue(value, null));
                }
                // ReSharper disable EmptyGeneralCatchClause
                catch { }
                // ReSharper restore EmptyGeneralCatchClause
            }
            return result;
        }
        #endregion

        #region FromDictionary<T>
        /// <summary>
        /// Copia os valores do dicionário para as propriedades do objeto.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="item"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T FromDictionary<T>(this Dictionary<string, object> settings, T item) where T : class
        {
            PropertyInfo[] props = item.GetType().GetProperties();
            //FieldInfo[] fields = item.GetType().GetFields();
            foreach (PropertyInfo pi in props)
            {
                if (settings.ContainsKey(pi.Name))
                {
                    if (pi.CanWrite)
                        pi.SetValue(item, settings[pi.Name], null);
                }
            }
            return item;
        }
        #endregion

        #region CopyTo<T>
        /// <summary>
        /// Copias as propriedades de um objeto para outro.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T CopyTo<T>(this object from, T to) where T : class
        {
            var settings = from.ToDictionary();

            to = settings.FromDictionary(to);

            return to;
        }
        #endregion

        #region IsNullableEnum
        /// <summary>
        /// Verifica se o enum é do tipo Nullable.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static bool IsNullableEnum(Type type)
        {
            var enumType = Nullable.GetUnderlyingType(type);

            return enumType != null && enumType.IsEnum;
        }
        #endregion
    }
}
