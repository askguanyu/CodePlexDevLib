//-----------------------------------------------------------------------
// <copyright file="MetadataModelExtensions.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Metadata
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Xml;

    /// <summary>
    /// Class MetadataModelExtensions.
    /// </summary>
    public static class MetadataModelExtensions
    {
        /// <summary>
        /// Convert object to MetadataModel instance.
        /// </summary>
        /// <param name="source">Object to convert.</param>
        /// <param name="sourceType">The type of source object.</param>
        /// <param name="name">Name for result MetadataModel.</param>
        /// <returns>Instance of MetadataModel.</returns>
        public static MetadataModel ToMetadataModel(this object source, Type sourceType, string name = null)
        {
            if (source == null)
            {
                return MetadataModel.GetEmptyMetadataModel();
            }

            if (sourceType.Equals(typeof(MetadataModel)))
            {
                return (source as MetadataModel) ?? MetadataModel.GetEmptyMetadataModel();
            }

            MetadataModel result = new MetadataModel { Name = name ?? sourceType.FullName.Replace('.', '_') };

            if (XmlConverter.CanConvert(sourceType))
            {
                result.IsValueType = true;

                try
                {
                    result.Value = XmlConverter.ToString(source);
                }
                catch
                {
                    result.Value = null;
                }
            }
            else if (source is XmlElement)
            {
                result.IsValueType = true;

                try
                {
                    result.Value = (source as XmlElement).OuterXml;
                }
                catch
                {
                    result.Value = null;
                }
            }
            else
            {
                result.IsValueType = false;

                if (CanEnumerable(sourceType))
                {
                    if (source != null)
                    {
                        foreach (var item in source as IEnumerable)
                        {
                            result.AddProperty(item.ToMetadataModel(item.GetType()));
                        }
                    }
                    else
                    {
                        Type elementType = null;

                        if (﻿IsDictionary(sourceType))
                        {
                            elementType = typeof(KeyValuePair<,>).MakeGenericType(sourceType.GetGenericArguments());
                        }
                        else
                        {
                            elementType = sourceType.IsArray ? sourceType.GetElementType() : sourceType.GetGenericArguments()[0];
                        }

                        result.AddProperty(((object)null).ToMetadataModel(elementType));
                    }
                }
                else
                {
                    foreach (var item in sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanRead))
                    {
                        try
                        {
                            object itemValue = source == null ? null : item.GetValue(source, null);

                            result.AddProperty(itemValue.ToMetadataModel(item.PropertyType, item.Name));
                        }
                        catch
                        {
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Convert object to MetadataModel instance.
        /// </summary>
        /// <typeparam name="T">The type of source object.</typeparam>
        /// <param name="source">Object to convert.</param>
        /// <param name="name">Name for result MetadataModel.</param>
        /// <returns>Instance of MetadataModel.</returns>
        public static MetadataModel ToMetadataModel<T>(this T source, string name = null)
        {
            return source.ToMetadataModel(typeof(T), name);
        }

        /// <summary>
        /// Convert MetadataModel instance to object.
        /// </summary>
        /// <param name="source">MetadataModel to convert.</param>
        /// <param name="targetType">The type of return object.</param>
        /// <returns>The target type object.</returns>
        public static object ToObject(this MetadataModel source, Type targetType)
        {
            if (source.IsNull)
            {
                return null;
            }

            if (targetType.Equals(typeof(MetadataModel)))
            {
                return source;
            }

            if (XmlConverter.CanConvert(targetType))
            {
                if (source.IsValueType && source.Value != null)
                {
                    return XmlConverter.ToObject(source.Value, targetType);
                }
                else
                {
                    return null;
                }
            }
            else if (targetType.Equals(typeof(XmlElement)))
            {
                if (source.IsValueType && source.Value != null)
                {
                    try
                    {
                        XmlDocument xmlDocument = new XmlDocument();
                        xmlDocument.LoadXml(source.Value);
                        return xmlDocument.DocumentElement;
                    }
                    catch
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            else
            {
                if (CanEnumerable(targetType))
                {
                    if (﻿IsDictionary(targetType))
                    {
                        IDictionary propertyDict = (IDictionary)Activator.CreateInstance(targetType, true);

                        foreach (var propertyItem in source.Properties)
                        {
                            if (!propertyItem.IsNull)
                            {
                                try
                                {
                                    var key = propertyItem[0].ToObject(targetType.GetGenericArguments()[0]);

                                    if (key != null)
                                    {
                                        var value = propertyItem[1].ToObject(targetType.GetGenericArguments()[1]);

                                        propertyDict.Add(key, value);
                                    }
                                }
                                catch
                                {
                                }
                            }
                        }

                        return propertyDict.Count > 0 ? propertyDict : (IDictionary)null;
                    }
                    else
                    {
                        if (targetType.IsArray)
                        {
                            Type elementType = targetType.GetElementType();

                            IList resultTemp = new List<dynamic>();

                            foreach (var propertyItem in source.Properties)
                            {
                                if (!propertyItem.IsNull)
                                {
                                    try
                                    {
                                        var listItem = propertyItem.ToObject(elementType);

                                        if (listItem != null)
                                        {
                                            resultTemp.Add(listItem);
                                        }
                                    }
                                    catch
                                    {
                                    }
                                }
                            }

                            if (resultTemp.Count > 0)
                            {
                                Array result = Array.CreateInstance(elementType, resultTemp.Count);

                                resultTemp.CopyTo(result, 0);

                                return result;
                            }
                            else
                            {
                                return (IList)null;
                            }
                        }
                        else
                        {
                            Type elementType = targetType.GetGenericArguments()[0];

                            IList result = (IList)Activator.CreateInstance(targetType, true);

                            foreach (var propertyItem in source.Properties)
                            {
                                if (!propertyItem.IsNull)
                                {
                                    try
                                    {
                                        var listItem = propertyItem.ToObject(elementType);

                                        if (listItem != null)
                                        {
                                            result.Add(listItem);
                                        }
                                    }
                                    catch
                                    {
                                    }
                                }
                            }

                            return result.Count > 0 ? result : (IList)null;
                        }
                    }
                }
                else
                {
                    object result = null;

                    try
                    {
                        result = Activator.CreateInstance(targetType, true);
                    }
                    catch
                    {
                        try
                        {
                            result = FormatterServices.GetUninitializedObject(targetType);
                        }
                        catch
                        {
                        }
                    }

                    if (result == null)
                    {
                        return result;
                    }

                    bool isNullReturn = true;

                    foreach (var item in targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(i => i.CanWrite))
                    {
                        try
                        {
                            if (source.HasProperty(item.Name))
                            {
                                var sourceItem = source.GetProperty(item.Name);

                                var sourceItemValue = sourceItem.ToObject(item.PropertyType);

                                item.SetValue(result, sourceItemValue, null);

                                if (sourceItemValue != null)
                                {
                                    isNullReturn = false;
                                }
                            }
                        }
                        catch
                        {
                        }
                    }

                    return isNullReturn ? null : result;
                }
            }
        }

        /// <summary>
        /// Convert MetadataModel instance to object.
        /// </summary>
        /// <typeparam name="T">The type of return object.</typeparam>
        /// <param name="source">MetadataModel to convert.</param>
        /// <returns>The target type object.</returns>
        public static T ToObject<T>(this MetadataModel source)
        {
            return (T)source.ToObject(typeof(T));
        }

        /// <summary>
        /// Convert MetadataModel instance to object.
        /// </summary>
        /// <typeparam name="T">The type of return object.</typeparam>
        /// <param name="source">MetadataModel to convert.</param>
        /// <returns>The target type object.</returns>
        public static T ToObject<T>(this MetadataModel<T> source)
        {
            return (T)source.ToObject(typeof(T));
        }

        /// <summary>
        /// Clone current MetadataModel to target MetadataModel.
        /// </summary>
        /// <param name="source">Current MetadataModel.</param>
        /// <param name="destModel">Destination MetadataModel.</param>
        /// <param name="includeName">true if want to include Metadata name; otherwise, false.</param>
        /// <returns>The destination MetadataModel.</returns>
        public static MetadataModel CloneTo(this MetadataModel source, MetadataModel destModel, bool includeName = false)
        {
            if (source == null)
            {
                destModel.Value = null;
                destModel.Properties = null;
            }
            else
            {
                if (includeName)
                {
                    destModel.Name = source.Name;
                }

                destModel.IsValueType = source.IsValueType;

                destModel.Value = source.Value;

                destModel.Properties = CloneDeep(source.Properties) as List<MetadataModel>;
            }

            return destModel;
        }

        /// <summary>
        /// Clone target MetadataModel to current MetadataModel.
        /// </summary>
        /// <param name="source">Current MetadataModel.</param>
        /// <param name="sourceModel">Target MetadataModel.</param>
        /// <param name="includeName">true if want to include Metadata name; otherwise, false.</param>
        /// <returns>The current MetadataModel.</returns>
        public static MetadataModel CloneFrom(this MetadataModel source, MetadataModel sourceModel, bool includeName = false)
        {
            if (sourceModel == null)
            {
                source.Value = null;
                source.Properties = null;
            }
            else
            {
                if (includeName)
                {
                    source.Name = sourceModel.Name;
                }

                source.IsValueType = sourceModel.IsValueType;
                source.Value = sourceModel.Value;
                source.Properties = CloneDeep(sourceModel.Properties) as List<MetadataModel>;
            }

            return source;
        }

        /// <summary>
        /// Gets the property names.
        /// </summary>
        /// <param name="source">The source type.</param>
        /// <param name="propertyAccess">The property access.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>The list of the property name and type.</returns>
        public static List<KeyValuePair<string, Type>> GetPropertyNames(this Type source, Func<PropertyInfo, bool> propertyAccess, string propertyName = null)
        {
            if (source == typeof(void)
                || XmlConverter.CanConvert(source)
                || source == typeof(XmlElement))
            {
                return GetPropertyNamesInternal(source, propertyAccess, propertyName);
            }
            else
            {
                return GetPropertyNamesInternal(source, propertyAccess, string.Empty);
            }
        }

        /// <summary>
        /// Method CanConvertible.
        /// </summary>
        /// <param name="source">Source Type.</param>
        /// <returns>true if the source Type inherit IConvertible interface; otherwise, false.</returns>
        private static bool CanConvertible(Type source)
        {
            return source.GetInterface("IConvertible") != null || source.Equals(typeof(Guid));
        }

        /// <summary>
        /// Method CanEnumerable.
        /// </summary>
        /// <param name="source">Source Type.</param>
        /// <returns>true if the source Type inherit IEnumerable interface; otherwise, false.</returns>
        private static bool CanEnumerable(Type source)
        {
            return source != typeof(string) && source.GetInterface("IEnumerable") != null;
        }

        /// <summary>
        /// Method IsDictionary.
        /// </summary>
        /// <param name="source">Source Type.</param>
        /// <returns>true if the source Type inherit IDictionary interface; otherwise, false.</returns>
        private static bool IsDictionary(Type source)
        {
            return source.GetInterface("IDictionary") != null;
        }

        /// <summary>
        /// Perform a deep Copy of the object.
        /// </summary>
        /// <param name="source">The object instance to copy.</param>
        /// <returns>The copied object.</returns>
        private static object CloneDeep(object source)
        {
            if (source == null)
            {
                return null;
            }

            if (!source.GetType().IsSerializable)
            {
                throw new ArgumentException("The type must be serializable.", "source");
            }

            BinaryFormatter binaryFormatter = new BinaryFormatter();

            using (MemoryStream memoryStream = new MemoryStream())
            {
                binaryFormatter.Serialize(memoryStream, source);
                memoryStream.Seek(0, SeekOrigin.Begin);
                return binaryFormatter.Deserialize(memoryStream);
            }
        }

        /// <summary>
        /// Gets the property names internal.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="propertyAccess">The property access.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>List of properties.</returns>
        private static List<KeyValuePair<string, Type>> GetPropertyNamesInternal(Type source, Func<PropertyInfo, bool> propertyAccess, string propertyName = null)
        {
            List<KeyValuePair<string, Type>> result = new List<KeyValuePair<string, Type>>();

            if (source == typeof(void)
                || XmlConverter.CanConvert(source)
                || source == typeof(XmlElement))
            {
                if (!string.IsNullOrWhiteSpace(propertyName))
                {
                    result.Add(new KeyValuePair<string, Type>(propertyName, source));
                }

                return result;
            }
            else
            {
                if (CanEnumerable(source))
                {
                    Type elementType = source.IsArray ? source.GetElementType() : source.GetGenericArguments()[0];

                    result.AddRange(GetPropertyNamesInternal(elementType, propertyAccess, propertyName + "[0]"));
                }
                else
                {
                    foreach (var item in source.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(propertyAccess))
                    {
                        Type type = item.PropertyType;

                        if (type == source)
                        {
                            continue;
                        }
                        else if (CanEnumerable(type))
                        {
                            Type elementType = type.IsArray ? type.GetElementType() : type.GetGenericArguments()[0];

                            if (elementType == source)
                            {
                                continue;
                            }
                        }

                        string name;

                        if (string.IsNullOrWhiteSpace(propertyName))
                        {
                            name = item.Name;
                        }
                        else
                        {
                            name = propertyName + "." + item.Name;
                        }

                        result.AddRange(GetPropertyNamesInternal(type, propertyAccess, name));
                    }
                }
            }

            return result;
        }
    }
}
