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
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace YAXLib
{
    /// <summary>
    /// Provides string utility methods
    /// </summary>
    internal static class StringUtils
    {
        /// <summary>
        /// Refines the location string. Trims it, and replaces invlalid characters with underscore.
        /// </summary>
        /// <param name="elemAddr">The element address to refine.</param>
        /// <returns>the refined location string</returns>
        public static string RefineLocationString(string elemAddr)
        {
            elemAddr = elemAddr.Trim(' ', '\t', '\r', '\n', '\v', '/', '\\');
            if (String.IsNullOrEmpty(elemAddr))
                return ".";

            // replace all back-slaches to slash
            elemAddr = elemAddr.Replace('\\', '/');

            var sb = new StringBuilder(elemAddr.Length);
            var steps = elemAddr.SplitPathNamespaceSafe();
            foreach(var step in steps)
            {
                sb.Append("/" + RefineSingleElement(step));
            }

            return sb.Remove(0, 1).ToString();
        }

        /// <summary>
        /// Heuristically determines if the supplied name conforms to the "expanded XML name" form supported by the System.Xml.Linq.XName class.
        /// </summary>
        /// <param name="name">The name to be examined.</param>
        /// <returns><c>true</c> if the supplied name appears to be in expanded form, otherwise <c>false</c>.</returns>
        public static bool LooksLikeExpandedXName(string name)
        {
            // XName permits strings of the form '{namespace}localname'. Detecting such cases allows
            // YAXLib to support explicit namespace use.
            // http://msdn.microsoft.com/en-us/library/system.xml.linq.xname.aspx

            name = name.Trim();

            // Needs at least 3 chars ('{}a' is, in theory, valid), must start with '{', must have a following closing '}' which must not be last char.
            if (name.Length >= 3)
            {
                if (name[ 0 ] == '{')
                {
                    int closingBrace = name.IndexOf('}', 1);
                    return closingBrace != -1 && closingBrace < (name.Length - 1);
                }
            }
            return false;
        }

        
        /// <summary>
        /// Refines a single element name. Refines the location string. Trims it, and replaces invlalid characters with underscore.
        /// </summary>
        /// <param name="elemName">Name of the element.</param>
        /// <returns>the refined element name</returns>
        public static string RefineSingleElement(string elemName)
        {
            if (elemName == null)
                return null;

            elemName = elemName.Trim(' ', '\t', '\r', '\n', '\v', '/', '\\');
            if (IsSingleLocationGeneric(elemName))
            {
                return elemName;
            }
            else if (LooksLikeExpandedXName(elemName))
            {
                // thanks go to CodePlex user: tg73 (http://www.codeplex.com/site/users/view/tg73)
                // for providing the code for expanded xml name support

                // XName permits strings of the form '{namespace}localname'. Detecting such cases allows
                // YAXLib to support explicit namespace use.
                // http://msdn.microsoft.com/en-us/library/system.xml.linq.xname.aspx

                // Leave namespace part alone, refine localname part.
                int closingBrace = elemName.IndexOf( '}' );
                string refinedLocalname = RefineSingleElement( elemName.Substring( closingBrace + 1 ) );
                return elemName.Substring( 0, closingBrace + 1 ) + refinedLocalname;
            }
            else
            {
                var sb = new StringBuilder(elemName.Length);

                // This uses the rules defined in http://www.w3.org/TR/xml/#NT-Name. 
                // Thanks go to [@asbjornu] for pointing to the W3C standard
                for (int i = 0; i < elemName.Length; i++)
                {
                    if (i == 0)
                        sb.Append(IsValidNameStartChar(elemName[i]) ? elemName[i] : '_');
                    else
                        sb.Append(IsValidNameChar(elemName[i]) ? elemName[i] : '_');
                }

                return sb.ToString();
            }
        }

        private static bool IsValidNameStartChar(char ch)
        {
            // This uses the rules defined in http://www.w3.org/TR/xml/#NT-Name. 
            // However colon (:) has been removed from the set of allowed characters,
            // because it is reserved for separating namespace prefix and XML-entity names.
            if (//ch == ':' || 
                ch == '_' ||
                IsInRange(ch, 'A', 'Z') || IsInRange(ch, 'a', 'z') ||
                IsInRange(ch, '\u00C0', '\u00D6') || 
                IsInRange(ch, '\u00D8', '\u00F6') ||
                IsInRange(ch, '\u00F8', '\u02FF') || 
                IsInRange(ch, '\u0370', '\u037D') ||
                IsInRange(ch, '\u037F', '\u1FFF') ||
                IsInRange(ch, '\u200C', '\u200D') ||
                IsInRange(ch, '\u2070', '\u218F') ||
                IsInRange(ch, '\u2C00', '\u2FEF') ||
                IsInRange(ch, '\u3001', '\uD7FF') ||
                IsInRange(ch, '\uF900', '\uFDCF') || 
                IsInRange(ch, '\uFDF0', '\uFFFD') 
                //|| IsInRange(ch, '\u10000', '\uEFFFF')
                )
                return true;

            return false;
        }

        private static bool IsValidNameChar(char ch)
        {
            return IsValidNameStartChar(ch) ||
                    ch == '-' || ch == '.' ||
                    IsInRange(ch, '0', '9') ||
                    ch == '\u00B7' ||
                    IsInRange(ch, '\u0300', '\u036F')
            || IsInRange(ch, '\u203F', '\u2040');
        }

        private static bool IsInRange(char ch, char lower, char upper)
        {
            return lower <= ch && ch <= upper;
        }


        /// <summary>
        /// Exttracts the path and alias from location string.
        /// A pure path location string: level1/level2
        /// A location string augmented with alias: level1/level2#somename
        /// Here path is "level1/level2" and alias is "somename".
        /// </summary>
        /// <param name="locationString">The location string.</param>
        /// <param name="path">The path to be extracted.</param>
        /// <param name="alias">The alias to be extracted.</param>
        public static void ExttractPathAndAliasFromLocationString(string locationString, out string path, out string alias)
        {
            int poundIndex = locationString.IndexOf('#');
            if(poundIndex >= 0)
            {
                if (poundIndex == 0)
                    path = "";
                else
                    path = locationString.Substring(0, poundIndex).Trim();

                if (poundIndex == locationString.Length - 1)
                    alias = "";
                else
                    alias = locationString.Substring(poundIndex + 1).Trim();
            }
            else
            {
                path = locationString;
                alias = "";
            }
        }


        /// <summary>
        /// Combines a location string and an element name to form a bigger location string.
        /// </summary>
        /// <param name="location">The location string.</param>
        /// <param name="elemName">Name of the element.</param>
        /// <returns>a bigger location string formed by combining a location string and an element name.</returns>
        public static string CombineLocationAndElementName(string location, XName elemName)
        {
            return String.Format("{0}/{1}", location, elemName);
        }

        /// <summary>
        /// Divides the location string one step, to form a shorter location string.
        /// </summary>
        /// <param name="location">The location string to divide.</param>
        /// <param name="newLocation">The new location string which is one level shorter.</param>
        /// <param name="newElem">The element name removed from the end of location string.</param>
        /// <returns></returns>
        public static bool DivideLocationOneStep(string location, out string newLocation, out string newElem)
        {
            newLocation = location;
            newElem = null;
            
            int slashIdx = location.LastIndexOf('/');
            if (slashIdx < 0) // no slashes found
            {
                if (IsLocationAllGeneric(location))
                {
                    return false;
                }
                else
                {
                    newElem = location;
                    newLocation = ".";
                    return true;
                }
            }
            else // some slashes found
            {
                string preSlash = location.Substring(0, slashIdx);
                string postSlash = location.Substring(slashIdx + 1);

                if (IsLocationAllGeneric(postSlash))
                {
                    return false;
                }
                else
                {
                    newLocation = preSlash;
                    newElem = postSlash;
                    return true;
                }
            }
        }

        /// <summary>
        /// Determines whether the specified location is composed of levels 
        /// which are themselves either "." or "..".
        /// </summary>
        /// <param name="location">The location string to check.</param>
        /// <returns>
        /// <c>true</c> if the specified location string is all composed of "." or ".." levels; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsLocationAllGeneric(string location)
        {
            var locSteps = location.SplitPathNamespaceSafe();

            foreach (var loc in locSteps)
            {
                if (!IsSingleLocationGeneric(loc))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Determines whether the specified location string is either "." or "..".
        /// </summary>
        /// <param name="location">The location string to check.</param>
        /// <returns>
        /// <c>true</c> if the specified location string is either "." or ".."; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsSingleLocationGeneric(string location)
        {
            return location == "." || location == "..";
        }

        /// <summary>
        /// Gets the string corresponidng to the given array dimensions.
        /// </summary>
        /// <param name="dims">The array dimensions.</param>
        /// <returns>the string corresponidng to the given array dimensions</returns>
        public static string GetArrayDimsString(int[] dims)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < dims.Length; i++)
            {
                if (i != 0)
                    sb.Append(",");
                sb.Append(dims[i]);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Parses the array dimensions string, and returns the corresponding dimensions array.
        /// </summary>
        /// <param name="str">The string to parse.</param>
        /// <returns>the dimensions array corresponiding to the given string</returns>
        public static int[] ParseArrayDimsString(string str)
        {
            string[] strDims = str.Split(new char[] { ',', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            List<int> lst = new List<int>();
            foreach (string strDim in strDims)
            {
                int dim;
                if (Int32.TryParse(strDim, out dim))
                    lst.Add(dim);
            }

            return lst.ToArray();
        }

        /// <summary>
        /// Splits a string at each instance of a '/' except where such slashes
        /// are within {}.
        /// </summary>
        /// <param name="value">The string to split</param>
        /// <returns>An enumerable set of strings which were seperated by '/'</returns>
        public static IEnumerable<string> SplitPathNamespaceSafe(this string value)
        {
            int bracketCount = 0;
            int lastStart = 0;
            string temp = value;

            if (value.Length <= 1)
            {
                yield return value;
                yield break;
            }

            for(int i = 0; i < temp.Length; i++)
            {
                if (temp[i] == '{')
                    bracketCount++;
                else if (temp[i] == '}')
                    bracketCount--;
                else if (temp[i] == '/')
                {
                    if (bracketCount == 0)
                    {
                        yield return temp.Substring(lastStart, i - lastStart);
                        lastStart = i + 1;
                    }
                }
                else continue;
            }

            if (lastStart <= temp.Length - 1)
                yield return temp.Substring(lastStart);
        }

        public static DateTime ParseDateTimeTimeZoneSafe(string str, IFormatProvider formatProvider)
        {
            DateTimeOffset dto;
            if (!DateTimeOffset.TryParse(str, out dto))
            {
                return DateTime.MinValue;
            }

            if (dto.Offset == TimeSpan.Zero)
            {
                return dto.UtcDateTime;
            }
            else
            {
                return dto.DateTime;
            }
        }


    }
}
