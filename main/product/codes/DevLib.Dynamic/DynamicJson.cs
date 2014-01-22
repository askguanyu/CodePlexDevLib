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
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization.Json;
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;

    /// <summary>
    /// Provides a class for specifying dynamic Json behavior at run time.
    /// </summary>
    public class DynamicJson : DynamicObject
    {
        /// <summary>
        /// Field _xElement.
        /// </summary>
        private readonly XElement _xElement;

        /// <summary>
        /// Field _jsonType.
        /// </summary>
        private readonly JsonType _jsonType;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicJson" /> class.
        /// </summary>
        public DynamicJson()
        {
            this._xElement = new XElement("root", CreateTypeAttribute(JsonType.@Object));
            this._jsonType = JsonType.@Object;
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
            @String,

            /// <summary>
            /// Represents number type.
            /// </summary>
            @Number,

            /// <summary>
            /// Represents bool type.
            /// </summary>
            @Bool,

            /// <summary>
            /// Represents complex object.
            /// </summary>
            @Object,

            /// <summary>
            /// Represents array.
            /// </summary>
            @Array,

            /// <summary>
            /// Represents null.
            /// </summary>
            @Null
        }

        /// <summary>
        /// Gets a value indicating whether this DynamicJson is complex object or not.
        /// </summary>
        public bool IsObject
        {
            get
            {
                return this._jsonType == JsonType.@Object;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this DynamicJson is array or not.
        /// </summary>
        public bool IsArray
        {
            get
            {
                return this._jsonType == JsonType.@Array;
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
                return ToValue(XElement.Load(reader));
            }
        }

        /// <summary>
        /// Parse Json string stream to DynamicJson.
        /// </summary>
        /// <param name="stream">Source Json string stream.</param>
        /// <param name="encoding">The encoding to apply to the string.</param>
        /// <returns>DynamicJson object.</returns>
        public static dynamic Parse(Stream stream, Encoding encoding = null)
        {
            using (var reader = JsonReaderWriterFactory.CreateJsonReader(stream, encoding ?? Encoding.Unicode, XmlDictionaryReaderQuotas.Max, _ => { }))
            {
                return ToValue(XElement.Load(reader));
            }
        }

        /// <summary>
        /// Load Json file to DynamicJson.
        /// </summary>
        /// <param name="jsonFile">Source Json file.</param>
        /// <param name="encoding">The encoding to apply to the string.</param>
        /// <returns>DynamicJson object.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public static dynamic Load(string jsonFile, Encoding encoding = null)
        {
            using (var reader = JsonReaderWriterFactory.CreateJsonReader(File.OpenRead(jsonFile), encoding ?? Encoding.Unicode, XmlDictionaryReaderQuotas.Max, _ => { }))
            {
                return ToValue(XElement.Load(reader));
            }
        }

        /// <summary>
        /// Serializes object to Json string.
        /// </summary>
        /// <param name="obj">Object to serialize.</param>
        /// <returns>Json string.</returns>
        public static string Serialize(object obj)
        {
            return CreateJsonString(new XStreamingElement("root", CreateTypeAttribute(GetJsonType(obj)), CreateJsonNode(obj)));
        }

        /// <summary>
        /// Gets a value indicating whether the current DynamicJson has specified property name.
        /// </summary>
        /// <param name="name">Property name.</param>
        /// <returns>true if the current DynamicJson has specified property name; otherwise, false.</returns>
        public bool IsDefined(string name)
        {
            return this.IsObject && (this._xElement.Element(name) != null);
        }

        /// <summary>
        /// Gets a value indicating whether the current DynamicJson has specified property index.
        /// </summary>
        /// <param name="index">Index of the property.</param>
        /// <returns>true if the current DynamicJson has specified property index; otherwise, false.</returns>
        public bool IsDefined(int index)
        {
            return this.IsArray && (this._xElement.Elements().ElementAtOrDefault(index) != null);
        }

        /// <summary>
        /// Delete property by name.
        /// </summary>
        /// <param name="name">Property name.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public bool Delete(string name)
        {
            var elem = this._xElement.Element(name);

            if (elem != null)
            {
                elem.Remove();

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
        public bool Delete(int index)
        {
            var elem = this._xElement.Elements().ElementAtOrDefault(index);

            if (elem != null)
            {
                elem.Remove();

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Deserializes DynamicJson to object.
        /// </summary>
        /// <typeparam name="T">Type of the <paramref name="returns"/> objet.</typeparam>
        /// <returns>Instance of object.</returns>
        public T Deserialize<T>()
        {
            return (T)this.Deserialize(typeof(T));
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
            result = this.IsArray ? this.Delete((int)args[0]) : this.Delete((string)args[0]);

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
            if (args.Length > 0)
            {
                result = null;

                return false;
            }

            result = this.IsDefined(binder.Name);

            return true;
        }

        /// <summary>
        /// Provides implementation for type conversion operations.
        /// </summary>
        /// <param name="binder">Provides information about the conversion operation.</param>
        /// <param name="result">The result of the type conversion operation.</param>
        /// <returns>true if the operation is successful; otherwise, false.</returns>
        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            if (binder.Type == typeof(IEnumerable) || binder.Type == typeof(object[]))
            {
                var enumerable = this.IsArray ? this._xElement.Elements().Select(i => ToValue(i)) : this._xElement.Elements().Select(i => (dynamic)new KeyValuePair<string, object>(i.Name.LocalName, ToValue(i)));

                result = (binder.Type == typeof(object[])) ? enumerable.ToArray() : enumerable;
            }
            else
            {
                result = this.Deserialize(binder.Type);
            }

            return true;
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
            return this.IsArray ? this.TryGet(this._xElement.Elements().ElementAtOrDefault((int)indexes[0]), out result) : this.TryGet(this._xElement.Element((string)indexes[0]), out result);
        }

        /// <summary>
        /// Provides the implementation for operations that get member values.
        /// </summary>
        /// <param name="binder">Provides information about the object that called the dynamic operation.</param>
        /// <param name="result">The result of the get operation.</param>
        /// <returns>true if the operation is successful; otherwise, false.</returns>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return this.IsArray ? this.TryGet(this._xElement.Elements().ElementAtOrDefault(int.Parse(binder.Name)), out result) : this.TryGet(this._xElement.Element(binder.Name), out result);
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
            return this.IsArray ? this.TrySet((int)indexes[0], value) : this.TrySet((string)indexes[0], value);
        }

        /// <summary>
        /// Provides the implementation for operations that set member values.
        /// </summary>
        /// <param name="binder">Provides information about the object that called the dynamic operation.</param>
        /// <param name="value">The value to set to the member.</param>
        /// <returns>true if the operation is successful; otherwise, false.</returns>
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            return this.IsArray ? this.TrySet(int.Parse(binder.Name), value) : this.TrySet(binder.Name, value);
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
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A Json string that represents the current object.</returns>
        public override string ToString()
        {
            foreach (var item in this._xElement.Descendants().Where(x => x.Attribute("type").Value == "null"))
            {
                item.RemoveNodes();
            }

            return CreateJsonString(new XStreamingElement("root", CreateTypeAttribute(this._jsonType), this._xElement.Elements()));
        }

        /// <summary>
        /// Method ToValue.
        /// </summary>
        /// <param name="xElement">Source XElement.</param>
        /// <returns>Value represents DynamicJson.</returns>
        private static dynamic ToValue(XElement xElement)
        {
            var type = (JsonType)Enum.Parse(typeof(JsonType), xElement.Attribute("type").Value);

            switch (type)
            {
                case JsonType.@Bool:
                    return (bool)xElement;
                case JsonType.@Number:
                    return (double)xElement;
                case JsonType.@String:
                    return (string)xElement;
                case JsonType.@Object:
                case JsonType.@Array:
                    return new DynamicJson(xElement, type);
                case JsonType.@Null:
                default:
                    return null;
            }
        }

        /// <summary>
        /// Method GetJsonType.
        /// </summary>
        /// <param name="obj">Source object.</param>
        /// <returns>JsonType of source object.</returns>
        private static JsonType GetJsonType(object obj)
        {
            if (obj == null)
            {
                return JsonType.@Null;
            }

            switch (Type.GetTypeCode(obj.GetType()))
            {
                case TypeCode.Boolean:
                    return JsonType.@Bool;
                case TypeCode.String:
                case TypeCode.Char:
                case TypeCode.DateTime:
                    return JsonType.@String;
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
                    return JsonType.@Number;
                case TypeCode.Object:
                    return (obj is IEnumerable) ? JsonType.@Array : JsonType.@Object;
                case TypeCode.DBNull:
                case TypeCode.Empty:
                default:
                    return JsonType.@Null;
            }
        }

        /// <summary>
        /// Method CreateTypeAttribute.
        /// </summary>
        /// <param name="jsonType">Source JsonType.</param>
        /// <returns>Instance of XAttribute.</returns>
        private static XAttribute CreateTypeAttribute(JsonType jsonType)
        {
            return new XAttribute("type", jsonType.ToString());
        }

        /// <summary>
        /// Method CreateJsonNode.
        /// </summary>
        /// <param name="obj">Source object.</param>
        /// <returns>JsonNode object.</returns>
        private static object CreateJsonNode(object obj)
        {
            var type = GetJsonType(obj);

            switch (type)
            {
                case JsonType.@String:
                case JsonType.@Number:
                    return obj;
                case JsonType.@Bool:
                    return obj.ToString().ToLowerInvariant();
                case JsonType.@Object:
                    return CreateXObject(obj);
                case JsonType.@Array:
                    return CreateXArray(obj as IEnumerable);
                case JsonType.@Null:
                default:
                    return null;
            }
        }

        /// <summary>
        /// Method CreateXArray.
        /// </summary>
        /// <typeparam name="T">Type of the <paramref name="obj"/>.</typeparam>
        /// <param name="obj">Source object.</param>
        /// <returns>Instance of IEnumerable{XStreamingElement}.</returns>
        private static IEnumerable<XStreamingElement> CreateXArray<T>(T obj) where T : IEnumerable
        {
            return obj.Cast<object>().Select(i => new XStreamingElement("item", CreateTypeAttribute(GetJsonType(i)), CreateJsonNode(i)));
        }

        /// <summary>
        /// Method CreateXObject.
        /// </summary>
        /// <param name="obj">Source object.</param>
        /// <returns>Instance of IEnumerable{XStreamingElement}.</returns>
        private static IEnumerable<XStreamingElement> CreateXObject(object obj)
        {
            return obj.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(i => new { Name = i.Name, Value = i.GetValue(obj, null) })
                .Select(i => new XStreamingElement(i.Name, CreateTypeAttribute(GetJsonType(i.Value)), CreateJsonNode(i.Value)));
        }

        /// <summary>
        /// Method CreateJsonString.
        /// </summary>
        /// <param name="element">Source XStreamingElement.</param>
        /// <returns>Json string.</returns>
        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "Reviewed.")]
        private static string CreateJsonString(XStreamingElement element)
        {
            using (var memoryStream = new MemoryStream())
            using (var writer = JsonReaderWriterFactory.CreateJsonWriter(memoryStream, Encoding.Unicode))
            {
                element.WriteTo(writer);
                writer.Flush();
                return Encoding.Unicode.GetString(memoryStream.ToArray());
            }
        }

        /// <summary>
        /// Method Deserialize.
        /// </summary>
        /// <param name="targetType">Target Type.</param>
        /// <returns>Instance of object.</returns>
        private object Deserialize(Type targetType)
        {
            return this.IsArray ? this.DeserializeArray(targetType) : this.DeserializeObject(targetType);
        }

        /// <summary>
        /// Method DeserializeValue.
        /// </summary>
        /// <param name="xElement">Source XElement.</param>
        /// <param name="elementType">Element type.</param>
        /// <returns>Element value.</returns>
        private dynamic DeserializeValue(XElement xElement, Type elementType)
        {
            var value = ToValue(xElement);

            if (value is DynamicJson)
            {
                value = ((DynamicJson)value).Deserialize(elementType);
            }

            return Convert.ChangeType(value, elementType);
        }

        /// <summary>
        /// Method DeserializeObject.
        /// </summary>
        /// <param name="targetType">Target Type.</param>
        /// <returns>Instance of object.</returns>
        private object DeserializeObject(Type targetType)
        {
            var result = Activator.CreateInstance(targetType);

            var dict = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(i => i.CanWrite).ToDictionary(i => i.Name, i => i);

            foreach (var item in this._xElement.Elements())
            {
                PropertyInfo propertyInfo;

                if (!dict.TryGetValue(item.Name.LocalName, out propertyInfo))
                {
                    continue;
                }

                var value = this.DeserializeValue(item, propertyInfo.PropertyType);

                propertyInfo.SetValue(result, value, null);
            }

            return result;
        }

        /// <summary>
        /// Method DeserializeArray.
        /// </summary>
        /// <param name="targetType">Target Type.</param>
        /// <returns>Instance of object.</returns>
        private object DeserializeArray(Type targetType)
        {
            if (targetType.IsArray)
            {
                var elementType = targetType.GetElementType();

                dynamic array = Array.CreateInstance(elementType, this._xElement.Elements().Count());

                var index = 0;

                foreach (var item in this._xElement.Elements())
                {
                    array[index++] = this.DeserializeValue(item, elementType);
                }

                return array;
            }
            else
            {
                var elementType = targetType.GetGenericArguments()[0];

                dynamic list = Activator.CreateInstance(targetType);

                foreach (var item in this._xElement.Elements())
                {
                    list.Add(this.DeserializeValue(item, elementType));
                }

                return list;
            }
        }

        /// <summary>
        /// Method TryGet.
        /// </summary>
        /// <param name="xElement">Source XElement.</param>
        /// <param name="result">Value of XElement.</param>
        /// <returns>true if the operation is successful; otherwise, false.</returns>
        private bool TryGet(XElement xElement, out object result)
        {
            if (xElement == null)
            {
                result = null;

                return false;
            }

            result = ToValue(xElement);

            return true;
        }

        /// <summary>
        /// Method TrySet.
        /// </summary>
        /// <param name="name">Property name.</param>
        /// <param name="value">Value for property.</param>
        /// <returns>true if the operation is successful; otherwise, false.</returns>
        private bool TrySet(string name, object value)
        {
            var type = GetJsonType(value);

            var element = this._xElement.Element(name);

            if (element == null)
            {
                this._xElement.Add(new XElement(name, CreateTypeAttribute(type), CreateJsonNode(value)));
            }
            else
            {
                element.Attribute("type").Value = type.ToString();

                element.ReplaceNodes(CreateJsonNode(value));
            }

            return true;
        }

        /// <summary>
        /// Method TrySet.
        /// </summary>
        /// <param name="index">Property index.</param>
        /// <param name="value">Value for index.</param>
        /// <returns>true if the operation is successful; otherwise, false.</returns>
        private bool TrySet(int index, object value)
        {
            var type = GetJsonType(value);

            var element = this._xElement.Elements().ElementAtOrDefault(index);

            if (element == null)
            {
                this._xElement.Add(new XElement("item", CreateTypeAttribute(type), CreateJsonNode(value)));
            }
            else
            {
                element.Attribute("type").Value = type.ToString();

                element.ReplaceNodes(CreateJsonNode(value));
            }

            return true;
        }
    }
}
