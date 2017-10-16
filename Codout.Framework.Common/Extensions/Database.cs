using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Codout.Framework.Common.Extensions
{
    /// <summary>
    /// Extensões comuns para tipos relacionadas a banco de dados.
    /// </summary>
    public static class Database
    {
        #region DbType.GetSqlDBType
        /// <summary>
        /// Returns the SqlDbType for a give DbType
        /// </summary>
        /// <returns>Retorna um SqlDbType.</returns>
        public static SqlDbType GetSqlDBType(this DbType dbType)
        {
            switch (dbType)
            {
                case DbType.AnsiString:
                    return SqlDbType.VarChar;
                case DbType.AnsiStringFixedLength:
                    return SqlDbType.Char;
                case DbType.Binary:
                    return SqlDbType.VarBinary;
                case DbType.Boolean:
                    return SqlDbType.Bit;
                case DbType.Byte:
                    return SqlDbType.TinyInt;
                case DbType.Currency:
                    return SqlDbType.Money;
                case DbType.Date:
                    return SqlDbType.DateTime;
                case DbType.DateTime:
                    return SqlDbType.DateTime;
                case DbType.Decimal:
                    return SqlDbType.Decimal;
                case DbType.Double:
                    return SqlDbType.Float;
                case DbType.Guid:
                    return SqlDbType.UniqueIdentifier;
                case DbType.Int16:
                    return SqlDbType.Int;
                case DbType.Int32:
                    return SqlDbType.Int;
                case DbType.Int64:
                    return SqlDbType.BigInt;
                case DbType.Object:
                    return SqlDbType.Variant;
                case DbType.SByte:
                    return SqlDbType.TinyInt;
                case DbType.Single:
                    return SqlDbType.Real;
                case DbType.String:
                    return SqlDbType.NVarChar;
                case DbType.StringFixedLength:
                    return SqlDbType.NChar;
                case DbType.Time:
                    return SqlDbType.DateTime;
                case DbType.UInt16:
                    return SqlDbType.Int;
                case DbType.UInt32:
                    return SqlDbType.Int;
                case DbType.UInt64:
                    return SqlDbType.BigInt;
                case DbType.VarNumeric:
                    return SqlDbType.Decimal;

                default:
                    return SqlDbType.VarChar;
            }
        }
        #endregion

        #region Type.GetDbType
        /// <summary>
        /// Busca o tipo de dados do banco relacionado o tipo de dados passado como argumento.
        /// </summary>
        /// <param name="type">Tipo do objeto.</param>
        /// <returns>Retorna o DbType relacionado.</returns>
        public static DbType GetDbType(Type type)
        {
            DbType result;

            if (type == typeof(Int32))
                result = DbType.Int32;
            else if (type == typeof(Int16))
                result = DbType.Int16;
            else if (type == typeof(Int64))
                result = DbType.Int64;
            else if (type == typeof(DateTime))
                result = DbType.DateTime;
            else if (type == typeof(float))
                result = DbType.Decimal;
            else if (type == typeof(decimal))
                result = DbType.Decimal;
            else if (type == typeof(double))
                result = DbType.Double;
            else if (type == typeof(Guid))
                result = DbType.Guid;
            else if (type == typeof(bool))
                result = DbType.Boolean;
            else if (type == typeof(byte[]))
                result = DbType.Binary;
            else
                result = DbType.String;

            return result;
        }
        #endregion

        #region IDataReader.Load<T>
        /// <summary>
        /// Coerces an IDataReader to try and load an object using name/property matching
        /// </summary>
        public static void Load<T>(this IDataReader rdr, T item, List<string> columnNames)
        {
            Type iType = typeof(T);

            PropertyInfo[] cachedProps = iType.GetProperties();
            FieldInfo[] cachedFields = iType.GetFields();

            PropertyInfo currentProp;
            FieldInfo currentField = null;

            for (int i = 0; i < rdr.FieldCount; i++)
            {
                string pName = rdr.GetName(i);
                currentProp = cachedProps.SingleOrDefault(x => x.Name.Equals(pName, StringComparison.InvariantCultureIgnoreCase));

                //mike if the property is null and ColumnNames has data then look in ColumnNames for match
                if (currentProp == null && columnNames != null && columnNames.Count > i)
                {
                    int i1 = i;
                    currentProp = cachedProps.First(x => x.Name == columnNames[i1]);
                }

                //if the property is null, likely it's a Field
                if (currentProp == null)
                    currentField = cachedFields.SingleOrDefault(x => x.Name.Equals(pName, StringComparison.InvariantCultureIgnoreCase));

                if (currentProp != null && !DBNull.Value.Equals(rdr.GetValue(i)))
                {
                    Type valueType = rdr.GetValue(i).GetType();
                    if (valueType == typeof(Boolean))
                    {
                        string value = rdr.GetValue(i).ToString();
                        currentProp.SetValue(item, value == "1" || value == "True", null);
                    }
                    else if (currentProp.PropertyType == typeof(Guid))
                    {
                        currentProp.SetValue(item, rdr.GetGuid(i), null);
                    }
                    else if (Objects.IsNullableEnum(currentProp.PropertyType))
                    {
                        var nullEnumObjectValue = Enum.ToObject(Nullable.GetUnderlyingType(currentProp.PropertyType), rdr.GetValue(i));
                        currentProp.SetValue(item, nullEnumObjectValue, null);
                    }
                    else if (currentProp.PropertyType.IsEnum)
                    {
                        var enumValue = Enum.ToObject(currentProp.PropertyType, rdr.GetValue(i));
                        currentProp.SetValue(item, enumValue, null);
                    }
                    else
                    {

                        var val = rdr.GetValue(i);
                        //try to assign it
                        currentProp.SetValue(item,
                            !currentProp.PropertyType.IsAssignableFrom(valueType)
                                ? val.ChangeTypeTo(currentProp.PropertyType)
                                : val, null);
                    }
                }
                else if (currentField != null && !DBNull.Value.Equals(rdr.GetValue(i)))
                {
                    Type valueType = rdr.GetValue(i).GetType();
                    if (valueType == typeof(Boolean))
                    {
                        string value = rdr.GetValue(i).ToString();
                        currentField.SetValue(item, value == "1" || value == "True");
                    }
                    else if (currentField.FieldType == typeof(Guid))
                    {
                        currentField.SetValue(item, rdr.GetGuid(i));
                    }
                    else if (Objects.IsNullableEnum(currentField.FieldType))
                    {
                        var nullEnumObjectValue = Enum.ToObject(Nullable.GetUnderlyingType(currentField.FieldType), rdr.GetValue(i));
                        currentField.SetValue(item, nullEnumObjectValue);
                    }
                    else
                    {
                        var val = rdr.GetValue(i);
                        //try to assign it
                        currentField.SetValue(item,
                            currentField.FieldType.IsAssignableFrom(valueType)
                                ? val
                                : val.ChangeTypeTo(currentField.FieldType));
                    }
                }
            }
        }
        #endregion

        #region IDataReader.LoadValueType<T>
        /// <summary>
        /// Loads a single primitive value type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void LoadValueType<T>(this IDataReader rdr, ref T item)
        {
            Type iType = typeof(T);
            //thanks to Pascal LaCroix for the help here...

            if (iType.IsValueType)
            {
                // We assume only one field
                if (iType == typeof(Int16) || iType == typeof(Int32) || iType == typeof(Int64))
                    item = (T)Convert.ChangeType(rdr.GetValue(0), iType);
                else
                    item = (T)rdr.GetValue(0);
            }
        }
        #endregion

        #region IDataReader.ToEnumerableValueType<T>
        /// <summary>
        /// Toes the type of the enumerable value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rdr">The IDataReader to read from.</param>
        /// <returns></returns>
        [Obsolete("See ToEnumerable")]
        public static IEnumerable<T> ToEnumerableValueType<T>(this IDataReader rdr)
        {
            //thanks to Pascal LaCroix for the help here...
            var result = new List<T>();
            while (rdr.Read())
            {
                var instance = Activator.CreateInstance<T>();
                LoadValueType(rdr, ref instance);
                result.Add(instance);
            }
            return result.AsEnumerable();
        }
        #endregion

        #region Type.IsCoreSystemType
        /// <summary>
        /// Determines whether [is core system type] [the specified type].
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        /// 	<c>true</c> if [is core system type] [the specified type]; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsCoreSystemType(Type type)
        {
            return type == typeof(string) ||
                   type == typeof(Int16) ||
                   type == typeof(Int16?) ||
                   type == typeof(Int32) ||
                   type == typeof(Int32?) ||
                   type == typeof(Int64) ||
                   type == typeof(Int64?) ||
                   type == typeof(decimal) ||
                   type == typeof(decimal?) ||
                   type == typeof(double) ||
                   type == typeof(double?) ||
                   type == typeof(DateTime) ||
                   type == typeof(DateTime?) ||
                   type == typeof(Guid) ||
                   type == typeof(Guid?) ||
                   type == typeof(bool) ||
                   type == typeof(bool?);
        }
        #endregion

        #region IDataReader.ToEnumerable<T>
        /// <summary>
        /// Coerces an IDataReader to load an enumerable of T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rdr"></param>
        /// <param name="columnNames"></param>
        /// <param name="onItemCreated">Invoked when a new item is created</param>
        public static IEnumerable<T> ToEnumerable<T>(this IDataReader rdr, List<string> columnNames, Func<object, object> onItemCreated)
        {
            //mike added ColumnNames
            var result = new List<T>();
            while (rdr.Read())
            {
                T instance;
                var type = typeof(T);
                if (type.Name.Contains("AnonymousType"))
                {

                    //this is an anon type and it has read-only fields that are set
                    //in a constructor. So - read the fields and build it
                    //http://stackoverflow.com/questions/478013/how-do-i-create-and-access-a-new-instance-of-an-anonymous-class-passed-as-a-param
                    var properties = type.GetProperties();
                    int objIdx = 0;
                    var objArray = new object[properties.Length];

                    foreach (var prop in properties)
                    {
                        objArray[objIdx++] = rdr[prop.Name];
                    }

                    result.Add((T)Activator.CreateInstance(type, objArray));
                }
                //TODO: there has to be a better way to work with the type system
                else if (IsCoreSystemType(type))
                {
                    instance = (T)rdr.GetValue(0).ChangeTypeTo(type);
                    result.Add(instance);
                }
                else if (type.IsValueType)
                {
                    instance = Activator.CreateInstance<T>();
                    LoadValueType(rdr, ref instance);
                    result.Add(instance);
                }
                else
                {
                    instance = Activator.CreateInstance<T>();

                    if (onItemCreated != null)
                    {
                        instance = (T)onItemCreated(instance);
                    }

                    //do we have a parameterless constructor?
                    Load(rdr, instance, columnNames);//mike added ColumnNames
                    result.Add(instance);
                }
            }

            return result;
        }
        #endregion

        #region IDataReader.ToList<T>
        /// <summary>
        /// Converte um DataReader para uma lista.
        /// </summary>
        /// <param name="rdr"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<T> ToList<T>(this IDataReader rdr) where T : new()
        {
            return rdr.ToList<T>(null);
        }
        #endregion

        #region IDataReader.ToList<T>
        /// <summary>
        /// Creates a typed list from an IDataReader
        /// </summary>
        public static List<T> ToList<T>(this IDataReader rdr, Func<object, object> onItemCreated) where T : new()
        {
            var result = new List<T>();

            //set the values        
            while (rdr.Read())
            {
                var item = new T();

                if (onItemCreated != null)
                {
                    item = (T)onItemCreated(item);
                }

                rdr.Load(item, null);//mike added null to match ColumnNames
                result.Add(item);
            }
            return result;
        }
        #endregion
    }
}
