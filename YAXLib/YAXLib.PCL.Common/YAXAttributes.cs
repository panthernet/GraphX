// Copyright 2009 - 2010 Sina Iravanian - <sina@sinairv.com>
//
// This source file(s) may be redistributed, altered and customized
// by any means PROVIDING the authors name and all copyright
// notices remain intact.
// THIS SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED. USE IT AT YOUR OWN RISK. THE AUTHOR ACCEPTS NO
// LIABILITY FOR ANY DATA DAMAGE/LOSS THAT THIS PRODUCT MAY CAUSE.
//-----------------------------------------------------------------------

using System;

namespace YAXLib
{
    /// <summary>
    /// The base class for all attributes defined in YAXLib.
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public abstract class YAXBaseAttribute : System.Attribute
    {
    }

    /// <summary>
    /// Creates a comment node per each line of the comment string provided.
    /// This attribute is applicable to classes, structures, fields, and properties.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field | AttributeTargets.Property)]
    public class YAXCommentAttribute : YAXBaseAttribute
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="YAXCommentAttribute"/> class.
        /// </summary>
        /// <param name="comment">The comment.</param>
        public YAXCommentAttribute(string comment)
        {
            this.Comment = comment;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the comment.
        /// </summary>
        /// <value>The comment.</value>
        public string Comment { get; set; }

        #endregion
    }

    /// <summary>
    /// Add this attribute to types, structs or classes which you want to override
    /// their default serialization behaviour. This attribute is optional.
    /// This attribute is applicable to classes and structures.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class YAXSerializableTypeAttribute : YAXBaseAttribute
    {
        #region Private Fields

        /// <summary>
        /// determines whether the serialization options property has been explicitly
        /// set by the user.
        /// </summary>
        private bool m_isOptionSet = false;

        /// <summary>
        /// Private variable to hold the serialization options
        /// </summary>
        private YAXSerializationOptions m_serializationOptions = YAXSerializationOptions.SerializeNullObjects;

        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="YAXSerializableTypeAttribute"/> class.
        /// </summary>
        public YAXSerializableTypeAttribute()
        {
            this.FieldsToSerialize = YAXSerializationFields.PublicPropertiesOnly;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the fields which YAXLib selects for serialization
        /// </summary>
        /// <value>The fields to serialize.</value>
        public YAXSerializationFields FieldsToSerialize { get; set; }

        /// <summary>
        /// Gets or sets the serialization options.
        /// </summary>
        /// <value>The options.</value>
        public YAXSerializationOptions Options 
        {
            get
            {
                return m_serializationOptions;
            }

            set
            {
                m_serializationOptions = value;
                m_isOptionSet = true;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Determines whether the serialization options property has been explicitly
        /// set by the user.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if the serialization options property has been explicitly
        /// set by the user; otherwise, <c>false</c>.
        /// </returns>
        public bool IsSerializationOptionSet()
        {
            return m_isOptionSet;
        }

        #endregion
    }

    /// <summary>
    /// Makes an element make use of a specific XML namespace.
    /// This attribute is applicable to classes, structs, fields, enums and properties
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Struct)]
    public class YAXNamespaceAttribute : YAXBaseAttribute
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="YAXNamespaceAttribute"/> class.
        /// </summary>
        /// <remarks>
        /// The element this applies to will take on the given XML namespace. In the case
        /// of this constructor, the default one defined by xmlns="namespace"
        /// </remarks>
        /// <param name="defaultNamespace">The default namespace to use for this item</param>
        public YAXNamespaceAttribute(string defaultNamespace)
        {
            Namespace = defaultNamespace;
            Prefix = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="YAXNamespaceAttribute"/> class.
        /// </summary>
        /// <remarks>
        /// The element this applies to will take on the given XML namespace. The namespace
        /// will be added to the root XML element, with the given prefix in the form: 
        ///     xmlns:prefix="namespace"
        /// </remarks>
        /// <param name="namespacePrefix">The prefix to use for this element's namespace</param>
        /// <param name="xmlNamespace">The xml namespace to use for this item</param>
        public YAXNamespaceAttribute(string namespacePrefix, string xmlNamespace)
        {
            Namespace = xmlNamespace;
            Prefix = namespacePrefix;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The namespace path
        /// </summary>
        public string Namespace
        { get; private set; }

        /// <summary>
        /// The xml prefix used for the namespace
        /// </summary>
        public string Prefix
        { get; private set; }

        #endregion

    }

    /// <summary>
    /// Add this attribute to properties or fields which you wish to be serialized, when 
    /// the enclosing class uses the <c>YAXSerializableType</c> attribute in which <c>FieldsToSerialize</c>
    /// has been set to <c>AttributedFieldsOnly</c>.
    /// This attribute is applicable to fields and properties.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class YAXSerializableFieldAttribute : YAXBaseAttribute
    {
    }
    
    /// <summary>
    /// Makes a property to appear as an attribute for the enclosing class (i.e. the parent element) if possible.
    /// This attribute is applicable to fields and properties only.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class YAXAttributeForClassAttribute : YAXBaseAttribute
    {
    }

    /// <summary>
    /// Makes a field or property to appear as an attribute for another element, if possible.
    /// This attribute is applicable to fields and properties.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class YAXAttributeForAttribute : YAXBaseAttribute
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="YAXAttributeForAttribute"/> class.
        /// </summary>
        /// <param name="parent">The element of which the property becomes an attribute.</param>
        public YAXAttributeForAttribute(string parent)
        {
            this.Parent = parent;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the element of which the property becomes an attribute.
        /// </summary>
        public string Parent { get; set; }

        #endregion
    }

    // TODO: rename to YAXContentFor in v3

    /// <summary>
    /// Makes a field or property to appear as a value for another element, if possible.
    /// This attribute is applicable to fields and properties.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class YAXValueForAttribute : YAXBaseAttribute
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="YAXAttributeForAttribute"/> class.
        /// </summary>
        /// <param name="parent">The element of which the property becomes an attribute.</param>
        public YAXValueForAttribute(string parent)
        {
            this.Parent = parent;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the element for which the property becomes a value.
        /// </summary>
        public string Parent { get; set; }

        #endregion
    }


    // TODO: rename to YAXContentForClass in v3
    /// <summary>
    /// Makes a field or property to appear as a value for its parent element, if possible.
    /// This attribute is applicable to fields and properties.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class YAXValueForClassAttribute : YAXBaseAttribute
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="YAXValueForClassAttribute"/> class.
        /// </summary>
        public YAXValueForClassAttribute()
        {
        }

        #endregion

    }



    /// <summary>
    /// Prevents serialization of some field or property.
    /// This attribute is applicable to fields and properties.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class YAXDontSerializeAttribute : YAXBaseAttribute
    {
    }

    /// <summary>
    /// Defines an alias for the field, property, class, or struct under 
    /// which it will be serialized. This attribute is applicable to fields, 
    /// properties, classes, and structs.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct)]
    public class YAXSerializeAsAttribute : YAXBaseAttribute
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="YAXSerializeAsAttribute"/> class.
        /// </summary>
        /// <param name="serializeAs">the alias for the property under which the property will be serialized.</param>
        public YAXSerializeAsAttribute(string serializeAs)
        {
            this.SerializeAs = serializeAs;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the alias for the property under which the property will be serialized.
        /// </summary>
        public string SerializeAs { get; set; }

        #endregion
    }

    /// <summary>
    /// Makes a property or field to appear as a child element 
    /// for another element. This attribute is applicable to fields and properties.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class YAXElementForAttribute : YAXBaseAttribute
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="YAXElementForAttribute"/> class.
        /// </summary>
        /// <param name="parent">The element of which the property becomes a child element.</param>
        public YAXElementForAttribute(string parent)
        {
            this.Parent = parent;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the element of which the property becomes a child element.
        /// </summary>
        /// <value>The element of which the property becomes a child element.</value>
        public string Parent { get; set; }

        #endregion
    }

    /// <summary>
    /// Controls the serialization of collection instances.
    /// This attribute is applicable to fields and properties, and collection classes.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct)]
    public class YAXCollectionAttribute : YAXBaseAttribute
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="YAXCollectionAttribute"/> class.
        /// </summary>
        /// <param name="serType">type of the serialization of the collection.</param>
        public YAXCollectionAttribute(YAXCollectionSerializationTypes serType)
        {
            this.SerializationType = serType;
            this.SeparateBy = " ";
            this.EachElementName = null;
            this.IsWhiteSpaceSeparator = true;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the type of the serialization of the collection.
        /// </summary>
        /// <value>The type of the serialization of the collection.</value>
        public YAXCollectionSerializationTypes SerializationType { get; set; }

        /// <summary>
        /// Gets or sets the string to separate collection items, if the Serialization type is set to <c>Serially</c>.
        /// </summary>
        /// <value>the string to separate collection items, if the Serialization Type is set to <c>Serially</c>.</value>
        public string SeparateBy { get; set; }

        /// <summary>
        /// Gets or sets the name of each child element corresponding to the collection members, if the Serialization type is set to <c>Recursive</c>.
        /// </summary>
        /// <value>The name of each child element corresponding to the collection members, if the Serialization type is set to <c>Recursive</c>.</value>
        public string EachElementName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether white space characters are to be
        /// treated as sparators or not.
        /// </summary>
        /// <value>
        /// <c>true</c> if white space separator characters are to be
        /// treated as sparators; otherwise, <c>false</c>.
        /// </value>
        public bool IsWhiteSpaceSeparator { get; set; }

        #endregion
    }

    /// <summary>
    /// Controls the serialization of generic Dictionary instances.
    /// This attribute is applicable to fields and properties, and 
    /// classes derived from the <c>Dictionary</c> base class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property| AttributeTargets.Class | AttributeTargets.Struct)]
    public class YAXDictionaryAttribute : YAXBaseAttribute
    {
        private YAXNodeTypes _serializeKeyAs = YAXNodeTypes.Element;
        private YAXNodeTypes _serializeValueAs = YAXNodeTypes.Element;

        /// <summary>
        /// Initializes a new instance of the <see cref="YAXDictionaryAttribute"/> class.
        /// </summary>
        public YAXDictionaryAttribute()
        {
            KeyName = null;
            ValueName = null;
            EachPairName = null;
            KeyFormatString = null;
            ValueFormatString = null;
        }

        /// <summary>
        /// Gets or sets the alias for the key part of the dicitonary.
        /// </summary>
        /// <value></value>
        public string KeyName { get; set; }

        /// <summary>
        /// Gets or sets alias for the value part of the dicitonary.
        /// </summary>
        /// <value></value>
        public string ValueName { get; set; }

        /// <summary>
        /// Gets or sets alias for the element containing the Key-Value pair.
        /// </summary>
        /// <value></value>
        public string EachPairName { get; set; }

        /// <summary>
        /// Gets or sets the node type according to which the key part of the dictionary is serialized.
        /// </summary>
        /// <value></value>
        public YAXNodeTypes SerializeKeyAs 
        {
            get
            {
                return _serializeKeyAs;
            }

            set
            {
                _serializeKeyAs = value;
                CheckIntegrity();
            }
        }

        /// <summary>
        /// Gets or sets the node type according to which the value part of the dictionary is serialized.
        /// </summary>
        /// <value></value>
        public YAXNodeTypes SerializeValueAs 
        {
            get
            {
                return _serializeValueAs;
            }
            
            set
            {
                _serializeValueAs = value;
                CheckIntegrity();
            }
        }

        /// <summary>
        /// Gets or sets the key format string.
        /// </summary>
        /// <value></value>
        public string KeyFormatString { get; set; }

        /// <summary>
        /// Gets or sets the value format string.
        /// </summary>
        /// <value></value>
        public string ValueFormatString { get; set; }

        private void CheckIntegrity()
        {
            if (_serializeKeyAs == _serializeValueAs && _serializeValueAs == YAXNodeTypes.Content)
            {
                throw new Exception("Key and Value cannot both be serialized as Content at the same time.");
            }
        }
    }

    /// <summary>
    /// Specifies the behavior of the deserialization method, if the element/attribute corresponding to this property is missed in the XML input.
    /// This attribute is applicable to fields and properties.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class YAXErrorIfMissedAttribute : YAXBaseAttribute
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="YAXErrorIfMissedAttribute"/> class.
        /// </summary>
        /// <param name="treatAs">The value indicating this situation is going to be treated as Error or Warning.</param>
        public YAXErrorIfMissedAttribute(YAXExceptionTypes treatAs)
        {
            this.TreatAs = treatAs;
            this.DefaultValue = null;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the value indicating this situation is going to be treated as Error or Warning.
        /// </summary>
        /// <value>The value indicating this situation is going to be treated as Error or Warning.</value>
        public YAXExceptionTypes TreatAs { get; set; }

        /// <summary>
        /// Gets or sets the default value for the property if the element/attribute corresponding to this property is missed in the XML input.
        /// Setting <c>null</c> means do nothing.
        /// </summary>
        /// <value>The default value.</value>
        public object DefaultValue { get; set; }

        #endregion
    }

    /// <summary>
    /// Specifies the format string provided for serializing data. The format string is the parameter 
    /// passed to the <c>ToString</c> method.
    /// If this attribute is applied to collection classes, the format, therefore, is applied to 
    /// the collection members.
    /// This attribute is applicable to fields and properties.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class YAXFormatAttribute : YAXBaseAttribute
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="YAXFormatAttribute"/> class.
        /// </summary>
        /// <param name="format">The format string.</param>
        public YAXFormatAttribute(string format)
        {
            this.Format = format;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the format string needed to serialize data. The format string is the parameter 
        /// passed to the <c>ToString</c> method.
        /// </summary>
        /// <value></value>
        public string Format { get; set; }

        #endregion
    }

    /// <summary>
    /// Specifies that a particular class, or a particular property or variable type, that is 
    /// driven from <c>IEnumerable</c> should not be treated as a collection class/object.
    /// This attribute is applicable to fields, properties, classes, and structs.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct)]
    public class YAXNotCollectionAttribute : YAXBaseAttribute
    {
    }

    /// <summary>
    /// Specifies an alias for an enum member.
    /// This attribute is applicable to enum members.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class YAXEnumAttribute : YAXBaseAttribute
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="YAXEnumAttribute"/> class.
        /// </summary>
        /// <param name="alias">The alias.</param>
        public YAXEnumAttribute(string alias)
        {
            this.Alias = alias.Trim();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the alias for the enum member.
        /// </summary>
        /// <value>The alias for the enum member.</value>
        public string Alias { get; private set; }

        #endregion
    }

    /// <summary>
    /// Specifies a custom serializer class for a field, property, class, or struct. YAXLib will instantiate an object
    /// from the specified type in this attribute, and calls appropriate methods while serializing.
    /// This attribute is applicable to fields, properties, classes, and structs.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct)]
    public class YAXCustomSerializerAttribute : YAXBaseAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="YAXCustomSerializerAttribute"/> class.
        /// </summary>
        /// <param name="customSerializerType">Type of the custom serializer.</param>
        public YAXCustomSerializerAttribute(Type customSerializerType)
        {
            this.CustomSerializerType = customSerializerType;
        }

        /// <summary>
        /// Gets or sets the type of the custom serializer.
        /// </summary>
        /// <value>The type of the custom serializer.</value>
        public Type CustomSerializerType { get; private set; }
    }

    /// <summary>
    /// Adds the attribute xml:space="preserve" to the serialized element, so that the deserializer would
    /// perserve all whitespace characters for the string values.
    /// Add this attribute to any string field that you want their whitespace be preserved during 
    /// deserialization, or add it to the containing class to be applied to all its fields and properties.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct)]
    public class YAXPreserveWhitespaceAttribute : YAXBaseAttribute
    {
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    [Obsolete("Not implemented", true)]
    public class YAXCollectionItemTypeAttribute : YAXBaseAttribute
    {
        public Type Type { get; private set; }

        public YAXCollectionItemTypeAttribute(Type type)
        {
            Type = type;
        }
    }


    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    [Obsolete("Not implemented", true)]
    public class YAXTypeAttribute : YAXBaseAttribute
    {
        public Type Type { get; private set; }

        public YAXTypeAttribute(Type type)
        {
            Type = type;
        }
    }
}
