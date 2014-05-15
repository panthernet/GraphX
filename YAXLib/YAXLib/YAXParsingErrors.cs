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
using System.Globalization;
using System.Text;

namespace YAXLib
{
    /// <summary>
    /// Holds list of exception occured during serialization/deserialization.
    /// </summary>
    public class YAXParsingErrors
    {
        #region Fields

        /// <summary>
        /// The list of exception occured during serialization/deserialization.
        /// </summary>
        private List<KeyValuePair<YAXException, YAXExceptionTypes>> listExceptions = new List<KeyValuePair<YAXException, YAXExceptionTypes>>();

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether the list of errors is empty or not.
        /// </summary>
        /// <value><c>true</c> if the list is not empty; otherwise, <c>false</c>.</value>
        public bool ContainsAnyError
        {
            get
            {
                return this.listExceptions.Count > 0;
            }
        }

        /// <summary>
        /// Gets the number of errors within the list of errors.
        /// </summary>
        /// <value>the number of errors within the list of errors.</value>
        public int Count
        {
            get
            {
                return this.listExceptions.Count;
            }
        }

        #endregion

        #region Indexers

        /// <summary>
        /// Gets the the pair of Exception and its corresponding exception-type with the specified n.
        /// </summary>
        /// <param name="n">The index of the exception/exception type pair in the error list to return.</param>
        /// <value></value>
        public KeyValuePair<YAXException, YAXExceptionTypes> this[int n]
        {
            get
            {
                return this.listExceptions[n];
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds an exception with the corresponding type.
        /// </summary>
        /// <param name="exception">The exception to add.</param>
        /// <param name="exceptionType">Type of the exception added.</param>
        public void AddException(YAXException exception, YAXExceptionTypes exceptionType)
        {
            this.listExceptions.Add(new KeyValuePair<YAXException, YAXExceptionTypes>(exception, exceptionType));
        }

        /// <summary>
        /// Clears the list of errors.
        /// </summary>
        public void ClearErrors()
        {
            this.listExceptions.Clear();
        }

        /// <summary>
        /// Adds the list of errors within another instance of <c>YAXParsingErrors</c>.
        /// </summary>
        /// <param name="parsingErrors">The parsing errors to add its content.</param>
        public void AddRange(YAXParsingErrors parsingErrors)
        {
            parsingErrors.listExceptions.ForEach(this.listExceptions.Add);
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            this.listExceptions.ForEach(pair =>
            {
                sb.AppendLine(String.Format(CultureInfo.CurrentCulture, "{0,-8} : {1}", pair.Value, pair.Key.Message));
                sb.AppendLine();
            });

            return sb.ToString();
        }

        #endregion
    }
}

