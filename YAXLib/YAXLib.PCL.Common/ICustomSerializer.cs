// Copyright 2009 - 2010 Sina Iravanian - <sina@sinairv.com>
//
// This source file(s) may be redistributed, altered and customized
// by any means PROVIDING the authors name and all copyright
// notices remain intact.
// THIS SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED. USE IT AT YOUR OWN RISK. THE AUTHOR ACCEPTS NO
// LIABILITY FOR ANY DATA DAMAGE/LOSS THAT THIS PRODUCT MAY CAUSE.
//-----------------------------------------------------------------------

using System.Xml.Linq;

namespace YAXLib
{
    /// <summary>
    /// Defines the interface to all custom serializers and deserializers used with YAXLib.
    /// Note that normally you don't need to implement all the methods.
    /// </summary>
    /// <typeparam name="T">The type of field, property, class, or struct for which custom serializer
    /// is provided</typeparam>
    public interface ICustomSerializer<T>
    {
        /// <summary>
        /// Serializes the given object and fills the provided reference to the
        /// XML attribute appropriately. Do NOT change the name of the provided attribute.
        /// </summary>
        /// <param name="objectToSerialize">The object to serialize.</param>
        /// <param name="attrToFill">The XML attribute to fill.</param>
        void SerializeToAttribute(T objectToSerialize, XAttribute attrToFill);


        /// <summary>
        /// Serializes the given object and fills the provided reference to the
        /// XML element appropriately. Do NOT change the name of the provided element.
        /// </summary>
        /// <param name="objectToSerialize">The object to serialize.</param>
        /// <param name="elemToFill">The XML element to fill.</param>
        void SerializeToElement(T objectToSerialize, XElement elemToFill);

        /// <summary>
        /// Serializes the given object to an string to be used as a value for an
        /// XML element.
        /// </summary>
        /// <param name="objectToSerialize">The object to serialize.</param>
        /// <returns></returns>
        string SerializeToValue(T objectToSerialize);

        /// <summary>
        /// Deserializes from an xml attribute, and returns the retreived value.
        /// You will normally need to use XAttribute.Value property only.
        /// </summary>
        /// <param name="attrib">The attribute to deserialize.</param>
        /// <returns></returns>
        T DeserializeFromAttribute(XAttribute attrib);

        /// <summary>
        /// Deserializes from an xml element, and returns the retreived value.
        /// You might need to use XElement.Value, XElement.Nodes(), 
        /// XElement.Attributes(), and XElement.Elements() only.
        /// </summary>
        /// <param name="element">The element to deserialize.</param>
        /// <returns></returns>
        T DeserializeFromElement(XElement element);

        /// <summary>
        /// Deserializes from a string value which has been serialized as the content of an element
        /// </summary>
        /// <param name="value">The string value to deserialize.</param>
        /// <returns></returns>
        T DeserializeFromValue(string value);

    }
}
