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
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Serialization;

    /// <summary>
    /// Provides a class for specifying dynamic Xml behavior at run time.
    /// </summary>
    public class DynamicXml : DynamicObject, IEnumerable<DynamicXml>, IEnumerable<KeyValuePair<string, DynamicXml>>, IEnumerable
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
        /// Field _xElementList.
        /// </summary>
        private readonly IList<XElement> _xElementList;

        /// <summary>
        /// Initializes static members of the <see cref="DynamicXml" /> class.
        /// </summary>
        static DynamicXml()
        {
            XmlConverters = new Dictionary<Type, Func<string, object>>(17);

            XmlConverters.Add(typeof(bool), s => XmlConvert.ToBoolean(s));
            XmlConverters.Add(typeof(byte), s => XmlConvert.ToByte(s));
            XmlConverters.Add(typeof(char), s => XmlConvert.ToChar(s));
            XmlConverters.Add(typeof(DateTime), s => XmlConvert.ToDateTime(s, XmlDateTimeSerializationMode.RoundtripKind));
            XmlConverters.Add(typeof(DateTimeOffset), s => XmlConvert.ToDateTimeOffset(s));
            XmlConverters.Add(typeof(decimal), s => XmlConvert.ToDecimal(s));
            XmlConverters.Add(typeof(double), s => XmlConvert.ToDouble(s));
            XmlConverters.Add(typeof(Guid), s => XmlConvert.ToGuid(s));
            XmlConverters.Add(typeof(short), s => XmlConvert.ToInt16(s));
            XmlConverters.Add(typeof(int), s => XmlConvert.ToInt32(s));
            XmlConverters.Add(typeof(long), s => XmlConvert.ToInt64(s));
            XmlConverters.Add(typeof(sbyte), s => XmlConvert.ToSByte(s));
            XmlConverters.Add(typeof(float), s => XmlConvert.ToSingle(s));
            XmlConverters.Add(typeof(TimeSpan), s => XmlConvert.ToTimeSpan(s));
            XmlConverters.Add(typeof(ushort), s => XmlConvert.ToUInt16(s));
            XmlConverters.Add(typeof(uint), s => XmlConvert.ToUInt32(s));
            XmlConverters.Add(typeof(ulong), s => XmlConvert.ToUInt64(s));
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
            this._xElementList = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicXml" /> class.
        /// </summary>
        /// <param name="xAttribute">XAttribute instance.</param>
        public DynamicXml(XAttribute xAttribute)
        {
            this._xAttribute = xAttribute;
            this._xElement = null;
            this._xElementList = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicXml" /> class.
        /// </summary>
        /// <param name="xElement">XElement instance.</param>
        /// <param name="xElementList">A list of XElement.</param>
        private DynamicXml(XElement xElement, IList<XElement> xElementList)
        {
            this._xElement = xElement;
            this._xElementList = xElementList;
            this._xAttribute = null;
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
        /// <param name="extraTypes">A <see cref="T:System.Type" /> array of additional object types to serialize.</param>
        /// <returns>DynamicXml object.</returns>
        public static dynamic LoadFrom(object source, Type[] extraTypes = null)
        {
            XmlSerializer xmlSerializer = (extraTypes == null || extraTypes.Length == 0) ? new XmlSerializer(source.GetType()) : new XmlSerializer(source.GetType(), extraTypes);

            using (MemoryStream memoryStream = new MemoryStream())
            {
                xmlSerializer.Serialize(memoryStream, source);
                memoryStream.Position = 0;
                return Load(memoryStream);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current DynamicXml has specified element name.
        /// </summary>
        /// <param name="name">Element name.</param>
        /// <returns>true if the current DynamicXml has specified element name; otherwise, false.</returns>
        public bool HasElement(string name)
        {
            return this._xElement != null && this._xElement.Element(XName.Get(name, this._xElement.GetDefaultNamespace().NamespaceName)) != null;
        }

        /// <summary>
        /// Gets a value indicating whether the current DynamicXml has specified element index.
        /// </summary>
        /// <param name="index">Index of the element.</param>
        /// <returns>true if the current DynamicXml has specified element index; otherwise, false.</returns>
        public bool HasElement(int index)
        {
            return this._xElement != null && this._xElement.Elements().ElementAtOrDefault(index) != null;
        }

        /// <summary>
        /// Gets a value indicating whether the current DynamicXml has specified attribute name.
        /// </summary>
        /// <param name="name">Attribute name.</param>
        /// <returns>true if the current DynamicXml has specified attribute name; otherwise, false.</returns>
        public bool HasAttribute(string name)
        {
            return (this._xAttribute != null && this._xAttribute.Name.LocalName.Equals(name, StringComparison.Ordinal)) ||
                (this._xElement != null && this._xElement.Attribute(name) != null);
        }

        /// <summary>
        /// Gets a value indicating whether the current DynamicXml has specified attribute index.
        /// </summary>
        /// <param name="index">Index of the attribute.</param>
        /// <returns>true if the current DynamicXml has specified attribute index; otherwise, false.</returns>
        public bool HasAttribute(int index)
        {
            return (this._xAttribute != null && index == 0) ||
                (this._xElement != null && this._xElement.Attributes().ElementAtOrDefault(index) != null);
        }

        /// <summary>
        /// Delete element by name.
        /// </summary>
        /// <param name="name">Element name.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public bool RemoveElement(string name)
        {
            if (this._xElement == null)
            {
                return false;
            }

            var element = this._xElement.Element(XName.Get(name, this._xElement.GetDefaultNamespace().NamespaceName));

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
            if (this._xElement == null)
            {
                return false;
            }

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
            if (this._xElement != null)
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
            else
            {
                if (this._xAttribute.Name.LocalName.Equals(name, StringComparison.Ordinal))
                {
                    this._xAttribute.Remove();

                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Delete attribute by index.
        /// </summary>
        /// <param name="index">Index of the attribute.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public bool RemoveAttribute(int index)
        {
            if (this._xElement != null)
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
            else
            {
                if (index == 0)
                {
                    this._xAttribute.Remove();

                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Returns a list of the child elements of this element or document, in document order.
        /// </summary>
        /// <returns>A list of the child elements of this instance.</returns>
        public List<dynamic> Elements()
        {
            if (this._xElement != null)
            {
                return this._xElement.Elements().Select(i => (dynamic)new DynamicXml(i)).ToList();
            }
            else
            {
                return new List<dynamic>();
            }
        }

        /// <summary>
        /// Returns a list of attributes of this element.
        /// </summary>
        /// <returns>A list of attributes of this instance.</returns>
        public List<dynamic> Attributes()
        {
            if (this._xElement != null)
            {
                return this._xElement.Attributes().Select(i => (dynamic)new DynamicXml(i)).ToList();
            }
            else
            {
                return new List<dynamic> { (dynamic)new DynamicXml(this._xAttribute) };
            }
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A Xml string that represents the current object.</returns>
        public override string ToString()
        {
            return this._xElement != null ? this._xElement.ToString() : this._xAttribute.ToString();
        }

        /// <summary>
        /// Returns the enumeration of all dynamic member names.
        /// </summary>
        /// <returns>A sequence that contains dynamic member names.</returns>
        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return this._xElement != null ? this._xElement.Elements().Select(i => i.Name.LocalName) : new List<string> { this._xAttribute.Name.LocalName };
        }

        /// <summary>
        /// Provides implementation for type conversion operations.
        /// </summary>
        /// <param name="binder">Provides information about the conversion operation.</param>
        /// <param name="result">The result of the type conversion operation.</param>
        /// <returns>true if the operation is successful; otherwise, false.</returns>
        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            if (binder.ReturnType == this.GetType())
            {
                result = this;

                return true;
            }

            if (this._xElement != null && this._xAttribute == null)
            {
                if (binder.ReturnType == typeof(XElement))
                {
                    result = this._xElement;

                    return true;
                }
                else if (binder.ReturnType == typeof(IEnumerable))
                {
                    result = this._xElement.Elements().Select(i => (dynamic)new DynamicXml(i)).ToList();

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
                    result = new List<dynamic> { (dynamic)new DynamicXml(this._xAttribute) };

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
            result = (dynamic)this.GetIndexInternal(this, false, indexes);

            return result != null;
        }

        /// <summary>
        /// Provides the implementation for operations that get member values.
        /// </summary>
        /// <param name="binder">Provides information about the object that called the dynamic operation.</param>
        /// <param name="result">The result of the get operation.</param>
        /// <returns>true if the operation is successful; otherwise, false.</returns>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = (dynamic)this.GetMemberInternal(this, binder.Name);

            return result != null;
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
            return this.SetIndexInternal(this, value, indexes);
        }

        /// <summary>
        /// Provides the implementation for operations that set member values.
        /// </summary>
        /// <param name="binder">Provides information about the object that called the dynamic operation.</param>
        /// <param name="value">The value to set to the member.</param>
        /// <returns>true if the operation is successful; otherwise, false.</returns>
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (this._xElement != null)
            {
                XElement element = this._xElement.Element(XName.Get(binder.Name, this._xElement.GetDefaultNamespace().NamespaceName));

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

            return false;
        }

        /// <summary>
        /// Provides the implementation for operations that invoke a member.
        /// </summary>
        /// <param name="binder">Provides information about the dynamic operation.</param>
        /// <param name="args">The arguments that are passed to the object member during the invoke operation.</param>
        /// <param name="result">The result of the member invocation.</param>
        /// <returns>true if the operation is successful; otherwise, false.</returns>
        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            if (args.Length == 0)
            {
                result = this.HasElement(binder.Name);
                return true;
            }
            else if (args.Length == 1)
            {
                DynamicXml innerDynamicXml = this.GetMemberInternal(this, binder.Name);

                if (innerDynamicXml == null)
                {
                    result = false;
                }

                dynamic attrIndex;

                if (args[0] is int)
                {
                    attrIndex = (int)args[0];
                }
                else if (args[0] is string)
                {
                    attrIndex = (string)args[0];

                    int index;
                    bool isNumber = int.TryParse((string)attrIndex, NumberStyles.Number, CultureInfo.InvariantCulture, out index);

                    if (isNumber)
                    {
                        attrIndex = index;
                    }
                }
                else
                {
                    result = false;
                    return false;
                }

                result = innerDynamicXml.HasAttribute(attrIndex);
                return true;
            }

            result = false;
            return false;
        }

        /// <summary>
        /// Provides the implementation for operations that invoke an object.
        /// </summary>
        /// <param name="binder">Provides information about the invoke operation.</param>
        /// <param name="args">The arguments that are passed to the object during the invoke operation.</param>
        /// <param name="result">The result of the object invocation.</param>
        /// <returns>true if the operation is successful; otherwise, false.</returns>
        public override bool TryInvoke(InvokeBinder binder, object[] args, out object result)
        {
            if (args.Length == 0)
            {
                if (this._xElement != null)
                {
                    result = this._xElement.Name.LocalName;
                }
                else
                {
                    result = this._xAttribute.Name.LocalName;
                }

                return true;
            }
            else if (args.Length == 1)
            {
                dynamic attrIndex;

                if (args[0] is int)
                {
                    attrIndex = (int)args[0];
                }
                else if (args[0] is string)
                {
                    attrIndex = (string)args[0];

                    int index;
                    bool isNumber = int.TryParse((string)attrIndex, NumberStyles.Number, CultureInfo.InvariantCulture, out index);

                    if (isNumber)
                    {
                        attrIndex = index;
                    }
                }
                else
                {
                    result = false;
                    return false;
                }

                result = this.HasAttribute(attrIndex);
                return true;
            }

            result = false;
            return false;
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An System.Collections.IEnumerator object that can be used to iterate through the collection.</returns>
        public IEnumerator GetEnumerator()
        {
            if (this._xElement != null)
            {
                return this._xElement.Elements().Select(i => new KeyValuePair<string, dynamic>(i.Name.LocalName, (dynamic)new DynamicXml(i))).GetEnumerator();
            }
            else
            {
                return new List<KeyValuePair<string, dynamic>> { new KeyValuePair<string, dynamic>(this._xAttribute.Name.LocalName, (dynamic)new DynamicXml(this._xAttribute)) }.GetEnumerator();
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An System.Collections.IEnumerator{DynamicXml} object that can be used to iterate through the collection.</returns>
        IEnumerator<DynamicXml> IEnumerable<DynamicXml>.GetEnumerator()
        {
            if (this._xElement != null)
            {
                return this._xElement.Elements().Select(i => new DynamicXml(i)).GetEnumerator();
            }
            else
            {
                return new List<DynamicXml> { new DynamicXml(this._xAttribute) }.GetEnumerator();
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An System.Collections.IEnumerator{KeyValuePair{string, DynamicXml}} object that can be used to iterate through the collection.</returns>
        IEnumerator<KeyValuePair<string, DynamicXml>> IEnumerable<KeyValuePair<string, DynamicXml>>.GetEnumerator()
        {
            if (this._xElement != null)
            {
                return this._xElement.Elements().Select(i => new KeyValuePair<string, DynamicXml>(i.Name.LocalName, new DynamicXml(i))).GetEnumerator();
            }
            else
            {
                return new List<KeyValuePair<string, DynamicXml>> { new KeyValuePair<string, DynamicXml>(this._xAttribute.Name.LocalName, new DynamicXml(this._xAttribute)) }.GetEnumerator();
            }
        }

        /// <summary>
        /// Provides the implementation for operations that get member values.
        /// </summary>
        /// <param name="source">Source object.</param>
        /// <param name="name">Name of member to get.</param>
        /// <returns>The result of the get operation.</returns>
        private DynamicXml GetMemberInternal(DynamicXml source, string name)
        {
            if (source._xElement != null)
            {
                var elements = source._xElement.Elements(XName.Get(name, source._xElement.GetDefaultNamespace().NamespaceName));

                if (elements.Count() > 1)
                {
                    return new DynamicXml(source._xElement, elements.ToList());
                }
                else if (elements.Count() == 1)
                {
                    return new DynamicXml(elements.FirstOrDefault());
                }
            }

            return null;
        }

        /// <summary>
        /// Provides the implementation for operations that get a value by index.
        /// </summary>
        /// <param name="source">Source object.</param>
        /// <param name="getInner">true to get inner level elements; false to get current level elements.</param>
        /// <param name="indexes">The indexes that are used in the operation.</param>
        /// <returns>The result of the index operation.</returns>
        private DynamicXml GetIndexInternal(DynamicXml source, bool getInner, params object[] indexes)
        {
            if (source == null)
            {
                return null;
            }

            if (indexes[0] is string)
            {
                string attrName = (string)indexes[0];
                int attrIndex = 0;
                bool isNumber = int.TryParse(attrName, NumberStyles.Number, CultureInfo.InvariantCulture, out attrIndex);

                if (source._xElement != null)
                {
                    XAttribute attribute = null;

                    if (isNumber)
                    {
                        attribute = source._xElement.Attributes().ElementAtOrDefault(attrIndex);
                    }
                    else
                    {
                        attribute = source._xElement.Attribute(attrName);
                    }

                    if (attribute != null)
                    {
                        return new DynamicXml(attribute);
                    }
                }
                else
                {
                    if ((isNumber && attrIndex == 0) || (!isNumber && source._xAttribute.Name.LocalName.Equals(attrName, StringComparison.Ordinal)))
                    {
                        return new DynamicXml(this._xAttribute);
                    }
                }
            }
            else if (indexes[0] is int)
            {
                int firstIndex = (int)indexes[0];

                XElement element = null;

                if (source._xElementList != null)
                {
                    element = source._xElementList.ElementAtOrDefault(firstIndex);
                }
                else if (source._xElement != null)
                {
                    if (getInner)
                    {
                        element = source._xElement.Elements().ElementAtOrDefault(firstIndex);
                    }
                    else
                    {
                        if (firstIndex == 0)
                        {
                            element = source._xElement;
                        }
                    }
                }

                if (element == null)
                {
                    return null;
                }

                if (indexes.Length == 1)
                {
                    return new DynamicXml(element);
                }
                else if (indexes.Length == 2)
                {
                    return this.GetIndexInternal(new DynamicXml(element), true, indexes[1]);
                }
            }

            return null;
        }

        /// <summary>
        /// Provides the implementation for operations that set a value by index.
        /// </summary>
        /// <param name="source">XObject to set.</param>
        /// <param name="value">The value to set to the object that has the specified index.</param>
        /// <param name="indexes">The indexes that are used in the operation.</param>
        /// <returns>true if the operation is successful; otherwise, false.</returns>
        private bool SetIndexInternal(DynamicXml source, object value, params object[] indexes)
        {
            if (source == null)
            {
                return false;
            }

            if (indexes[0] is string)
            {
                string attrName = (string)indexes[0];
                int attrIndex = 0;
                bool isNumber = int.TryParse(attrName, NumberStyles.Number, CultureInfo.InvariantCulture, out attrIndex);

                if (source._xElement != null)
                {
                    XAttribute attribute = null;

                    if (isNumber)
                    {
                        attribute = source._xElement.Attributes().ElementAtOrDefault(attrIndex);
                    }
                    else
                    {
                        attribute = source._xElement.Attribute(attrName);
                    }

                    if (attribute != null)
                    {
                        attribute.Value = XmlConvert.ToString((dynamic)value);
                    }
                    else
                    {
                        source._xElement.Add(new XAttribute(isNumber ? "attribute" + attrName : attrName, XmlConvert.ToString((dynamic)value)));
                    }

                    return true;
                }
                else
                {
                    if ((isNumber && attrIndex == 0) || (!isNumber && source._xAttribute.Name.LocalName.Equals(attrName, StringComparison.Ordinal)))
                    {
                        source._xAttribute.Value = XmlConvert.ToString((dynamic)value);

                        return true;
                    }
                }
            }
            else if (indexes[0] is int)
            {
                int firstIndex = (int)indexes[0];

                if (source._xElementList != null)
                {
                    XElement element = source._xElementList.ElementAtOrDefault(firstIndex);

                    if (indexes.Length == 1)
                    {
                        if (element != null)
                        {
                            element.ReplaceNodes(this.CreateXContent(value));
                        }
                        else
                        {
                            element = new XElement(value.GetType().Name, this.CreateXContent(value));
                            source._xElement.Add(element);
                            source._xElementList.Add(element);
                        }

                        return true;
                    }
                    else if (indexes.Length == 2)
                    {
                        if (element == null)
                        {
                            element = new XElement("ArrayOf" + value.GetType().Name);
                            source._xElement.Add(element);
                            source._xElementList.Add(element);
                        }

                        return this.SetIndexInternal(new DynamicXml(element), value, indexes[1]);
                    }
                }
                else if (source._xElement != null)
                {
                    XElement element = source._xElement.Elements().ElementAtOrDefault(firstIndex);

                    if (indexes.Length == 1)
                    {
                        if (element != null)
                        {
                            element.ReplaceNodes(this.CreateXContent(value));
                        }
                        else
                        {
                            source._xElement.Add(new XElement(value.GetType().Name, this.CreateXContent(value)));
                        }

                        return true;
                    }
                    else if (indexes.Length == 2)
                    {
                        if (element == null)
                        {
                            element = new XElement("ArrayOf" + value.GetType().Name);
                            source._xElement.Add(element);
                        }

                        return this.SetIndexInternal(new DynamicXml(element), value, indexes[1]);
                    }
                }
                else if (source._xAttribute != null)
                {
                    if (firstIndex == 0)
                    {
                        source._xAttribute.Value = XmlConvert.ToString((dynamic)value);

                        return true;
                    }
                }
            }

            return false;
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
            else if (returnType == this.GetType())
            {
                result = new DynamicXml(xElement);

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
            else if (returnType == this.GetType())
            {
                result = new DynamicXml(new XElement(value, this.CreateXContent(value)));

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
        /// Method Deserialize.
        /// </summary>
        /// <param name="xElement">Source element.</param>
        /// <param name="targetType">Target Type.</param>
        /// <returns>Instance of object.</returns>
        private object Deserialize(XElement xElement, Type targetType)
        {
            return this.IsEnumerable(targetType) ? this.DeserializeEnumerable(xElement, targetType) : this.DeserializeObject(xElement, targetType);
        }

        /// <summary>
        /// Method DeserializeEnumerable.
        /// </summary>
        /// <param name="xElement">Source element.</param>
        /// <param name="targetType">Target Type.</param>
        /// <returns>Instance of object.</returns>
        private object DeserializeEnumerable(XElement xElement, Type targetType)
        {
            if (this.IsDictionary(targetType))
            {
                IDictionary result = (IDictionary)Activator.CreateInstance(targetType, true);

                Type keyType = targetType.GetGenericArguments()[0];
                Type valueType = targetType.GetGenericArguments()[1];

                foreach (var item in xElement.Elements())
                {
                    dynamic key = null;

                    if (this.TryXmlConvert(item.Name.LocalName, keyType, out key))
                    {
                        if (key != null)
                        {
                            dynamic value = null;

                            if (this.TryXmlConvert(item, valueType, out value))
                            {
                                if (value != null)
                                {
                                    result[key] = value;
                                }
                            }
                        }
                    }
                }

                return result;
            }
            else
            {
                Type elementType = targetType.IsArray ? targetType.GetElementType() : targetType.GetGenericArguments()[0];

                if (targetType.IsArray)
                {
                    IList list = new List<dynamic>();

                    foreach (var item in xElement.Elements())
                    {
                        object element = null;

                        if (this.TryXmlConvert(item, elementType, out element))
                        {
                            if (element != null)
                            {
                                list.Add(element);
                            }
                        }
                    }

                    Array result = Array.CreateInstance(elementType, list.Count);
                    list.CopyTo(result, 0);

                    return result;
                }
                else
                {
                    IList result = (IList)Activator.CreateInstance(targetType);

                    foreach (var item in xElement.Elements())
                    {
                        dynamic element = null;

                        if (this.TryXmlConvert(item, elementType, out element))
                        {
                            if (element != null)
                            {
                                result.Add(element);
                            }
                        }
                    }

                    return result;
                }
            }
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

            var properties = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(i => i.CanWrite);

            foreach (var item in properties)
            {
                var elements = xElement.Elements(XName.Get(item.Name, this._xElement.GetDefaultNamespace().NamespaceName));

                if (elements.Count() >= 1)
                {
                    object itemValue = null;

                    if (this.TryXmlConvert(elements.First(), item.PropertyType, out itemValue))
                    {
                        item.SetValue(result, itemValue, null);

                        continue;
                    }
                }

                var attributes = xElement.Attributes(item.Name).ToArray();

                if (attributes.Length >= 1)
                {
                    object itemValue = null;

                    if (this.TryXmlConvert(attributes[0].Value, item.PropertyType, out itemValue))
                    {
                        item.SetValue(result, itemValue, null);

                        continue;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Method IsEnumerable.
        /// </summary>
        /// <param name="source">Source Type.</param>
        /// <returns>true if the source Type inherit IEnumerable interface; otherwise, false.</returns>
        private bool IsEnumerable(Type source)
        {
            return source != this.GetType() && source != typeof(string) && source.GetInterface("IEnumerable") != null;
        }

        /// <summary>
        /// Method IsDictionary.
        /// </summary>
        /// <param name="source">Source Type.</param>
        /// <returns>true if the source Type inherit IDictionary interface; otherwise, false.</returns>
        private bool IsDictionary(Type source)
        {
            return source.GetInterface("IDictionary") != null;
        }
    }
}
