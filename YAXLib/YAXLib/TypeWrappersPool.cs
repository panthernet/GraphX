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
using System.Linq;
using System.Text;

namespace YAXLib
{
    /// <summary>
    /// Implements a singleton pool of type-wrappers to prevent excessive creation of 
    /// repetetive type wrappers
    /// </summary>
    internal class TypeWrappersPool
    {
        /// <summary>
        /// The instance to the pool, to implement the singleton
        /// </summary>
        private static TypeWrappersPool s_instance = null;

        /// <summary>
        /// A dictionary from types to their corresponding wrappers
        /// </summary>
        private Dictionary<Type, UdtWrapper> m_dicTypes = new Dictionary<Type, UdtWrapper>();

        /// <summary>
        /// An object to lock type-wrapper dictionary to make it thread-safe
        /// </summary>
        private object m_lockDic = new object();

        /// <summary>
        /// Prevents a default instance of the <c>TypeWrappersPool</c> class from being created, from
        /// outside the scope of this class.
        /// </summary>
        private TypeWrappersPool()
        {
        }

        /// <summary>
        /// Gets the type wrappers pool.
        /// </summary>
        /// <value>The type wrappers pool.</value>
        public static TypeWrappersPool Pool
        {
            get
            {
                if (s_instance == null)
                    s_instance = new TypeWrappersPool();
                return s_instance;
            }
        }

        /// <summary>
        /// Cleans up the pool.
        /// </summary>
        public static void CleanUp()
        {
            if (s_instance != null)
            {
                s_instance = null;
                // TODO: not sure if it's good work to do
                GC.Collect();
            }
        }

        /// <summary>
        /// Gets the type wrapper corresponding to the specified type.
        /// </summary>
        /// <param name="t">The type whose wrapper is needed.</param>
        /// <param name="caller">reference to the serializer instance which called this method.</param>
        /// <returns>the type wrapper corresponding to the specified type</returns>
        public UdtWrapper GetTypeWrapper(Type t, YAXSerializer caller)
        {
            lock (m_lockDic)
            {
                UdtWrapper result;
                if (!m_dicTypes.TryGetValue(t, out result))
                {
                    result = new UdtWrapper(t, caller);
                    m_dicTypes.Add(t, result);
                }
                else
                {
                    result.SetYAXSerializerOptions(caller);
                }

                return result;
            }
        }
    }
}
