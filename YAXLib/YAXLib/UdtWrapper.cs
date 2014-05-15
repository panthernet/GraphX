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
using System.Xml.Linq;

namespace YAXLib
{
    /// <summary>
    /// a wrapper class around user-defined types, for quick acess to their YAXLib related attributes
    /// </summary>
    internal class UdtWrapper
    {
        /// <summary>
        /// the underlying type for this instance of <c>TypeWrapper</c>
        /// </summary>
        private readonly Type m_udtType = typeof(object);

        /// <summary>
        /// boolean value indicating whether this instance is a wrapper around a collection type
        /// </summary>
        private readonly bool m_isTypeCollection;

        /// <summary>
        /// boolean value indicating whether this instance is a wrapper around a dictionary type
        /// </summary>
        private readonly bool m_isTypeDictionary;

        /// <summary>
        /// The collection attribute instance
        /// </summary>
        private YAXCollectionAttribute m_collectionAttributeInstance = null;

        /// <summary>
        /// the dictionary attribute instance
        /// </summary>
        private YAXDictionaryAttribute m_dictionaryAttributeInstance = null;

        /// <summary>
        /// reference to an instance of <c>EnumWrapper</c> in case that the current instance is an enum.
        /// </summary>
        private EnumWrapper m_enumWrapper;

        /// <summary>
        /// value indicating whether the serialization options has been explicitly adjusted
        /// using attributes for the class
        /// </summary>
        private bool m_isSerializationOptionSetByAttribute;

        /// <summary>
        /// Alias for the type
        /// </summary>
        private XName m_alias = null;

        /// <summary>
        /// the namespace associated with this element
        /// </summary>
        private XNamespace m_namespace = XNamespace.None;

        /// <summary>
        /// Initializes a new instance of the <see cref="UdtWrapper"/> class.
        /// </summary>
        /// <param name="udtType">The underlying type to create the wrapper around.</param>
        /// <param name="callerSerializer">reference to the serializer 
        /// instance which is building this instance.</param>
        public UdtWrapper(Type udtType, YAXSerializer callerSerializer)
        {
            m_isTypeDictionary = false;
            m_udtType = udtType;
            m_isTypeCollection = ReflectionUtils.IsCollectionType(m_udtType);
            m_isTypeDictionary = ReflectionUtils.IsIDictionary(m_udtType);

            Alias = StringUtils.RefineSingleElement(ReflectionUtils.GetTypeFriendlyName(m_udtType));
            Comment = null;
            FieldsToSerialize = YAXSerializationFields.PublicPropertiesOnly;
            IsAttributedAsNotCollection = false;

            SetYAXSerializerOptions(callerSerializer);

            foreach (var attr in m_udtType.GetCustomAttributes(true))
            {
                if (attr is YAXBaseAttribute)
                    ProcessYAXAttribute(attr);
            }
        }

        /// <summary>
        /// Gets the alias of the type.
        /// </summary>
        /// <value>The alias of the type.</value>
        public XName Alias 
        {
            get
            {
                return m_alias;
            }

            private set
            {
                if (Namespace.IsEmpty())
                    m_alias = Namespace + value.LocalName;
                else
                {
                    m_alias = value;
                    if (m_alias.Namespace.IsEmpty())
                        m_namespace = m_alias.Namespace;
                }
            }
        }

        /// <summary>
        /// Gets an array of comments for the underlying type.
        /// </summary>
        /// <value>The array of comments for the underlying type.</value>
        public string[] Comment { get; private set; }

        /// <summary>
        /// Gets the fields to be serialized.
        /// </summary>
        /// <value>The fields to be serialized.</value>
        public YAXSerializationFields FieldsToSerialize { get; private set; }

        /// <summary>
        /// Gets the serialization options.
        /// </summary>
        /// <value>The serialization options.</value>
        public YAXSerializationOptions SerializationOption { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is attributed as not collection.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is attributed as not collection; otherwise, <c>false</c>.
        /// </value>
        public bool IsAttributedAsNotCollection { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance has comment.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has comment; otherwise, <c>false</c>.
        /// </value>
        public bool HasComment
        {
            get
            {
                return this.Comment != null && this.Comment.Length > 0;
            }
        }

        /// <summary>
        /// Gets the underlying type corresponding to this wrapper.
        /// </summary>
        /// <value>The underlying type corresponding to this wrapper.</value>
        public Type UnderlyingType
        {
            get
            {
                return m_udtType;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the underlying type is a known-type
        /// </summary>
        public bool IsKnownType
        {
            get { return KnownTypes.IsKnowType(m_udtType); }
        }

        /// <summary>
        /// Gets a value indicating whether this instance wraps around an enum.
        /// </summary>
        /// <value><c>true</c> if this instance wraps around an enum; otherwise, <c>false</c>.</value>
        public bool IsEnum
        {
            get
            {
                return m_udtType.IsEnum;
            }
        }

        /// <summary>
        /// Gets the enum wrapper, provided that this instance wraps around an enum.
        /// </summary>
        /// <value>The enum wrapper, provided that this instance wraps around an enum.</value>
        public EnumWrapper EnumWrapper
        {
            get
            {
                if (IsEnum)
                {
                    if (m_enumWrapper == null)
                        m_enumWrapper = new EnumWrapper(m_udtType);

                    return m_enumWrapper;
                }

                return null;
            }
        }

        /// <summary>
        /// Determines whether serialization of null objects is not allowd.
        /// </summary>
        /// <returns>
        /// <c>true</c> if serialization of null objects is not allowd; otherwise, <c>false</c>.
        /// </returns>
        public bool IsNotAllowdNullObjectSerialization
        {
            get
            {
                return this.SerializationOption == YAXSerializationOptions.DontSerializeNullObjects;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance wraps around a collection type.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance wraps around a collection type; otherwise, <c>false</c>.
        /// </value>
        public bool IsCollectionType
        {
            get
            {
                return m_isTypeCollection;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance wraps around a dictionary type.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance wraps around a dictionary type; otherwise, <c>false</c>.
        /// </value>
        public bool IsDictionaryType
        {
            get
            {
                return m_isTypeDictionary;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is treated as collection.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is treated as collection; otherwise, <c>false</c>.
        /// </value>
        public bool IsTreatedAsCollection
        {
            get
            {
                return (!IsAttributedAsNotCollection && IsCollectionType);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is treated as dictionary.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is treated as dictionary; otherwise, <c>false</c>.
        /// </value>
        public bool IsTreatedAsDictionary
        {
            get
            {
                return !IsAttributedAsNotCollection && IsDictionaryType;
            }
        }

        /// <summary>
        /// Gets the collection attribute instance.
        /// </summary>
        /// <value>The collection attribute instance.</value>
        public YAXCollectionAttribute CollectionAttributeInstance
        {
            get
            {
                return m_collectionAttributeInstance;
            }
        }

        /// <summary>
        /// Gets the dictionary attribute instance.
        /// </summary>
        /// <value>The dictionary attribute instance.</value>
        public YAXDictionaryAttribute DictionaryAttributeInstance
        {
            get
            {
                return m_dictionaryAttributeInstance;
            }
        }

        /// <summary>
        /// Gets or sets the type of the custom serializer.
        /// </summary>
        /// <value>The type of the custom serializer.</value>
        public Type CustomSerializerType { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance has custom serializer.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has custom serializer; otherwise, <c>false</c>.
        /// </value>
        public bool HasCustomSerializer
        {
            get
            {
                return CustomSerializerType != null;
            }
        }

        public bool PreservesWhitespace { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance has a custom namespace
        /// defined for it through the <see cref="YAXNamespaceAttribute"/> attribute.
        /// </summary>
        public bool HasNamespace 
        {
            get
            {
                return Namespace.IsEmpty();
            }
        }

        /// <summary>
        /// Gets the namespace associated with this element.
        /// </summary>
        /// <remarks>
        /// If <see cref="HasNamespace"/> is <c>false</c> then this should
        /// be inherited from any parent elements.
        /// </remarks>
        public XNamespace Namespace 
        {
            get
            {
                return m_namespace;
            }

            private set
            {
                m_namespace = value;
                // explicit namespace definition overrides namespace definitions in SerializeAs attributes.
                m_alias = m_namespace + m_alias.LocalName;

            }
        }

        /// <summary>
        /// Gets the namespace prefix associated with this element
        /// </summary>
        /// <remarks>
        /// If <see cref="HasNamespace"/> is <c>false</c> then this should
        /// be inherited from any parent elements.
        /// If this is <c>null</c>, then it should be assumed that the specified
        /// <see cref="Namespace"/> (if it is present) is the default namespace.
        /// 
        /// It should also be noted that if a namespace is not provided for the
        /// entire document (default namespace) and yet a default namespace is
        /// provided for one element that an exception should be thrown (since
        /// setting a default namespace for that element would make it apply to
        /// the whole document).
        /// </remarks>
        public string NamespacePrefix { get; private set; }


        /// <summary>
        /// Sets the serializer options.
        /// </summary>
        /// <param name="caller">The caller serializer.</param>
        public void SetYAXSerializerOptions(YAXSerializer caller)
        {
            if (!m_isSerializationOptionSetByAttribute)
            {
                SerializationOption = caller != null ? caller.SerializationOption : YAXSerializationOptions.SerializeNullObjects;
            }
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            return m_udtType.ToString();
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>.</param>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.NullReferenceException">
        /// The <paramref name="obj"/> parameter is null.
        /// </exception>
        public override bool Equals(object obj)
        {
            if (obj is UdtWrapper)
            {
                var other = obj as UdtWrapper;
                return m_udtType == other.m_udtType;
            }

            return false;
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            return m_udtType.GetHashCode();
        }

        /// <summary>
        /// Processes the specified attribute.
        /// </summary>
        /// <param name="attr">The attribute to process.</param>
        private void ProcessYAXAttribute(object attr)
        {
            if (attr is YAXCommentAttribute)
            {
                string comment = (attr as YAXCommentAttribute).Comment;
                if(!String.IsNullOrEmpty(comment))
                {
                    string[] comments = comment.Split(new [] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    for(int i = 0; i < comments.Length; i++)
                    {
                        comments[i] = String.Format(" {0} ", comments[i].Trim());
                    }

                    this.Comment = comments;
                }
            }
            else if (attr is YAXSerializableTypeAttribute)
            {
                var theAttr = attr as YAXSerializableTypeAttribute;
                this.FieldsToSerialize = theAttr.FieldsToSerialize;
                if (theAttr.IsSerializationOptionSet())
                {
                    SerializationOption = theAttr.Options;
                    m_isSerializationOptionSetByAttribute = true;
                }
            }
            else if (attr is YAXSerializeAsAttribute)
            {
                Alias = StringUtils.RefineSingleElement((attr as YAXSerializeAsAttribute).SerializeAs);
            }
            else if (attr is YAXNotCollectionAttribute)
            {
                if(!ReflectionUtils.IsArray(m_udtType))
                    IsAttributedAsNotCollection = true;
            }
            else if (attr is YAXCustomSerializerAttribute)
            {
                Type serType = (attr as YAXCustomSerializerAttribute).CustomSerializerType;

                Type genTypeArg;
                bool isDesiredInterface = ReflectionUtils.IsDerivedFromGenericInterfaceType(serType, typeof(ICustomSerializer<>), out genTypeArg);

                if (!isDesiredInterface)
                {
                    throw new YAXException("The provided custom serialization type is not derived from the proper interface");
                }

                if (genTypeArg != UnderlyingType)
                {
                    throw new YAXException("The generic argument of the class and the type of the class do not match");
                }
                
                this.CustomSerializerType = serType;
            }
            else if(attr is YAXPreserveWhitespaceAttribute)
            {
                PreservesWhitespace = true;
            }
            else if (attr is YAXNamespaceAttribute)
            {
                var nsAttrib = (attr as YAXNamespaceAttribute);
                Namespace = nsAttrib.Namespace;
                NamespacePrefix = nsAttrib.Prefix;
            }
            else if (attr is YAXCollectionAttribute)
            {
                m_collectionAttributeInstance = attr as YAXCollectionAttribute;
            }
            else if (attr is YAXDictionaryAttribute)
            {
                m_dictionaryAttributeInstance = attr as YAXDictionaryAttribute;
            }
            else
            {
                throw new Exception("Attribute not applicable to types!");
            }
        }
    }
}
