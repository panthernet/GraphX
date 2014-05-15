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
using System.Globalization;

namespace YAXLib
{
    /// <summary>
    /// The base for all exception classes of YAXLib
    /// </summary>
    public class YAXException : System.Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="YAXException"/> class.
        /// </summary>
        public YAXException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="YAXException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public YAXException(string message) 
            : base(message)
        {
            
        }
    }

    /// <summary>
    /// Raised when the location of serialization specified cannot be
    /// created or cannot be read from.
    /// This exception is raised during serialization and deserialization
    /// </summary>
    public class YAXBadLocationException : YAXException
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="YAXBadLocationException"/> class.
        /// </summary>
        /// <param name="location">The location.</param>
        public YAXBadLocationException(string location)
        {

        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the bad location which caused the exception
        /// </summary>
        /// <value>The location.</value>
        public string Location { get; private set; }

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// The error message that explains the reason for the exception, or an empty string("").
        /// </returns>
        public override string Message
        {
            get
            {
                return String.Format("The location specified cannot be read from or written to: {0}", this.Location);
            }
        }

        #endregion
    }

    /// <summary>
    /// Raised when trying to serialize an attribute where 
    /// another attribute with the same name already exists.
    /// This exception is raised during serialization.
    /// </summary>
    public class YAXAttributeAlreadyExistsException : YAXException
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="YAXAttributeAlreadyExistsException"/> class.
        /// </summary>
        /// <param name="attrName">Name of the attribute.</param>
        public YAXAttributeAlreadyExistsException(string attrName)
        {
            this.AttrName = attrName;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the name of the attribute.
        /// </summary>
        /// <value>The name of the attribute.</value>
        public string AttrName { get; private set; }

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// The error message that explains the reason for the exception, or an empty string("").
        /// </returns>
        public override string Message
        {
            get
            {
                return String.Format(CultureInfo.CurrentCulture, "An attribute with this name already exists: '{0}'.", this.AttrName);
            }
        }

        #endregion
    }

    /// <summary>
    /// Raised when the attribute corresponding to some property is not present in the given XML file, when deserializing.
    /// This exception is raised during deserialization.
    /// </summary>
    public class YAXAttributeMissingException : YAXException
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="YAXAttributeMissingException"/> class.
        /// </summary>
        /// <param name="attrName">Name of the attribute.</param>
        public YAXAttributeMissingException(string attrName)
        {
            this.AttributeName = attrName;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the name of the attribute.
        /// </summary>
        /// <value>The name of the attribute.</value>
        public string AttributeName { get; private set; }

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// The error message that explains the reason for the exception, or an empty string("").
        /// </returns>
        public override string Message
        {
            get
            {
                return String.Format(CultureInfo.CurrentCulture, "No attributes with this name found: '{0}'.", this.AttributeName);
            }
        }

        #endregion
    }

    /// <summary>
    /// Raised when the element value corresponding to some property is not present in the given XML file, when deserializing.
    /// This exception is raised during deserialization.
    /// </summary>
    public class YAXElementValueMissingException : YAXException
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="YAXAttributeMissingException"/> class.
        /// </summary>
        /// <param name="elementName">Name of the element.</param>
        public YAXElementValueMissingException(string elementName)
        {
            this.ElementName = elementName;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the name of the attribute.
        /// </summary>
        /// <value>The name of the attribute.</value>
        public string ElementName { get; private set; }

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// The error message that explains the reason for the exception, or an empty string("").
        /// </returns>
        public override string Message
        {
            get
            {
                return String.Format(CultureInfo.CurrentCulture, "Element with the given name does not contain text values: '{0}'.", this.ElementName);
            }
        }

        #endregion
    }

    /// <summary>
    /// Raised when the element value corresponding to some property is not present in the given XML file, when deserializing.
    /// This exception is raised during deserialization.
    /// </summary>
    public class YAXElementValueAlreadyExistsException : YAXException
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="YAXAttributeMissingException"/> class.
        /// </summary>
        /// <param name="elementName">Name of the element.</param>
        public YAXElementValueAlreadyExistsException(string elementName)
        {
            this.ElementName = elementName;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the name of the attribute.
        /// </summary>
        /// <value>The name of the attribute.</value>
        public string ElementName { get; private set; }

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// The error message that explains the reason for the exception, or an empty string("").
        /// </returns>
        public override string Message
        {
            get
            {
                return String.Format(CultureInfo.CurrentCulture, "Element with the given name already has value: '{0}'.", this.ElementName);
            }
        }

        #endregion
    }


    /// <summary>
    /// Raised when the element corresponding to some property is not present in the given XML file, when deserializing.
    /// This exception is raised during deserialization.
    /// </summary>
    public class YAXElementMissingException : YAXException
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="YAXElementMissingException"/> class.
        /// </summary>
        /// <param name="elemName">Name of the element.</param>
        public YAXElementMissingException(string elemName)
        {
            this.ElementName = elemName;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or the name of the element.
        /// </summary>
        /// <value>The name of the element.</value>
        public string ElementName { get; private set; }

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// The error message that explains the reason for the exception, or an empty string("").
        /// </returns>
        public override string Message
        {
            get
            {
                return String.Format(CultureInfo.CurrentCulture, "No elements with this name found: '{0}'.", this.ElementName);
            }
        }

        #endregion
    }

    /// <summary>
    /// Raised when the value provided for some property in the XML input, cannot be 
    /// converted to the type of the property.
    /// This exception is raised during deserialization.
    /// </summary>
    public class YAXBadlyFormedInput : YAXException
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="YAXBadlyFormedInput"/> class.
        /// </summary>
        /// <param name="elemName">Name of the element.</param>
        /// <param name="badInput">The value of the input which could not be converted to the type of the property.</param>
        public YAXBadlyFormedInput(string elemName, string badInput)
        {
            this.ElementName = elemName;
            this.BadInput = badInput;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the name of the element.
        /// </summary>
        /// <value>The name of the element.</value>
        public string ElementName { get; private set; }

        /// <summary>
        /// Gets the value of the input which could not be converted to the type of the property.
        /// </summary>
        /// <value>The value of the input which could not be converted to the type of the property.</value>
        public string BadInput { get; private set; }

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// The error message that explains the reason for the exception, or an empty string("").
        /// </returns>
        public override string Message
        {
            get
            {
                return String.Format(
                    CultureInfo.CurrentCulture,
                    "The format of the value specified for the property '{0}' is not proper: '{1}'.",
                    this.ElementName,
                    this.BadInput);
            }
        }

        #endregion
    }

    /// <summary>
    /// Raised when the value provided for some property in the XML input, cannot be 
    /// assigned to the property.
    /// This exception is raised during deserialization.
    /// </summary>
    public class YAXPropertyCannotBeAssignedTo : YAXException
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="YAXPropertyCannotBeAssignedTo"/> class.
        /// </summary>
        /// <param name="propName">Name of the property.</param>
        public YAXPropertyCannotBeAssignedTo(string propName)
        {
            this.PropertyName = propName;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        /// <value>The name of the property.</value>
        public string PropertyName { get; private set; }

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// The error message that explains the reason for the exception, or an empty string("").
        /// </returns>
        public override string Message
        {
            get
            {
                return String.Format(CultureInfo.CurrentCulture, "Could not assign to the property '{0}'.", this.PropertyName);
            }
        }

        #endregion
    }

    /// <summary>
    /// Raised when some member of the collection in the input XML, cannot be added to the collection object.
    /// This exception is raised during deserialization.
    /// </summary>
    public class YAXCannotAddObjectToCollection : YAXException
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="YAXCannotAddObjectToCollection"/> class.
        /// </summary>
        /// <param name="propName">Name of the property.</param>
        /// <param name="obj">The object that could not be added to the collection.</param>
        public YAXCannotAddObjectToCollection(string propName, object obj)
        {
            this.PropertyName = propName;
            this.ObjectToAdd = obj;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        /// <value>The name of the property.</value>
        public string PropertyName { get; private set; }

        /// <summary>
        /// Gets the object that could not be added to the collection.
        /// </summary>
        /// <value>the object that could not be added to the collection.</value>
        public object ObjectToAdd { get; private set; }

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// The error message that explains the reason for the exception, or an empty string("").
        /// </returns>
        public override string Message
        {
            get
            {
                return String.Format(
                    CultureInfo.CurrentCulture,
                    "Could not add object ('{0}') to the collection ('{1}').",
                    this.ObjectToAdd,
                    this.PropertyName);
            }
        }

        #endregion
    }

    /// <summary>
    /// Raised when the default value specified by the <c>YAXErrorIfMissedAttribute</c> could not be assigned to the property.
    /// This exception is raised during deserialization.
    /// </summary>
    public class YAXDefaultValueCannotBeAssigned : YAXException
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="YAXDefaultValueCannotBeAssigned"/> class.
        /// </summary>
        /// <param name="propName">Name of the property.</param>
        /// <param name="defaultValue">The default value which caused the problem.</param>
        public YAXDefaultValueCannotBeAssigned(string propName, object defaultValue)
        {
            this.PropertyName = propName;
            this.TheDefaultValue = defaultValue;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        /// <value>The name of the property.</value>
        public string PropertyName { get; private set; }

        /// <summary>
        /// Gets the default value which caused the problem.
        /// </summary>
        /// <value>The default value which caused the problem.</value>
        public object TheDefaultValue { get; private set; }

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// The error message that explains the reason for the exception, or an empty string("").
        /// </returns>
        public override string Message
        {
            get
            {
                return String.Format(
                    CultureInfo.CurrentCulture,
                    "Could not assign the default value specified ('{0}') for the property '{1}'.",
                    this.TheDefaultValue,
                    this.PropertyName);
            }
        }

        #endregion
    }

    /// <summary>
    /// Raised when the XML input does not follow standard XML formatting rules.
    /// This exception is raised during deserialization.
    /// </summary>
    public class YAXBadlyFormedXML : YAXException
    {
        #region Fields

        /// <summary>
        /// The inner exception.
        /// </summary>
        private Exception innerException;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="YAXBadlyFormedXML"/> class.
        /// </summary>
        /// <param name="innerException">The inner exception.</param>
        public YAXBadlyFormedXML(Exception innerException)
        {
            this.innerException = innerException;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="YAXBadlyFormedXML"/> class.
        /// </summary>
        public YAXBadlyFormedXML()
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// The error message that explains the reason for the exception, or an empty string("").
        /// </returns>
        public override string Message
        {
            get
            {
                string msg = "The input xml file is not properly formatted!";

                if (this.innerException != null)
                {
                    msg += String.Format(CultureInfo.CurrentCulture, "\r\nMore Details:\r\n{0}", this.innerException.Message);
                }

                return msg;
            }
        }

        #endregion
    }

    /// <summary>
    /// Raised when an object cannot be formatted with the format string provided.
    /// This exception is raised during serialization.
    /// </summary>
    public class YAXInvalidFormatProvided : YAXException
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="YAXInvalidFormatProvided"/> class.
        /// </summary>
        /// <param name="objectType">Type of the fiedl to serialize.</param>
        /// <param name="format">The format string.</param>
        public YAXInvalidFormatProvided(Type objectType, string format)
        {
            this.ObjectType = objectType;
            this.Format = format;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the type of the field to serialize
        /// </summary>
        /// <value>The type of the field to serialize.</value>
        public Type ObjectType { get; private set; }

        /// <summary>
        /// Gets the format string.
        /// </summary>
        /// <value>The format string.</value>
        public string Format { get; private set; }

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        /// <returns>
        /// The error message that explains the reason for the exception, or an empty string("").
        /// </returns>
        public override string Message
        {
            get
            {
                return String.Format(
                    CultureInfo.CurrentCulture,
                    "Could not format objects of type '{0}' with the format string '{1}'",
                    this.ObjectType.Name,
                    this.Format);
            }
        }

        #endregion
    }

    /// <summary>
    /// Raised when trying to serialize self-referential types. This exception cannot be turned off.
    /// This exception is raised during deserialization.
    /// </summary>
    public class YAXCannotSerializeSelfReferentialTypes : YAXException
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="YAXCannotSerializeSelfReferentialTypes"/> class.
        /// </summary>
        /// <param name="type">The the self-referential type that caused the problem.</param>
        public YAXCannotSerializeSelfReferentialTypes(Type type)
        {
            this.SelfReferentialType = type;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the self-referential type that caused the problem.
        /// </summary>
        /// <value>The type of the self-referential type that caused the problem.</value>
        public Type SelfReferentialType { get; private set; }

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        /// <returns>
        /// The error message that explains the reason for the exception, or an empty string("").
        /// </returns>
        public override string Message
        {
            get
            {
                return String.Format(CultureInfo.CurrentCulture, "Self Referential types ('{0}') cannot be serialized.", this.SelfReferentialType.FullName);
            }
        }

        #endregion
    }

    /// <summary>
    /// Raised when the object provided for serialization is not of the type provided for the serializer. This exception cannot be turned off.
    /// This exception is raised during serialization.
    /// </summary>
    public class YAXObjectTypeMismatch : YAXException
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="YAXObjectTypeMismatch"/> class.
        /// </summary>
        /// <param name="expectedType">The expected type.</param>
        /// <param name="receivedType">The type of the object which did not match the expected type.</param>
        public YAXObjectTypeMismatch(Type expectedType, Type receivedType)
        {
            this.ExpectedType = expectedType;
            this.ReceivedType = receivedType;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the expected type.
        /// </summary>
        /// <value>The expected type.</value>
        public Type ExpectedType { get; private set; }

        /// <summary>
        /// Gets the type of the object which did not match the expected type.
        /// </summary>
        /// <value>The type of the object which did not match the expected type.</value>
        public Type ReceivedType { get; private set; }

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        /// <returns>
        /// The error message that explains the reason for the exception, or an empty string("").
        /// </returns>
        public override string Message
        {
            get
            {
                return String.Format(
                   CultureInfo.CurrentCulture,
                   "Expected an object of type '{0}' but received an object of type '{1}'.",
                   this.ExpectedType.Name,
                   this.ReceivedType.Name);
            }
        }

        #endregion
    }
}
