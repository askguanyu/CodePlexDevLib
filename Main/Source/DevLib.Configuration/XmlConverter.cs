//-----------------------------------------------------------------------
// <copyright file="XmlConverter.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Configuration
{
    using System;
    using System.Xml;

    /// <summary>
    /// Provides methods for converting between common language runtime types and XML Schema definition language (XSD) types.
    /// When converting data types the values returned are locale independent.
    /// </summary>
    internal static class XmlConverter
    {
        /// <summary>
        /// Returns whether this converter can convert the object to a <see cref="T:System.String" /> and vice versa.
        /// </summary>
        /// <param name="sourceType">A <see cref="T:System.Type" /> that represents the type you want to convert.</param>
        /// <returns>true if this converter can perform the conversion; otherwise, false.</returns>
        public static bool CanConvert(Type sourceType)
        {
            if (Type.GetTypeCode(sourceType) == TypeCode.Object &&
                !sourceType.IsEnum &&
                sourceType != typeof(Guid) &&
                sourceType != typeof(TimeSpan) &&
                sourceType != typeof(DateTimeOffset) &&
                !IsNullableCanConvert(sourceType))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Converts object to a <see cref="T:System.String" />.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>A string representation of the object.</returns>
        public static string ToString(object value)
        {
            if (value == null)
            {
                return null;
            }

            Type sourceType = value.GetType();

            switch (Type.GetTypeCode(sourceType))
            {
                case TypeCode.Boolean:
                    return XmlConvert.ToString((bool)value);
                case TypeCode.Byte:
                    return XmlConvert.ToString((byte)value);
                case TypeCode.Char:
                    return XmlConvert.ToString((char)value);
                case TypeCode.DBNull:
                    return string.Empty;
                case TypeCode.DateTime:
                    return XmlConvert.ToString((DateTime)value, XmlDateTimeSerializationMode.RoundtripKind);
                case TypeCode.Decimal:
                    return XmlConvert.ToString((decimal)value);
                case TypeCode.Double:
                    return XmlConvert.ToString((double)value);
                case TypeCode.Empty:
                    return string.Empty;
                case TypeCode.Int16:
                    return XmlConvert.ToString((short)value);
                case TypeCode.Int32:
                    return XmlConvert.ToString((int)value);
                case TypeCode.Int64:
                    return XmlConvert.ToString((long)value);
                case TypeCode.SByte:
                    return XmlConvert.ToString((sbyte)value);
                case TypeCode.Single:
                    return XmlConvert.ToString((float)value);
                case TypeCode.String:
                    return (string)value;
                case TypeCode.UInt16:
                    return XmlConvert.ToString((ushort)value);
                case TypeCode.UInt32:
                    return XmlConvert.ToString((uint)value);
                case TypeCode.UInt64:
                    return XmlConvert.ToString((ulong)value);
                case TypeCode.Object:
                default:

                    if (sourceType.IsEnum)
                    {
                        return value.ToString();
                    }

                    if (IsNullableCanConvert(sourceType))
                    {
                        return ToString(Convert.ChangeType(value, Nullable.GetUnderlyingType(sourceType)));
                    }

                    if (sourceType == typeof(Guid))
                    {
                        return XmlConvert.ToString((Guid)value);
                    }

                    if (sourceType == typeof(TimeSpan))
                    {
                        return XmlConvert.ToString((TimeSpan)value);
                    }

                    if (sourceType == typeof(DateTimeOffset))
                    {
                        return XmlConvert.ToString((DateTimeOffset)value);
                    }

                    break;
            }

            return null;
        }

        /// <summary>
        /// Converts the <see cref="T:System.String" /> to a object equivalent.
        /// </summary>
        /// <param name="value">The string to convert.</param>
        /// <param name="targetType">Target type to convert.</param>
        /// <returns>An object equivalent of the string.</returns>
        public static object ToObject(string value, Type targetType)
        {
            if (value == null)
            {
                return null;
            }

            switch (Type.GetTypeCode(targetType))
            {
                case TypeCode.Boolean:
                    return XmlConvert.ToBoolean(value);
                case TypeCode.Byte:
                    return XmlConvert.ToByte(value);
                case TypeCode.Char:
                    return XmlConvert.ToChar(value);
                case TypeCode.DBNull:
                    return DBNull.Value;
                case TypeCode.DateTime:
                    return XmlConvert.ToDateTime(value, XmlDateTimeSerializationMode.RoundtripKind);
                case TypeCode.Decimal:
                    return XmlConvert.ToDecimal(value);
                case TypeCode.Double:
                    return XmlConvert.ToDouble(value);
                case TypeCode.Empty:
                    return null;
                case TypeCode.Int16:
                    return XmlConvert.ToInt16(value);
                case TypeCode.Int32:
                    return XmlConvert.ToInt32(value);
                case TypeCode.Int64:
                    return XmlConvert.ToInt64(value);
                case TypeCode.SByte:
                    return XmlConvert.ToSByte(value);
                case TypeCode.Single:
                    return XmlConvert.ToSingle(value);
                case TypeCode.String:
                    return value;
                case TypeCode.UInt16:
                    return XmlConvert.ToUInt16(value);
                case TypeCode.UInt32:
                    return XmlConvert.ToUInt32(value);
                case TypeCode.UInt64:
                    return XmlConvert.ToUInt64(value);
                case TypeCode.Object:
                default:

                    if (targetType.IsEnum)
                    {
                        if (Enum.IsDefined(targetType, value))
                        {
                            return Enum.Parse(targetType, value);
                        }

                        Type enumType = Enum.GetUnderlyingType(targetType);

                        object rawValue = ToObject(value, enumType);

                        return Enum.ToObject(targetType, rawValue);
                    }

                    if (IsNullableCanConvert(targetType))
                    {
                        return ToObject(value, Nullable.GetUnderlyingType(targetType));
                    }

                    if (targetType == typeof(Guid))
                    {
                        return XmlConvert.ToGuid(value);
                    }

                    if (targetType == typeof(TimeSpan))
                    {
                        return XmlConvert.ToTimeSpan(value);
                    }

                    if (targetType == typeof(DateTimeOffset))
                    {
                        return XmlConvert.ToDateTimeOffset(value);
                    }

                    break;
            }

            return null;
        }

        /// <summary>
        /// Converts the <see cref="T:System.String" /> to a object equivalent.
        /// </summary>
        /// <typeparam name="T">Target type to convert.</typeparam>
        /// <param name="value">The string to convert.</param>
        /// <returns>An object equivalent of the string.</returns>
        public static T ToObject<T>(string value)
        {
            return (T)ToObject(value, typeof(T));
        }

        /// <summary>
        /// Method IsNullableCanConvert.
        /// </summary>
        /// <param name="value">The type to check.</param>
        /// <returns>true if the type is Nullable{} type; otherwise, false.</returns>
        private static bool IsNullableCanConvert(Type value)
        {
            return value.IsGenericType && value.GetGenericTypeDefinition() == typeof(Nullable<>) && CanConvert(Nullable.GetUnderlyingType(value));
        }
    }
}
