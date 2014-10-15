//-----------------------------------------------------------------------
// <copyright file="DynamicJson.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Dynamic
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Dynamic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Json;
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;

    /// <summary>
    /// Provides a class for specifying dynamic Json behavior at run time.
    /// </summary>
    public class DynamicJson : DynamicObject, IEnumerable<DynamicJson>, IEnumerable<KeyValuePair<string, DynamicJson>>, IEnumerable
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
        /// Field _jsonType.
        /// </summary>
        private JsonType _jsonType;

        /// <summary>
        /// Initializes static members of the <see cref="DynamicJson" /> class.
        /// </summary>
        static DynamicJson()
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
        /// Initializes a new instance of the <see cref="DynamicJson" /> class.
        /// </summary>
        public DynamicJson()
        {
            this._xElement = new XElement("root", this.CreateTypeAttribute(JsonType.@object));
            this._jsonType = JsonType.@object;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicJson" /> class.
        /// </summary>
        /// <param name="xElement">XElement instance.</param>
        /// <param name="jsonType">Json type.</param>
        private DynamicJson(XElement xElement, JsonType jsonType)
        {
            this._xElement = xElement;
            this._jsonType = jsonType;
        }

        /// <summary>
        /// Enum JsonType.
        /// </summary>
        private enum JsonType
        {
            /// <summary>
            /// Represents string type.
            /// </summary>
            @string,

            /// <summary>
            /// Represents number type.
            /// </summary>
            @number,

            /// <summary>
            /// Represents bool type.
            /// </summary>
            @boolean,

            /// <summary>
            /// Represents complex object.
            /// </summary>
            @object,

            /// <summary>
            /// Represents array.
            /// </summary>
            @array,

            /// <summary>
            /// Represents null.
            /// </summary>
            @null
        }

        /// <summary>
        /// Gets a value indicating whether this DynamicJson is array or not.
        /// </summary>
        public bool IsArray
        {
            get
            {
                return this._jsonType == JsonType.@array;
            }
        }

        /// <summary>
        /// Parse Json string to DynamicJson.
        /// </summary>
        /// <param name="jsonString">Source Json string.</param>
        /// <param name="encoding">The encoding to apply to the string.</param>
        /// <returns>DynamicJson object.</returns>
        public static dynamic Parse(string jsonString, Encoding encoding = null)
        {
            using (var reader = JsonReaderWriterFactory.CreateJsonReader((encoding ?? Encoding.Unicode).GetBytes(jsonString), XmlDictionaryReaderQuotas.Max))
            {
                return CreateDynamicJson(XElement.Load(reader));
            }
        }

        /// <summary>
        /// Parse Json string stream to DynamicJson.
        /// </summary>
        /// <param name="stream">Source Json string stream.</param>
        /// <returns>DynamicJson object.</returns>
        public static dynamic Load(Stream stream)
        {
            using (var reader = JsonReaderWriterFactory.CreateJsonReader(stream, XmlDictionaryReaderQuotas.Max))
            {
                return CreateDynamicJson(XElement.Load(reader));
            }
        }

        /// <summary>
        /// Load Json file to DynamicJson.
        /// </summary>
        /// <param name="jsonFile">Source Json file.</param>
        /// <returns>DynamicJson object.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public static dynamic Load(string jsonFile)
        {
            using (var reader = JsonReaderWriterFactory.CreateJsonReader(File.OpenRead(jsonFile), XmlDictionaryReaderQuotas.Max))
            {
                return CreateDynamicJson(XElement.Load(reader));
            }
        }

        /// <summary>
        /// Load Json from an object to DynamicJson.
        /// </summary>
        /// <param name="source">Source object.</param>
        /// <param name="knownTypes">A <see cref="T:System.Type" /> array that may be present in the object graph.</param>
        /// <returns>DynamicJson object.</returns>
        public static dynamic LoadFrom(object source, Type[] knownTypes = null)
        {
            DataContractJsonSerializer dataContractJsonSerializer = (knownTypes == null || knownTypes.Length == 0) ? new DataContractJsonSerializer(source.GetType()) : new DataContractJsonSerializer(source.GetType(), knownTypes);

            using (MemoryStream memoryStream = new MemoryStream())
            {
                dataContractJsonSerializer.WriteObject(memoryStream, source);
                memoryStream.Position = 0;
                return Load(memoryStream);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current DynamicJson has specified property name.
        /// </summary>
        /// <param name="name">Property name.</param>
        /// <returns>true if the current DynamicJson has specified property name; otherwise, false.</returns>
        public bool Has(string name)
        {
            return !this.IsArray && this._xElement.Element(name) != null;
        }

        /// <summary>
        /// Gets a value indicating whether the current DynamicJson has specified property index.
        /// </summary>
        /// <param name="index">Index of the property.</param>
        /// <returns>true if the current DynamicJson has specified property index; otherwise, false.</returns>
        public bool Has(int index)
        {
            return this._xElement.Elements().ElementAtOrDefault(index) != null;
        }

        /// <summary>
        /// Delete property by name.
        /// </summary>
        /// <param name="name">Property name.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public bool Remove(string name)
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
        /// Delete property by index.
        /// </summary>
        /// <param name="index">Index of the property.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public bool Remove(int index)
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
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A Json string that represents the current object.</returns>
        public override string ToString()
        {
            this._xElement.Descendants().Where(x => x.Attribute("type").Value == "null").Remove();

            return this.CreateJsonString(new XStreamingElement("root", this.CreateTypeAttribute(this._jsonType), this._xElement.Elements()));
        }

        /// <summary>
        /// Returns the enumeration of all dynamic member names.
        /// </summary>
        /// <returns>A sequence that contains dynamic member names.</returns>
        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return this.IsArray ? this._xElement.Elements().Select((x, i) => i.ToString()) : this._xElement.Elements().Select(i => i.Name.LocalName);
        }

        /// <summary>
        /// Provides implementation for type conversion operations.
        /// </summary>
        /// <param name="binder">Provides information about the conversion operation.</param>
        /// <param name="result">The result of the type conversion operation.</param>
        /// <returns>true if the operation is successful; otherwise, false.</returns>
        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            if (binder.ReturnType == typeof(XElement))
            {
                result = this._xElement;

                return true;
            }
            else if (binder.ReturnType == this.GetType())
            {
                result = this;

                return true;
            }
            else if (binder.ReturnType == typeof(IEnumerable))
            {
                if (this.IsArray)
                {
                    result = this._xElement.Elements().Select(i => (dynamic)CreateDynamicJson(i)).ToList();
                }
                else
                {
                    result = this._xElement.Elements().Select(i => new KeyValuePair<string, dynamic>(i.Name.LocalName, CreateDynamicJson(i))).ToList();
                }

                return true;
            }
            else if (this.TryXmlConvert(this._xElement, binder.ReturnType, out result))
            {
                return true;
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
            if (this.IsArray && indexes[0] is string)
            {
                result = null;

                return false;
            }

            XElement element = null;

            object indexer = indexes[0];

            if (indexer is string)
            {
                element = this._xElement.Element((string)indexer);
            }
            else if (indexer is int)
            {
                element = this._xElement.Elements().ElementAtOrDefault((int)indexer);
            }

            if (element != null)
            {
                result = (dynamic)CreateDynamicJson(element);

                return true;
            }
            else
            {
                result = null;

                return false;
            }
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
            if (this.IsArray && indexes[0] is string)
            {
                return false;
            }

            XElement element = null;

            var jsonType = this.GetJsonType(value);

            object indexer = indexes[0];

            bool isIndexerString = false;

            if (indexer is string)
            {
                isIndexerString = true;

                element = this._xElement.Element((string)indexer);
            }
            else if (indexer is int)
            {
                isIndexerString = false;

                element = this._xElement.Elements().ElementAtOrDefault((int)indexer);
            }
            else
            {
                return false;
            }

            if (element == null)
            {
                if (this.IsArray)
                {
                    this._xElement.Add(new XElement("item", this.CreateTypeAttribute(jsonType), this.CreateXContent(value)));
                }
                else if (isIndexerString)
                {
                    this._jsonType = JsonType.@object;

                    this._xElement.Attribute("type").Value = JsonType.@object.ToString();

                    this._xElement.Add(new XElement((string)indexer, this.CreateTypeAttribute(jsonType), this.CreateXContent(value)));
                }
                else
                {
                    return false;
                }
            }
            else
            {
                element.Attribute("type").Value = jsonType.ToString();

                element.ReplaceNodes(this.CreateXContent(value));
            }

            return true;
        }

        /// <summary>
        /// Provides the implementation for operations that set member values.
        /// </summary>
        /// <param name="binder">Provides information about the object that called the dynamic operation.</param>
        /// <param name="value">The value to set to the member.</param>
        /// <returns>true if the operation is successful; otherwise, false.</returns>
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (this.IsArray)
            {
                return false;
            }

            var jsonType = this.GetJsonType(value);

            var element = this._xElement.Element(binder.Name);

            if (element == null)
            {
                this._jsonType = JsonType.@object;

                this._xElement.Attribute("type").Value = JsonType.@object.ToString();

                this._xElement.Add(new XElement(binder.Name, this.CreateTypeAttribute(jsonType), this.CreateXContent(value)));
            }
            else
            {
                element.Attribute("type").Value = jsonType.ToString();

                element.ReplaceNodes(this.CreateXContent(value));
            }

            return true;
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
                result = this.Has(binder.Name);
                return true;
            }
            else if (args.Length == 1)
            {
                DynamicJson innerDynamicJson = this.GetMemberInternal(this, binder.Name);

                if (innerDynamicJson == null)
                {
                    result = false;
                }

                dynamic memberIndex;

                if (args[0] is int)
                {
                    memberIndex = (int)args[0];
                }
                else if (args[0] is string)
                {
                    memberIndex = (string)args[0];

                    int index;
                    bool isNumber = int.TryParse((string)memberIndex, NumberStyles.Number, CultureInfo.InvariantCulture, out index);

                    if (isNumber)
                    {
                        memberIndex = index;
                    }
                }
                else
                {
                    result = false;
                    return false;
                }

                result = innerDynamicJson.Has(memberIndex);
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
                result = this._xElement.Name.LocalName;
                return true;
            }
            else if (args.Length == 1)
            {
                dynamic memberIndex;

                if (args[0] is int)
                {
                    memberIndex = (int)args[0];
                }
                else if (args[0] is string)
                {
                    memberIndex = (string)args[0];

                    int index;
                    bool isNumber = int.TryParse((string)memberIndex, NumberStyles.Number, CultureInfo.InvariantCulture, out index);

                    if (isNumber)
                    {
                        memberIndex = index;
                    }
                }
                else
                {
                    result = false;
                    return false;
                }

                result = this.Has(memberIndex);
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
            if (this.IsArray)
            {
                return this._xElement.Elements().Select(i => (dynamic)CreateDynamicJson(i)).GetEnumerator();
            }
            else
            {
                return this._xElement.Elements().Select(i => new KeyValuePair<string, dynamic>(i.Name.LocalName, (dynamic)CreateDynamicJson(i))).GetEnumerator();
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An System.Collections.IEnumerator{DynamicJson} object that can be used to iterate through the collection.</returns>
        IEnumerator<DynamicJson> IEnumerable<DynamicJson>.GetEnumerator()
        {
            return this._xElement.Elements().Select(i => CreateDynamicJson(i)).GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An System.Collections.IEnumerator{KeyValuePair{string, DynamicJson}} object that can be used to iterate through the collection.</returns>
        IEnumerator<KeyValuePair<string, DynamicJson>> IEnumerable<KeyValuePair<string, DynamicJson>>.GetEnumerator()
        {
            return this._xElement.Elements().Select(i => new KeyValuePair<string, DynamicJson>(i.Name.LocalName, CreateDynamicJson(i))).GetEnumerator();
        }

        /// <summary>
        /// Method CreateDynamicJson.
        /// </summary>
        /// <param name="xElement">Source XElement.</param>
        /// <returns>DynamicJson object.</returns>
        private static DynamicJson CreateDynamicJson(XElement xElement)
        {
            JsonType type = (JsonType)Enum.Parse(typeof(JsonType), xElement.Attribute("type").Value);

            return new DynamicJson(xElement, type);
        }

        /// <summary>
        /// Provides the implementation for operations that get member values.
        /// </summary>
        /// <param name="source">Source object.</param>
        /// <param name="name">Name of member to get.</param>
        /// <returns>The result of the get operation..</returns>
        private DynamicJson GetMemberInternal(DynamicJson source, string name)
        {
            if (source.IsArray)
            {
                return null;
            }

            var element = source._xElement.Element(name);

            if (element != null)
            {
                return CreateDynamicJson(element);
            }

            return null;
        }

        /// <summary>
        /// Method CreateTypeAttribute.
        /// </summary>
        /// <param name="jsonType">Target JsonType.</param>
        /// <returns>Instance of XAttribute.</returns>
        private XAttribute CreateTypeAttribute(JsonType jsonType)
        {
            return new XAttribute("type", jsonType.ToString());
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
                .Select(i => new XStreamingElement(i.Name, this.CreateTypeAttribute(this.GetJsonType(i.Value)), this.CreateXContent(i.Value)));
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
                .Select(i => new XStreamingElement("item", this.CreateTypeAttribute(this.GetJsonType(i)), this.CreateXContent(i)));
        }

        /// <summary>
        /// Method GetJsonType.
        /// </summary>
        /// <param name="obj">Source object.</param>
        /// <returns>Json type.</returns>
        private JsonType GetJsonType(object obj)
        {
            if (obj == null)
            {
                return JsonType.@null;
            }

            switch (Type.GetTypeCode(obj.GetType()))
            {
                case TypeCode.Boolean:
                    return JsonType.@boolean;
                case TypeCode.String:
                case TypeCode.Char:
                case TypeCode.DateTime:
                    return JsonType.@string;
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                case TypeCode.SByte:
                case TypeCode.Byte:
                    return JsonType.@number;
                case TypeCode.Object:
                    return (obj is IEnumerable) ? JsonType.@array : JsonType.@object;
                case TypeCode.DBNull:
                case TypeCode.Empty:
                default:
                    return JsonType.@null;
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
            else if (returnType == this.GetType())
            {
                result = CreateDynamicJson(xElement);

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
                result = new DynamicJson(new XElement(value, this.CreateTypeAttribute(JsonType.@string), this.CreateXContent(value)), JsonType.@string);

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
                        dynamic element = null;

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
                var elements = xElement.Elements(item.Name).ToArray();

                if (elements.Length >= 1)
                {
                    object itemValue = null;

                    if (this.TryXmlConvert(elements[0], item.PropertyType, out itemValue))
                    {
                        item.SetValue(result, itemValue, null);

                        continue;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Method CreateJsonString.
        /// </summary>
        /// <param name="element">Source XStreamingElement.</param>
        /// <returns>Json string.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        private string CreateJsonString(XStreamingElement element)
        {
            MemoryStream memoryStream = new MemoryStream();

            using (var writer = JsonReaderWriterFactory.CreateJsonWriter(memoryStream, Encoding.Unicode, true))
            {
                element.WriteTo(writer);
                writer.Flush();
                memoryStream.Position = 0;
                return Encoding.Unicode.GetString(memoryStream.ToArray());
            }
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
