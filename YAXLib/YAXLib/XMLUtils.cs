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
using System.Globalization;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace YAXLib
{
    /// <summary>
    /// Provides utility methods for manipulating XML.
    /// There are four methods for each unit. UnitExists, FindUnit, CanCreateUnit, CreateUnit
    /// Units are: Location, Element, and Attribute
    /// </summary>
    internal static class XMLUtils
    {
        /// <summary>
        /// Determines whether the location specified exists in the given XML element.
        /// </summary>
        /// <param name="baseElement">The XML element.</param>
        /// <param name="location">The location string.</param>
        /// <returns>a value indicating whether the location specified exists in the given XML element</returns>
        public static bool LocationExists(XElement baseElement, string location)
        {
            XElement newLoc = FindLocation(baseElement, location);
            return newLoc != null;
        }

        /// <summary>
        /// Finds the location specified in the given XML element specified.
        /// </summary>
        /// <param name="baseElement">The XML element.</param>
        /// <param name="location">The location string.</param>
        /// <returns>the XML element corresponding to the specified location, or <c>null</c> if it is not found</returns>
        public static XElement FindLocation(XElement baseElement, string location)
        {
            if (baseElement == null || location == null)
                throw new ArgumentNullException();

            var locSteps = location.SplitPathNamespaceSafe();

            XElement currentLocation = baseElement;
            foreach (string loc in locSteps)
            {
                if (loc == ".")
                {
                    continue;
                }
                else if (loc == "..")
                {
                    currentLocation = currentLocation.Parent;
                    if (currentLocation == null)
                        break;
                }
                else
                {
                    XName curLocName = loc;
                    if (curLocName.Namespace.IsEmpty())
                        currentLocation = currentLocation.Element(curLocName);
                    else
                        currentLocation = currentLocation.Element_NamespaceNeutral(curLocName);

                    if (currentLocation == null)
                        break;
                }
            }

            return currentLocation;
        }

        /// <summary>
        /// Determines whether the specified location can be created in the specified XML element.
        /// </summary>
        /// <param name="baseElement">The XML element.</param>
        /// <param name="location">The location string.</param>
        /// <returns>
        /// <c>true</c> if the specified location can be created in the specified XML element; otherwise, <c>false</c>.
        /// </returns>
        public static bool CanCreateLocation(XElement baseElement, string location)
        {
            var locSteps = location.SplitPathNamespaceSafe();

            XElement currentLocation = baseElement;
            foreach (string loc in locSteps)
            {
                if (loc == ".")
                {
                    continue;
                }
                else if (loc == "..")
                {
                    currentLocation = currentLocation.Parent;
                    if (currentLocation == null)
                        return false;
                }
                else
                {
                    XName curLocName = loc;
                    if (curLocName.Namespace.IsEmpty())
                        currentLocation = currentLocation.Element(curLocName);
                    else
                        currentLocation = currentLocation.Element_NamespaceNeutral(curLocName);

                    if (currentLocation == null)
                        return true;
                }
            }

            return true;
        }

        /// <summary>
        /// Creates and returns XML element corresponding to the sepcified location in the given XML element.
        /// </summary>
        /// <param name="baseElement">The XML element.</param>
        /// <param name="location">The location string.</param>
        /// <returns>XML element corresponding to the sepcified location created in the given XML element</returns>
        public static XElement CreateLocation(XElement baseElement, string location)
        {
            var locSteps = location.SplitPathNamespaceSafe();

            XElement currentLocation = baseElement;
            foreach (string loc in locSteps)
            {
                if (loc == ".")
                {
                    continue;
                }
                else if (loc == "..")
                {
                    currentLocation = currentLocation.Parent;
                    if (currentLocation == null)
                        break;
                }
                else
                {
                    XName curLocName = loc;
                    XElement newLoc;
                    if (curLocName.Namespace.IsEmpty())
                        newLoc = currentLocation.Element(curLocName);
                    else
                        newLoc = currentLocation.Element_NamespaceNeutral(curLocName);

                    if (newLoc == null)
                    {
                        var newElem = new XElement(curLocName.OverrideNsIfEmpty(currentLocation.Name.Namespace));
                        currentLocation.Add(newElem);
                        currentLocation = newElem;
                    }
                    else
                    {
                        currentLocation = newLoc;
                    }
                }
            }

            return currentLocation;
        }

        /// <summary>
        /// Determines whether the attribute with the given name located in the given location string exists in the given XML element.
        /// </summary>
        /// <param name="baseElement">The XML element.</param>
        /// <param name="location">The location string.</param>
        /// <param name="attrName">Name of the attribute.</param>
        /// <returns>
        /// a value indicating whether the attribute with the given name located in the given location string exists in the given XML element.
        /// </returns>
        public static bool AttributeExists(XElement baseElement, string location, XName attrName)
        {
            return FindAttribute(baseElement, location, attrName) != null;
        }

        /// <summary>
        /// Finds the attribute with the given name located in the given location string in the given XML element.
        /// </summary>
        /// <param name="baseElement">The XML element.</param>
        /// <param name="location">The location string.</param>
        /// <param name="attrName">Name of the attribute.</param>
        /// <returns>a value indicating whether the attribute with the given name located in 
        /// the given location string in the given XML element has been found.</returns>
        public static XAttribute FindAttribute(XElement baseElement, string location, XName attrName)
        {
            XElement newLoc = FindLocation(baseElement, location);
            if (newLoc == null)
                return null;

            var newAttrName = attrName;
            // the following stupid code is because of odd behaviour of LINQ to XML
            if (newAttrName.Namespace == newLoc.Name.Namespace)
                newAttrName = newAttrName.RemoveNamespace();

            if (newAttrName.Namespace.IsEmpty())
                return newLoc.Attribute(newAttrName);
            else
                return newLoc.Attribute_NamespaceNeutral(newAttrName);
        }

        /// <summary>
        /// Determines whether the attribute with the given name can be created in the location 
        /// specified by the given location string in the given XML element.
        /// </summary>
        /// <param name="baseElement">The XML element.</param>
        /// <param name="location">The location string.</param>
        /// <param name="attrName">Name of the attribute.</param>
        /// <returns>
        /// <c>true</c> if the attribute with the given name can be created in the location 
        /// specified by the given location string in the given XML element; otherwise, <c>false</c>.
        /// </returns>
        public static bool CanCreateAttribute(XElement baseElement, string location, XName attrName)
        {
            XElement newLoc = FindLocation(baseElement, location);
            if (newLoc == null) //if the location does not exist
            {
                if (CanCreateLocation(baseElement, location)) // see if you can create the location
                {
                    // if you can create the location you can create the attribute too
                    return true;
                }
                else
                {
                    // if you can't create the location you can't create the attribute either
                    return false;
                }
            }

            var newAttrName = attrName;
            // the following stupid code is because of odd behaviour of LINQ to XML
            if (newAttrName.Namespace == newLoc.Name.Namespace)
                newAttrName = newAttrName.RemoveNamespace();

            // check if the attribute does not exist
            if (newAttrName.Namespace.IsEmpty())
                return newLoc.Attribute(newAttrName) == null;
            else
                return newLoc.Attribute_NamespaceNeutral(newAttrName) == null;
        }

        /// <summary>
        /// Creates and returns the attribute with the given name in the location 
        /// specified by the given location string in the given XML element.
        /// </summary>
        /// <param name="baseElement">The XML element.</param>
        /// <param name="location">The location string.</param>
        /// <param name="attrName">Name of the attribute.</param>
        /// <param name="attrValue">The value to be assigned to the attribute.</param>
        /// <returns>returns the attribute with the given name in the location 
        /// specified by the given location string in the given XML element.</returns>
        public static XAttribute CreateAttribute(XElement baseElement, string location, XName attrName, object attrValue, XNamespace documentDefaultNamespace)
        {
            XElement newLoc = FindLocation(baseElement, location);
            if (newLoc == null)
            {
                if (CanCreateLocation(baseElement, location))
                {
                    newLoc = CreateLocation(baseElement, location);
                }
                else
                {
                    return null;
                }
            }

            // check if the attribute does not exist
            if (attrName.Namespace.IsEmpty() && attrName.Namespace != documentDefaultNamespace)
            {
                // i.e., the attribute already exists 
                if (newLoc.Attribute(attrName) != null)
                    return null; // we cannot create another one with the same name
            }
            else
            {
                if (newLoc.Attribute_NamespaceNeutral(attrName) != null) // i.e., the attribute already exists
                    return null; // we cannot create another one with the same name
            }

            return newLoc.AddAttributeNamespaceSafe(attrName, attrValue, documentDefaultNamespace);
        }

        /// <summary>
        /// Finds the element with the given name located in the given location string in the given XML element.
        /// </summary>
        /// <param name="baseElement">The XML element.</param>
        /// <param name="location">The location string.</param>
        /// <param name="elemName">Name of the element.</param>
        /// <returns>a value indicating whether the element with the given name located in 
        /// the given location string in the given XML element has been found</returns>
        public static XElement FindElement(XElement baseElement, string location, XName elemName)
        {
            return FindLocation(baseElement, StringUtils.CombineLocationAndElementName(location, elemName));
        }

        /// <summary>
        /// Determines whether the XML element with the given name located in the 
        /// given location string in the given XML element exists.
        /// </summary>
        /// <param name="baseElement">The XML element.</param>
        /// <param name="location">The location string.</param>
        /// <param name="elemName">Name of the element.</param>
        /// <returns>a value indicating whether the XML element with the given name located in the 
        /// given location string in the given XML element exists</returns>
        public static bool ElementExists(XElement baseElement, string location, XName elemName)
        {
            return FindElement(baseElement, location, elemName) != null;
        }

        /// <summary>
        /// Determines whether the XML element with the given name located in the 
        /// given location string in the given XML element can be created
        /// </summary>
        /// <param name="baseElement">The XML element.</param>
        /// <param name="location">The location string.</param>
        /// <param name="elemName">Name of the element.</param>
        /// <returns>
        /// <c>true</c> if the XML element with the given name located in the given 
        /// location string in the given XML element can be created; otherwise, <c>false</c>.
        /// </returns>
        public static bool CanCreateElement(XElement baseElement, string location, XName elemName)
        {
            return CanCreateLocation(baseElement, StringUtils.CombineLocationAndElementName(location, elemName));
        }

        /// <summary>
        /// Creates and returns the XML element with the given name located in the 
        /// given location string in the given XML element.
        /// </summary>
        /// <param name="baseElement">The parent XML element.</param>
        /// <param name="location">The location string.</param>
        /// <param name="elemName">Name of the element to create.</param>
        /// <returns>returns the XML element with the given name located in the 
        /// given location string in the given XML element</returns>
        public static XElement CreateElement(XElement baseElement, string location, XName elemName)
        {
            return CreateLocation(baseElement, StringUtils.CombineLocationAndElementName(location, elemName));
        }

        /// <summary>
        /// Creates and returns the XML element with the given name located in the 
        /// given location string in the given XML element.
        /// </summary>
        /// <param name="baseElement">The parent XML element.</param>
        /// <param name="location">The location string.</param>
        /// <param name="elemName">Name of the element to create.</param>
        /// <param name="elemValue">The element value to be assigned to the created element.</param>
        /// <returns>returns the XML element with the given name located in the 
        /// given location string in the given XML element.</returns>
        public static XElement CreateElement(XElement baseElement, string location, XName elemName, object elemValue)
        {
            XElement elem = CreateElement(baseElement, location, elemName);
            if (elem != null)
                elem.SetValue(elemValue);
            return elem;
        }

        /// <summary>
        /// Moves all the children of src (including all its elements and attributes) to the 
        /// destination element, dst.
        /// </summary>
        /// <param name="src">The source element.</param>
        /// <param name="dst">The destination element.</param>
        public static void MoveDescendants(XElement src, XElement dst)
        {
            foreach (XAttribute attr in src.Attributes())
            {
                if (dst.Attribute(attr.Name) != null)
                {
                    throw new YAXAttributeAlreadyExistsException(attr.Name.ToString());
                }

                dst.Add(attr);
            }

            foreach (XNode elem in src.Nodes())
            {
                dst.Add(elem);
            }
        }

        /// <summary>
        /// Determines whether the specified element has neither any child attributes nor any child elements.
        /// </summary>
        /// <param name="elem">The element.</param>
        /// <returns>
        /// <c>true</c> if the specified element has neither any child attributes nor any child elements; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsElementCompletelyEmpty(XElement elem)
        {
            return !elem.HasAttributes && !elem.HasElements && elem.IsEmpty;
        }

        /// <summary>
        /// Decodes the XML escape sequences into normal string
        /// </summary>
        /// <param name="str">The string to decode.</param>
        /// <returns></returns>
        public static string DecodeXMLString(string str)
        {
            if (str.IndexOf('&') >= 0)
            {
                return str.Replace("&lt;", "<").Replace("&gt;", ">").Replace("&quot;", "\"").Replace("&apos;", "'").Replace("&amp;", "&");
                // Make sure that &amp; is the final replace so that sequences such as &amp;gt; do not get corrupted
            }

            return str;
        }

        /// <summary>
        /// Adds the 'xml:space="preserve"' attribute to the specified element.
        /// </summary>
        /// <param name="element">Element to add the 'xml:space="preserve"' attribute to</param>
        /// <returns></returns>
        public static XElement AddPreserveSpaceAttribute(XElement element)
        {
            element.AddAttributeNamespaceSafe(XNamespace.Xml + "space", "preserve", null);
            return element;
        }
        
        public static string GetRandomPrefix(this XElement self)
        {
            var q = self.Attributes().Where(xa => xa.Name.Namespace == XNamespace.Xmlns).Select(xa => xa.Name.LocalName).ToArray();
            var setPrefixes = new HashSet<string>(q);

            string prefix = "p";

            for(int i = 1; i <= 10000; i++)
            {
                if (!setPrefixes.Contains(prefix + i))
                    return prefix + i;
            }

            throw new InvalidOperationException("Cannot create a unique random prefix");
        }

        public static string ToXmlValue(this object self)
        {
            return Convert.ToString((self ?? String.Empty), CultureInfo.InvariantCulture);
        }

        public static XAttribute AddAttributeNamespaceSafe(this XElement parent, XName attrName, object attrValue, XNamespace documentDefaultNamespace)
        {
            var newAttrName = attrName;

            if (newAttrName.Namespace == documentDefaultNamespace)
                    newAttrName = newAttrName.RemoveNamespace();

            var newAttr = new XAttribute(newAttrName, attrValue.ToXmlValue());
            parent.Add(newAttr);
            return newAttr;
        }

        public static XAttribute Attribute_NamespaceSafe(this XElement parent, XName attrName, XNamespace documentDefaultNamespace)
        {
            if(attrName.Namespace == documentDefaultNamespace)
                attrName = attrName.RemoveNamespace();
            return parent.Attribute(attrName);
        }

        public static IEnumerable<XAttribute> Attributes_NamespaceSafe(this XElement parent, XName attrName, XNamespace documentDefaultNamespace)
        {
            if (attrName.Namespace == documentDefaultNamespace)
                attrName = attrName.RemoveNamespace();
            return parent.Attributes(attrName);
        }

        public static XElement AddXmlContent(this XElement self, object contentValue)
        {
            self.Add(new XText(contentValue.ToXmlValue()));
            return self;
        }

        public static string GetXmlContent(this XElement self)
        {
            XText[] values = self.Nodes().OfType<XText>().ToArray();
            if (values.Length > 0)
                return values[0].Value;
            else 
                return null;
        }

        public static XAttribute Attribute_NamespaceNeutral(this XElement parent, XName name)
        {
            return parent.Attributes().FirstOrDefault(e => e.Name.LocalName == name.LocalName);
        }

        public static IEnumerable<XAttribute> Attributes_NamespaceNeutral(this XElement parent, XName name)
        {
            return parent.Attributes().Where(e => e.Name.LocalName == name.LocalName);
        }

        public static XElement Element_NamespaceNeutral(this XContainer parent, XName name)
        {
            return parent.Elements().FirstOrDefault(e => e.Name.LocalName == name.LocalName);
        }

        public static IEnumerable<XElement> Elements_NamespaceNeutral(this XContainer parent, XName name)
        {
            return parent.Elements().Where(e => e.Name.LocalName == name.LocalName);
        }

        public static bool IsEmpty(this XNamespace self)
        {
            return self != null && !String.IsNullOrEmpty(self.NamespaceName.Trim());
        }

        public static XNamespace IfEmptyThen(this XNamespace self, XNamespace next)
        {
            return self.IsEmpty() ? self : next;
        }

        public static XNamespace IfEmptyThenNone(this XNamespace self)
        {
            return IfEmptyThen(self, XNamespace.None);
        }


        public static XName OverrideNsIfEmpty(this XName self, XNamespace ns)
        {
            if (self.Namespace.IsEmpty())
                return self;
            else if (ns.IsEmpty())
                return ns + self.LocalName;
            else
                return self;
        }

        public static XName RemoveNamespace(this XName self)
        {
            return XName.Get(self.LocalName);
        }


    }
}
