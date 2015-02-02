//-----------------------------------------------------------------------
// <copyright file="MetadataModel.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Metadata
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    /// <summary>
    /// Class MetadataModel.
    /// </summary>
    [Serializable]
    public class MetadataModel : IXmlSerializable
    {
        /// <summary>
        /// Field PropertiesXmlSerializer.
        /// </summary>
        private static readonly XmlSerializer PropertiesXmlSerializer;

        /// <summary>
        /// Field EmptyXmlSerializerNamespaces.
        /// </summary>
        private static readonly XmlSerializerNamespaces EmptyXmlSerializerNamespaces;

        /// <summary>
        /// Initializes static members of the <see cref="MetadataModel" /> class.
        /// </summary>
        static MetadataModel()
        {
            PropertiesXmlSerializer = new XmlSerializer(typeof(List<MetadataModel>), new XmlRootAttribute("Properties"));

            EmptyXmlSerializerNamespaces = new XmlSerializerNamespaces();

            EmptyXmlSerializerNamespaces.Add(string.Empty, string.Empty);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataModel"/> class.
        /// </summary>
        public MetadataModel()
        {
            this.Name = string.Empty;
            this.IsValueType = false;
            this.Value = null;
            this.Properties = null;
        }

        /// <summary>
        /// Gets or sets MetadataModel object name.
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this MetadataModel is a simple value type or not.
        /// </summary>
        public bool IsValueType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets MetadataModel object value.
        /// </summary>
        public string Value
        {
            get;
            set;
        }

        /// <summary>
        /// Gets Properties Count.
        /// </summary>
        public int PropertiesCount
        {
            get
            {
                if (this.Properties == null)
                {
                    return 0;
                }
                else
                {
                    return this.Properties.Count;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether current Metadata represent null or not.
        /// </summary>
        public bool IsNull
        {
            get
            {
                return this.IsValueType ? this.Value == null : this.Properties == null;
            }
        }

        /// <summary>
        /// Gets or sets MetadataModel properties.
        /// </summary>
        public List<MetadataModel> Properties
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the MetadataModel at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the MetadataModel to get or set.</param>
        /// <returns>The MetadataModel at the specified index.</returns>
        public MetadataModel this[int index]
        {
            get
            {
                if (!this.IsValueType)
                {
                    return this.Properties[index];
                }
                else
                {
                    throw new InvalidOperationException("This MetadataModel is a value type. Cannot access MetadataModel Properties.");
                }
            }

            set
            {
                if (!this.IsValueType)
                {
                    if (this.Properties == null)
                    {
                        this.Properties = new List<MetadataModel>();
                    }

                    int count = this.Properties.Count;

                    if (index > count - 1)
                    {
                        for (int i = 0; i <= index - count; i++)
                        {
                            this.Properties.Add(MetadataModel.GetEmptyMetadataModel());
                        }
                    }

                    this.Properties[index].CloneFrom(value, false);
                }
                else
                {
                    throw new InvalidOperationException("This MetadataModel is a value type. Cannot access MetadataModel Properties.");
                }
            }
        }

        /// <summary>
        /// Gets or sets the MetadataModel at the specified property name.
        /// </summary>
        /// <param name="propertyName">The property name of the MetadataModel to get or set.</param>
        /// <returns>The MetadataModel at the specified property name.</returns>
        public MetadataModel this[string propertyName]
        {
            get
            {
                if (!this.IsValueType)
                {
                    if (propertyName == string.Empty)
                    {
                        return this;
                    }
                    else
                    {
                        return this.Properties.First(i => i.Name.Equals(propertyName));
                    }
                }
                else if (this.Name == propertyName)
                {
                    return this;
                }
                else
                {
                    throw new InvalidOperationException(string.Format("This MetadataModel (Name: {0}) is a value type. Cannot access MetadataModel Properties.", this.Name ?? string.Empty));
                }
            }

            set
            {
                if (!this.IsValueType)
                {
                    if (this.Properties == null)
                    {
                        this.Properties = new List<MetadataModel>();
                    }

                    int index = this.Properties.FindIndex(i => i.Name.Equals(propertyName));

                    if (index < 0)
                    {
                        this.Properties.Add(new MetadataModel { Name = propertyName }.CloneFrom(value, false));
                    }
                    else
                    {
                        this.Properties[index].CloneFrom(value, false);
                    }
                }
                else if (this.Name == propertyName)
                {
                    this.Value = value.Value;
                }
                else
                {
                    throw new InvalidOperationException(string.Format("This MetadataModel (Name: {0}) is a value type. Cannot access MetadataModel Properties.", this.Name ?? string.Empty));
                }
            }
        }

        /// <summary>
        /// Gets the empty metadata model.
        /// </summary>
        /// <returns>The empty MetadataModel.</returns>
        public static MetadataModel GetEmptyMetadataModel()
        {
            return new MetadataModel { Value = null, Properties = null };
        }

        /// <summary>
        /// Create an empty MetadataModel from specified type.
        /// </summary>
        /// <param name="type">The source type to create from.</param>
        /// <returns>Instance of MetadataModel.</returns>
        public static MetadataModel CreateFrom(Type type)
        {
            return ((object)null).ToMetadataModel(type);
        }

        /// <summary>
        /// Gets the MetadataModel at the specified property name.
        /// </summary>
        /// <param name="propertyName">The property name of the MetadataModel to get.</param>
        /// <returns>The MetadataModel at the specified property name.</returns>
        public MetadataModel GetProperty(string propertyName)
        {
            if (!this.IsValueType)
            {
                if (string.IsNullOrWhiteSpace(propertyName))
                {
                    return this;
                }

                if (!propertyName.Contains('.'))
                {
                    if (!propertyName.EndsWith("]"))
                    {
                        return this[propertyName];
                    }
                    else
                    {
                        int propertyNameIndex = propertyName.LastIndexOf("[");
                        string propertyIndexerName = propertyName.Substring(0, propertyNameIndex);
                        int index = int.Parse(propertyName.Substring(propertyNameIndex, propertyName.Length - propertyNameIndex).Trim('[', ']'));
                        return this.GetProperty(propertyIndexerName)[index];
                    }
                }
                else
                {
                    var propertyChain = propertyName.Trim().Split('.');

                    MetadataModel result = this;

                    foreach (var item in propertyChain)
                    {
                        result = result.GetProperty(item);

                        if (result == null)
                        {
                            break;
                        }
                    }

                    return result;
                }
            }
            else if (this.Name == propertyName)
            {
                return this;
            }
            else
            {
                throw new InvalidOperationException(string.Format("This MetadataModel (Name: {0}) is a value type. Cannot access MetadataModel Properties.", this.Name ?? string.Empty));
            }
        }

        /// <summary>
        /// Gets the property.
        /// </summary>
        /// <typeparam name="T">The type of the object who contains the property.</typeparam>
        /// <param name="propertyExpression">The property expression.</param>
        /// <returns>The MetadataModel.</returns>
        public MetadataModel GetProperty<T>(Expression<Func<T, object>> propertyExpression)
        {
            return this.GetProperty(propertyExpression.ExtractPropertyName<T>());
        }

        /// <summary>
        /// Determines whether the MetadataModel has the specified property.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>true if the MetadataModel has the specified property; otherwise, false.</returns>
        public bool HasProperty(string propertyName)
        {
            if (!this.IsValueType)
            {
                if (!propertyName.Contains('.'))
                {
                    if (this.Properties != null && this.Properties.Count > 0)
                    {
                        string itemName = propertyName;

                        if (propertyName.EndsWith("]"))
                        {
                            int itemNameIndex = propertyName.LastIndexOf("[");
                            string itemIndexerName = propertyName.Substring(0, itemNameIndex);
                            itemName = itemIndexerName;
                        }

                        return this.Properties.Any(p => p.Name.Equals(itemName));
                    }

                    return false;
                }
                else
                {
                    var propertyChain = propertyName.Trim().Split('.');

                    MetadataModel result = this;

                    foreach (var item in propertyChain)
                    {
                        try
                        {
                            if (result.Properties != null && result.Properties.Count > 0)
                            {
                                string itemName = item;

                                if (item.EndsWith("]"))
                                {
                                    int itemNameIndex = item.LastIndexOf("[");
                                    string itemIndexerName = item.Substring(0, itemNameIndex);
                                    itemName = itemIndexerName;
                                }

                                if (result.Properties.Any(p => p.Name.Equals(itemName)))
                                {
                                    result = result.GetProperty(item);
                                    continue;
                                }
                            }

                            return false;
                        }
                        catch
                        {
                            return false;
                        }
                    }

                    return true;
                }
            }
            else
            {
                throw new InvalidOperationException(string.Format("This MetadataModel (Name: {0}) is a value type. Cannot access MetadataModel Properties.", this.Name ?? string.Empty));
            }
        }

        /// <summary>
        /// Determines whether the MetadataModel has the specified property.
        /// </summary>
        /// <typeparam name="T">The type of the object who contains the property.</typeparam>
        /// <param name="propertyExpression">The property expression.</param>
        /// <returns>true if the MetadataModel has the specified property; otherwise, false.</returns>
        public bool HasProperty<T>(Expression<Func<T, object>> propertyExpression)
        {
            return this.HasProperty(propertyExpression.ExtractPropertyName<T>());
        }

        /// <summary>
        /// Sets the MetadataModel at the specified property name.
        /// </summary>
        /// <param name="propertyName">The property name of the MetadataModel to set.</param>
        /// <param name="property">The instance of the property.</param>
        public void SetProperty(string propertyName, MetadataModel property)
        {
            if (!this.IsValueType)
            {
                if (!propertyName.Contains('.'))
                {
                    if (!propertyName.EndsWith("]"))
                    {
                        this[propertyName] = property;
                    }
                    else
                    {
                        int propertyNameIndex = propertyName.LastIndexOf("[");
                        string propertyIndexerName = propertyName.Substring(0, propertyNameIndex);
                        int index = int.Parse(propertyName.Substring(propertyNameIndex, propertyName.Length - propertyNameIndex).Trim('[', ']'));

                        MetadataModel result = this;

                        try
                        {
                            if (result.Properties != null && result.Properties.Count > 0 && result.Properties.Any(p => p.Name.Equals(propertyIndexerName)))
                            {
                                result = result.GetProperty(propertyIndexerName);
                            }
                            else
                            {
                                result.SetProperty(propertyIndexerName, new MetadataModel { Name = propertyIndexerName, IsValueType = false });

                                result = result.GetProperty(propertyIndexerName);
                            }
                        }
                        catch
                        {
                            result.SetProperty(propertyIndexerName, new MetadataModel { Name = propertyIndexerName, IsValueType = false });

                            result = result.GetProperty(propertyIndexerName);
                        }

                        result[index] = property;
                    }
                }
                else
                {
                    var propertyChain = propertyName.Trim().Split('.');

                    MetadataModel result = this;

                    foreach (var item in propertyChain)
                    {
                        try
                        {
                            if (result.Properties != null && result.Properties.Count > 0)
                            {
                                string itemName = item;

                                if (item.EndsWith("]"))
                                {
                                    int itemNameIndex = item.LastIndexOf("[");
                                    string itemIndexerName = item.Substring(0, itemNameIndex);
                                    itemName = itemIndexerName;
                                }

                                if (result.Properties.Any(p => p.Name.Equals(itemName)) || itemName == string.Empty)
                                {
                                    result = result.GetProperty(item);
                                    continue;
                                }
                            }

                            result.SetProperty(item, new MetadataModel { Name = item, IsValueType = false });

                            result = result.GetProperty(item);
                        }
                        catch
                        {
                            result.SetProperty(item, new MetadataModel { Name = item, IsValueType = false });

                            result = result.GetProperty(item);
                        }
                    }

                    result.CloneFrom(property);
                }
            }
            else if (this.Name == propertyName)
            {
                this.Value = property.Value;
            }
            else
            {
                throw new InvalidOperationException(string.Format("This MetadataModel (Name: {0}) is a value type. Cannot access MetadataModel Properties.", this.Name ?? string.Empty));
            }
        }

        /// <summary>
        /// Sets the MetadataModel at the specified property name.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="property">The instance of the property.</param>
        public void SetProperty<TProperty>(string propertyName, TProperty property)
        {
            this.SetProperty(propertyName, property.ToMetadataModel());
        }

        /// <summary>
        /// Sets the MetadataModel at the specified property name.
        /// </summary>
        /// <typeparam name="T">The type of the object who contains the property.</typeparam>
        /// <param name="propertyExpression">The property expression.</param>
        /// <param name="property">The instance of the property.</param>
        public void SetProperty<T>(Expression<Func<T, object>> propertyExpression, MetadataModel property)
        {
            this.SetProperty(propertyExpression.ExtractPropertyName<T>(), property);
        }

        /// <summary>
        /// Sets the MetadataModel at the specified property name.
        /// </summary>
        /// <typeparam name="T">The type of the object who contains the property.</typeparam>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="propertyExpression">The property expression.</param>
        /// <param name="property">The instance of the property.</param>
        public void SetProperty<T, TProperty>(Expression<Func<T, TProperty>> propertyExpression, TProperty property)
        {
            this.SetProperty(propertyExpression.ExtractPropertyName<T, TProperty>(), property.ToMetadataModel());
        }

        /// <summary>
        /// Adds the MetadataModel to the end of the Properties.
        /// </summary>
        /// <param name="property">The property to be added.</param>
        public void AddProperty(MetadataModel property)
        {
            if (!this.IsValueType)
            {
                if (this.Properties == null)
                {
                    this.Properties = new List<MetadataModel>();
                }

                this.Properties.Add(property);
            }
            else
            {
                throw new InvalidOperationException(string.Format("This MetadataModel (Name: {0}) is a value type. Cannot access MetadataModel Properties.", this.Name ?? string.Empty));
            }
        }

        /// <summary>
        /// Removes the MetadataModel at the specified property name.
        /// </summary>
        /// <param name="propertyName">The property to remove.</param>
        public void RemoveProperty(string propertyName)
        {
            if (!this.IsValueType)
            {
                if (this.Properties != null)
                {
                    int index = this.Properties.FindIndex(i => i.Name.Equals(propertyName));

                    if (index >= 0)
                    {
                        this.Properties.RemoveAt(index);
                    }
                }
            }
            else
            {
                throw new InvalidOperationException(string.Format("This MetadataModel (Name: {0}) is a value type. Cannot access MetadataModel Properties.", this.Name ?? string.Empty));
            }
        }

        /// <summary>
        /// Removes the MetadataModel at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        public void RemoveProperty(int index)
        {
            if (!this.IsValueType)
            {
                if (this.Properties != null)
                {
                    if (index >= 0)
                    {
                        this.Properties.RemoveAt(index);
                    }
                }
            }
            else
            {
                throw new InvalidOperationException(string.Format("This MetadataModel (Name: {0}) is a value type. Cannot access MetadataModel Properties.", this.Name ?? string.Empty));
            }
        }

        /// <summary>
        /// This method is reserved and should not be used. When implementing the IXmlSerializable interface, you should return null (Nothing in Visual Basic) from this method, and instead, if specifying a custom schema is required, apply the System.Xml.Serialization.XmlSchemaProviderAttribute to the class.
        /// </summary>
        /// <returns>An System.Xml.Schema.XmlSchema that describes the XML representation of the object that is produced by the System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter) method and consumed by the System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader) method.</returns>
        public XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>
        /// Generates an object from its XML representation.
        /// </summary>
        /// <param name="reader">The System.Xml.XmlReader stream from which the object is deserialized.</param>
        public void ReadXml(XmlReader reader)
        {
            try
            {
                this.Name = reader.GetAttribute("name");
            }
            catch
            {
            }

            try
            {
                this.IsValueType = bool.Parse(reader.GetAttribute("isValueType"));
            }
            catch
            {
                this.IsValueType = false;
            }

            if (this.IsValueType)
            {
                try
                {
                    this.Value = reader.GetAttribute("value");
                }
                catch
                {
                    this.Value = null;
                }
            }

            reader.ReadStartElement();

            if (!this.IsValueType)
            {
                if (!reader.IsEmptyElement)
                {
                    try
                    {
                        this.Properties = PropertiesXmlSerializer.Deserialize(reader) as List<MetadataModel>;
                    }
                    catch
                    {
                    }

                    reader.ReadEndElement();
                }
            }
        }

        /// <summary>
        /// Converts an object into its XML representation.
        /// </summary>
        /// <param name="writer">The System.Xml.XmlWriter stream to which the object is serialized.</param>
        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("name", this.Name);
            writer.WriteAttributeString("isValueType", this.IsValueType.ToString());

            if (this.IsValueType)
            {
                if (this.Value != null)
                {
                    writer.WriteAttributeString("value", this.Value);
                }
            }
            else
            {
                if (this.Properties != null)
                {
                    try
                    {
                        PropertiesXmlSerializer.Serialize(writer, this.Properties, EmptyXmlSerializerNamespaces);
                    }
                    catch
                    {
                    }
                }
            }
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            if (this.IsValueType)
            {
                return string.Format("{0} = {1}", this.Name ?? string.Empty, this.Value ?? string.Empty);
            }
            else
            {
                return string.Format("PropertiesCount = {0}", this.Properties.Count);
            }
        }
    }

    /// <summary>
    /// Generic Class MetadataModel
    /// </summary>
    /// <typeparam name="T">The type of the model.</typeparam>
    [Serializable]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Reviewed.")]
    public class MetadataModel<T> : MetadataModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataModel{T}"/> class.
        /// </summary>
        public MetadataModel()
            : base()
        {
        }

        /// <summary>
        /// Gets the property.
        /// </summary>
        /// <param name="propertyExpression">The property expression.</param>
        /// <returns>The MetadataModel.</returns>
        [SuppressMessage("Microsoft.Design", "CA1061:DoNotHideBaseClassMethods", Justification = "Reviewed.")]
        public MetadataModel GetProperty(Expression<Func<T, object>> propertyExpression)
        {
            return this.GetProperty(propertyExpression.ExtractPropertyName<T>());
        }

        /// <summary>
        /// Sets the MetadataModel at the specified property name.
        /// </summary>
        /// <param name="propertyExpression">The property expression.</param>
        /// <param name="property">The instance of the property.</param>
        [SuppressMessage("Microsoft.Design", "CA1061:DoNotHideBaseClassMethods", Justification = "Reviewed.")]
        public void SetProperty(Expression<Func<T, object>> propertyExpression, MetadataModel property)
        {
            this.SetProperty(propertyExpression.ExtractPropertyName<T>(), property);
        }

        /// <summary>
        /// Sets the MetadataModel at the specified property name.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="propertyExpression">The property expression.</param>
        /// <param name="property">The instance of the property.</param>
        [SuppressMessage("Microsoft.Design", "CA1061:DoNotHideBaseClassMethods", Justification = "Reviewed.")]
        public void SetProperty<TProperty>(Expression<Func<T, TProperty>> propertyExpression, TProperty property)
        {
            this.SetProperty(propertyExpression.ExtractPropertyName<T, TProperty>(), property.ToMetadataModel());
        }
    }
}
