//-----------------------------------------------------------------------
// <copyright file="DynamicXml.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Dynamic
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Serialization;

    /// <summary>
    /// Provides a class for specifying dynamic Xml behavior at run time.
    /// </summary>
    public class DynamicXml : DynamicObject, IEnumerable<KeyValuePair<string, DynamicXml>>, IEnumerable
    {
        /// <summary>
        /// Field XmlConverters.
        /// </summary>
        private static readonly Dictionary<Type, Func<string, object>> XmlConverters;

        /// <summary>
        /// Field _xElement.
        /// </summary>
        private readonly XElement _xElement;

        /// <summary>
        /// Field _xAttribute.
        /// </summary>
        private readonly XAttribute _xAttribute;

        /// <summary>
        /// Initializes static members of the <see cref="DynamicXml" /> class.
        /// </summary>
        static DynamicXml()
        {
            XmlConverters = new Dictionary<Type, Func<string, object>>
            {
                { typeof(bool), s => XmlConvert.ToBoolean(s) },
                { typeof(byte), s => XmlConvert.ToByte(s) },
                { typeof(char), s => XmlConvert.ToChar(s) },
                { typeof(DateTime), s => XmlConvert.ToDateTime(s, XmlDateTimeSerializationMode.RoundtripKind) },
                { typeof(DateTimeOffset), s => XmlConvert.ToDateTimeOffset(s) },
                { typeof(decimal), s => XmlConvert.ToDecimal(s) },
                { typeof(double), s => XmlConvert.ToDouble(s) },
                { typeof(Guid), s => XmlConvert.ToGuid(s) },
                { typeof(short), s => XmlConvert.ToInt16(s) },
                { typeof(int), s => XmlConvert.ToInt32(s) },
                { typeof(long), s => XmlConvert.ToInt64(s) },
                { typeof(sbyte), s => XmlConvert.ToSByte(s) },
                { typeof(float), s => XmlConvert.ToSingle(s) },
                { typeof(TimeSpan), s => XmlConvert.ToTimeSpan(s) },
                { typeof(ushort), s => XmlConvert.ToUInt16(s) },
                { typeof(uint), s => XmlConvert.ToUInt32(s) },
                { typeof(ulong), s => XmlConvert.ToUInt64(s) },
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicXml" /> class.
        /// </summary>
        public DynamicXml()
        {
            this._xElement = new XElement("root");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicXml" /> class.
        /// </summary>
        /// <param name="xElement">XElement instance.</param>
        public DynamicXml(XElement xElement)
        {
            this._xElement = xElement;
            this._xAttribute = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicXml" /> class.
        /// </summary>
        /// <param name="xAttribute">XAttribute instance.</param>
        public DynamicXml(XAttribute xAttribute)
        {
            this._xElement = null;
            this._xAttribute = xAttribute;
        }

        /// <summary>
        /// Parse Xml string to DynamicXml.
        /// </summary>
        /// <param name="xmlString">Source Xml string.</param>
        /// <returns>DynamicXml object.</returns>
        public static dynamic Parse(string xmlString)
        {
            return new DynamicXml(XElement.Parse(xmlString));
        }

        /// <summary>
        /// Parse Xml string stream to DynamicXml.
        /// </summary>
        /// <param name="stream">Source Xml string stream.</param>
        /// <returns>DynamicXml object.</returns>
        public static dynamic Load(Stream stream)
        {
            return new DynamicXml(XElement.Load(stream));
        }

        /// <summary>
        /// Load Xml file to DynamicXml.
        /// </summary>
        /// <param name="xmlFile">Source Xml file.</param>
        /// <returns>DynamicXml object.</returns>
        public static dynamic Load(string xmlFile)
        {
            return new DynamicXml(XElement.Load(xmlFile));
        }

        /// <summary>
        /// Load Xml from an object to DynamicXml.
        /// </summary>
        /// <param name="source">Source object.</param>
        /// <param name="encoding">The encoding to apply to the string.</param>
        /// <param name="extraTypes">A <see cref="T:System.Type" /> array of additional object types to serialize.</param>
        /// <returns>DynamicXml object.</returns>
        public static dynamic LoadFrom(object source, Encoding encoding = null, Type[] extraTypes = null)
        {
            XmlSerializer xmlSerializer = (extraTypes == null || extraTypes.Length == 0) ? new XmlSerializer(source.GetType()) : new XmlSerializer(source.GetType(), extraTypes);

            using (MemoryStream memoryStream = new MemoryStream())
            {
                xmlSerializer.Serialize(memoryStream, source);

                return Load(memoryStream);
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An System.Collections.IEnumerator object that can be used to iterate through the collection.</returns>
        public IEnumerator<KeyValuePair<string, DynamicXml>> GetEnumerator()
        {
            return this._xElement.Elements().ToDictionary(k => k.Name.LocalName, v => new DynamicXml(v)).GetEnumerator();
        }

        /// <summary>
        /// Gets a value indicating whether the current DynamicXml has specified element name.
        /// </summary>
        /// <param name="name">Element name.</param>
        /// <returns>true if the current DynamicXml has specified element name; otherwise, false.</returns>
        public bool HasElement(string name)
        {
            return this._xElement.Element(name) != null;
        }

        /// <summary>
        /// Gets a value indicating whether the current DynamicXml has specified element index.
        /// </summary>
        /// <param name="index">Index of the element.</param>
        /// <returns>true if the current DynamicXml has specified element index; otherwise, false.</returns>
        public bool HasElement(int index)
        {
            return this._xElement.Elements().ElementAtOrDefault(index) != null;
        }

        /// <summary>
        /// Gets a value indicating whether the current DynamicXml has specified attribute name.
        /// </summary>
        /// <param name="name">Attribute name.</param>
        /// <returns>true if the current DynamicXml has specified attribute name; otherwise, false.</returns>
        public bool HasAttribute(string name)
        {
            return this._xElement.Attribute(name) != null;
        }

        /// <summary>
        /// Gets a value indicating whether the current DynamicXml has specified attribute index.
        /// </summary>
        /// <param name="index">Index of the attribute.</param>
        /// <returns>true if the current DynamicXml has specified attribute index; otherwise, false.</returns>
        public bool HasAttribute(int index)
        {
            return this._xElement.Attributes().ElementAtOrDefault(index) != null;
        }

        /// <summary>
        /// Delete element by name.
        /// </summary>
        /// <param name="name">Element name.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public bool RemoveElement(string name)
        {
            var element = this._xElement.Element(name);

            if (element != null)
            {
                element.Remove();

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Delete element by index.
        /// </summary>
        /// <param name="index">Index of the element.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public bool RemoveElement(int index)
        {
            var element = this._xElement.Elements().ElementAtOrDefault(index);

            if (element != null)
            {
                element.Remove();

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Delete attribute by name.
        /// </summary>
        /// <param name="name">Attribute name.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public bool RemoveAttribute(string name)
        {
            var attribute = this._xElement.Attribute(name);

            if (attribute != null)
            {
                attribute.Remove();

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Delete attribute by index.
        /// </summary>
        /// <param name="index">Index of the attribute.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public bool RemoveAttribute(int index)
        {
            var attribute = this._xElement.Attributes().ElementAtOrDefault(index);

            if (attribute != null)
            {
                attribute.Remove();

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A Xml string that represents the current object.</returns>
        public override string ToString()
        {
            return this._xElement.ToString();
        }

        /// <summary>
        /// Returns the enumeration of all dynamic member names.
        /// </summary>
        /// <returns>A sequence that contains dynamic member names.</returns>
        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return this._xElement.Elements().Select(i => i.Name.LocalName);
        }

        /// <summary>
        /// Provides implementation for type conversion operations.
        /// </summary>
        /// <param name="binder">Provides information about the conversion operation.</param>
        /// <param name="result">The result of the type conversion operation.</param>
        /// <returns>true if the operation is successful; otherwise, false.</returns>
        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            if (this._xElement != null && this._xAttribute == null)
            {
                if (binder.ReturnType == typeof(XElement))
                {
                    result = this._xElement;

                    return true;
                }
                else if (binder.ReturnType == typeof(IEnumerable))
                {
                    result = this._xElement.Elements().Select(i => new DynamicXml(i)).ToList();

                    return true;
                }
                else if (this.TryXmlConvert(this._xElement, binder.ReturnType, out result))
                {
                    return true;
                }
            }
            else if (this._xElement == null && this._xAttribute != null)
            {
                if (binder.ReturnType == typeof(XAttribute))
                {
                    result = this._xAttribute;

                    return true;
                }
                else if (binder.ReturnType == typeof(IEnumerable))
                {
                    result = this._xElement.Attributes().Select(i => new DynamicXml(i)).ToList();

                    return true;
                }
                else if (this.TryXmlConvert(this._xAttribute.Value, binder.ReturnType, out result))
                {
                    return true;
                }
            }

            return base.TryConvert(binder, out result);
        }

        /// <summary>
        /// Provides the implementation for operations that get a value by index.
        /// </summary>
        /// <param name="binder">Provides information about the operation.</param>
        /// <param name="indexes">The indexes that are used in the operation.</param>
        /// <param name="result">The result of the index operation.</param>
        /// <returns>true if the operation is successful; otherwise, false.</returns>
        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            if (indexes[0] is string)
            {
                XAttribute attribute = this._xElement.Attribute((string)indexes[0]);

                if (attribute != null)
                {
                    result = new DynamicXml(attribute);

                    return true;
                }
            }
            else if (indexes[0] is int)
            {
                XElement element = this._xElement.Elements().ElementAtOrDefault((int)indexes[0]);

                if (element != null)
                {
                    result = new DynamicXml(element);

                    return true;
                }
            }

            result = null;

            return false;
        }

        /// <summary>
        /// Provides the implementation for operations that get member values.
        /// </summary>
        /// <param name="binder">Provides information about the object that called the dynamic operation.</param>
        /// <param name="result">The result of the get operation.</param>
        /// <returns>true if the operation is successful; otherwise, false.</returns>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var elements = this._xElement.Elements(binder.Name);

            if (elements.Count() > 1)
            {
                result = elements.Select(i => new DynamicXml(i));

                return true;
            }
            else if (elements.Count() == 1)
            {
                result = new DynamicXml(elements.FirstOrDefault());

                return true;
            }

            result = null;

            return false;
        }

        /// <summary>
        /// Provides the implementation for operations that set a value by index.
        /// </summary>
        /// <param name="binder">Provides information about the operation.</param>
        /// <param name="indexes">The indexes that are used in the operation.</param>
        /// <param name="value">The value to set to the object that has the specified index.</param>
        /// <returns>true if the operation is successful; otherwise, false.</returns>
        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            if (indexes[0] is string)
            {
                XAttribute attribute = this._xElement.Attribute((string)indexes[0]);

                if (attribute != null)
                {
                    attribute.Value = XmlConvert.ToString((dynamic)value);
                }
                else
                {
                    this._xElement.Add(new XAttribute((string)indexes[0], XmlConvert.ToString((dynamic)value)));
                }

                return true;
            }
            else if (indexes[0] is int)
            {
                XElement element = this._xElement.Elements().ElementAtOrDefault((int)indexes[0]);

                if (element != null)
                {
                    element.ReplaceNodes(this.CreateXContent(value));
                }
                else
                {
                    this._xElement.Add(new XElement(value.GetType().Name, this.CreateXContent(value)));
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Provides the implementation for operations that set member values.
        /// </summary>
        /// <param name="binder">Provides information about the object that called the dynamic operation.</param>
        /// <param name="value">The value to set to the member.</param>
        /// <returns>true if the operation is successful; otherwise, false.</returns>
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            XElement element = this._xElement.Element(binder.Name);

            if (element != null)
            {
                element.ReplaceNodes(this.CreateXContent(value));
            }
            else
            {
                this._xElement.Add(new XElement(binder.Name, this.CreateXContent(value)));
            }

            return true;
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An System.Collections.IEnumerator object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this._xElement.Elements().ToDictionary(k => k.Name.LocalName, v => new DynamicXml(v)).GetEnumerator();
        }

        /// <summary>
        /// Method CreateXContent.
        /// </summary>
        /// <param name="obj">Source object.</param>
        /// <returns>XElement content object.</returns>
        private object CreateXContent(object obj)
        {
            if (obj == null)
            {
                return null;
            }

            switch (Type.GetTypeCode(obj.GetType()))
            {
                case TypeCode.Object:
                    return (obj is IEnumerable) ? this.CreateXArray(obj as IEnumerable) : this.CreateXObject(obj);
                case TypeCode.DBNull:
                case TypeCode.Empty:
                    return null;
                default:
                    return obj;
            }
        }

        /// <summary>
        /// Method CreateXObject.
        /// </summary>
        /// <param name="obj">Source object.</param>
        /// <returns>Instance of IEnumerable{XStreamingElement}.</returns>
        private IEnumerable<XStreamingElement> CreateXObject(object obj)
        {
            return obj
                .GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(i => new { Name = i.Name, Value = i.GetValue(obj, null) })
                .Select(i => new XStreamingElement(i.Name, this.CreateXContent(i.Value)));
        }

        /// <summary>
        /// Method CreateXArray.
        /// </summary>
        /// <typeparam name="T">Type of the <paramref name="obj"/>.</typeparam>
        /// <param name="obj">Source object.</param>
        /// <returns>Instance of IEnumerable{XStreamingElement}.</returns>
        private IEnumerable<XStreamingElement> CreateXArray<T>(T obj) where T : IEnumerable
        {
            return obj
                .Cast<object>()
                .Select(i => new XStreamingElement(i.GetType().Name, this.CreateXContent(i)));
        }

        /// <summary>
        /// Method TryXmlConvert.
        /// </summary>
        /// <param name="value">Source value.</param>
        /// <param name="returnType">Target type.</param>
        /// <param name="result">Result object.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        private bool TryXmlConvert(string value, Type returnType, out object result)
        {
            if (returnType == typeof(string))
            {
                result = value;

                return true;
            }
            else if (returnType.IsEnum)
            {
                if (Enum.IsDefined(returnType, value))
                {
                    result = Enum.Parse(returnType, value);

                    return true;
                }

                var enumType = Enum.GetUnderlyingType(returnType);

                var rawValue = XmlConverters[enumType].Invoke(value);

                result = Enum.ToObject(returnType, rawValue);

                return true;
            }
            else
            {
                var converter = default(Func<string, object>);

                if (XmlConverters.TryGetValue(returnType, out converter))
                {
                    result = converter(value);

                    return true;
                }
                else
                {
                    result = null;

                    return false;
                }
            }
        }

        /// <summary>
        /// Method TryXmlConvert.
        /// </summary>
        /// <param name="xElement">Source element.</param>
        /// <param name="returnType">Target type.</param>
        /// <param name="result">Result object.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        private bool TryXmlConvert(XElement xElement, Type returnType, out object result)
        {
            if (returnType == typeof(string))
            {
                result = xElement.Value;

                return true;
            }
            else if (returnType.IsEnum)
            {
                if (Enum.IsDefined(returnType, xElement.Value))
                {
                    result = Enum.Parse(returnType, xElement.Value);

                    return true;
                }

                var enumType = Enum.GetUnderlyingType(returnType);

                var rawValue = XmlConverters[enumType].Invoke(xElement.Value);

                result = Enum.ToObject(returnType, rawValue);

                return true;
            }
            else
            {
                var converter = default(Func<string, object>);

                if (XmlConverters.TryGetValue(returnType, out converter))
                {
                    result = converter(xElement.Value);

                    return true;
                }
                else
                {
                    result = this.Deserialize(xElement, returnType);

                    return true;
                }
            }
        }

        /// <summary>
        /// Method Deserialize.
        /// </summary>
        /// <param name="xElement">Source element.</param>
        /// <param name="targetType">Target Type.</param>
        /// <returns>Instance of object.</returns>
        private object Deserialize(XElement xElement, Type targetType)
        {
            return this.CanEnumerable(targetType) ? this.DeserializeArray(xElement, targetType) : this.DeserializeObject(xElement, targetType);
        }

        /// <summary>
        /// Method DeserializeArray.
        /// </summary>
        /// <param name="xElement">Source element.</param>
        /// <param name="targetType">Target Type.</param>
        /// <returns>Instance of object.</returns>
        private object DeserializeArray(XElement xElement, Type targetType)
        {
            Type elementType = targetType.IsArray ? targetType.GetElementType() : targetType.GetGenericArguments()[0];

            IList list = (IList)Activator.CreateInstance(targetType);

            foreach (var item in this._xElement.Elements())
            {
                object result = null;

                if (this.TryXmlConvert(item, targetType, out result))
                {
                    if (result != null)
                    {
                        list.Add(result);
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// Method DeserializeObject.
        /// </summary>
        /// <param name="xElement">Source element.</param>
        /// <param name="targetType">Target Type.</param>
        /// <returns>Instance of object.</returns>
        private object DeserializeObject(XElement xElement, Type targetType)
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

            var properties = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(i => i.CanWrite).ToDictionary(i => i.Name, i => i);

            foreach (var item in this._xElement.Elements())
            {
                PropertyInfo propertyInfo;

                if (!properties.TryGetValue(item.Name.LocalName, out propertyInfo))
                {
                    continue;
                }

                object itemValue = null;

                if (this.TryXmlConvert(item, propertyInfo.PropertyType, out itemValue))
                {
                    propertyInfo.SetValue(result, itemValue, null);
                }
            }

            return result;
        }

        /// <summary>
        /// Method CanEnumerable.
        /// </summary>
        /// <param name="source">Source Type.</param>
        /// <returns>true if the source Type inherit IEnumerable interface; otherwise, false.</returns>
        private bool CanEnumerable(Type source)
        {
            return !source.Equals(typeof(string)) && source.GetInterface("IEnumerable") != null;
        }
    }
}
