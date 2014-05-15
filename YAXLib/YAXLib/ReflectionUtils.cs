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
using System.Globalization;
using System.Collections;
using System.Reflection;

namespace YAXLib
{
    /// <summary>
    /// A utility class for reflection related stuff
    /// </summary>
    internal static class ReflectionUtils
    {
        /// <summary>
        /// Determines whether the specified type is basic type. A basic type is one that can be wholly expressed
        /// as an XML attribute. All primitive data types and type <c>string</c> and <c>DataTime</c> are basic.
        /// </summary>
        /// <param name="t">The type to check.</param>
        /// <returns>
        /// <value><c>true</c> if the specified type is a basic type; otherwise, <c>false</c>.</value>
        /// </returns>
        public static bool IsBasicType(Type t)
        {
            if (t == typeof(string) || t.IsPrimitive || t.IsEnum || t == typeof(DateTime) || t == typeof(decimal) ||
                t == typeof(Guid))
            {
                return true;
            }
            else
            {
                Type nullableValueType;
                if(IsNullable(t, out nullableValueType))
                    return IsBasicType(nullableValueType);
                return false;
            }
        }

        /// <summary>
        /// Determines whether the specified type is array.
        /// </summary>
        /// <param name="type">The type to check</param>
        /// <param name="elementType">Type of the containing element.</param>
        /// <returns>
        /// 	<value><c>true</c> if the specified type is array; otherwise, <c>false</c>.</value>
        /// </returns>
        public static bool IsArray(Type type, out Type elementType)
        {
            if (type.IsArray)
            {
                elementType = type.GetElementType();
                return true;
            }
            else if (type == typeof(Array)) // i.e., a direct ref to System.Array
            {
                elementType = typeof(object);
                return true;
            }
            else
            {
                elementType = typeof(object);
                return false;
            }
        }

        /// <summary>
        /// Determines whether the specified type is array.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>
        /// <c>true</c> if the specified type is array; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsArray(Type type)
        {
            Type elementType;
            return IsArray(type, out elementType);
        }

        /// <summary>
        /// Gets the array dimensions.
        /// </summary>
        /// <param name="ar">The array to return its dimensions.</param>
        /// <returns>the specified array's dimensions</returns>
        public static int[] GetArrayDimensions(object ar)
        {
            int[] dims = null;
            if (IsArray(ar.GetType()))
            {
                var arObj = ar as System.Array;
                dims = new int[arObj.Rank];
                for (int i = 0; i < dims.Length; i++)
                    dims[i] = arObj.GetLength(i);
            }

            return dims;
        }

        /// <summary>
        /// Gets the friendly name for the type. Recommended for generic types.
        /// </summary>
        /// <param name="type">The type to get its friendly name</param>
        /// <returns>The friendly name for the type</returns>
        public static string GetTypeFriendlyName(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            string name = type.Name;
            if (type.IsGenericType)
            {
                int backqIndex = name.IndexOf('`');
                if (backqIndex == 0)
                {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, "Bad type name: {0}", name));
                }
                else if (backqIndex > 0)
                {
                    name = name.Substring(0, backqIndex);
                }

                name += "Of";

                Array.ForEach(
                    type.GetGenericArguments(),
                    genType => name += GetTypeFriendlyName(genType));
            }
            else if (type.IsArray)
            {
                Type t = type.GetElementType();
                name = String.Format(CultureInfo.InvariantCulture, "Array{0}Of{1}", type.GetArrayRank(), GetTypeFriendlyName(t));
            }

            return name;
        }

        /// <summary>
        /// Determines whether the type specified contains generic parameters or not.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>
        /// <value><c>true</c> if the type contains generic parameters; otherwise,<c>false</c>.</value>
        /// </returns>
        public static bool TypeContainsGenericParameters(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (type.IsGenericType)
            {
                foreach (Type genType in type.GetGenericArguments())
                {
                    if (genType.IsGenericParameter)
                    {
                        return true;
                    }
                    else if (TypeContainsGenericParameters(genType))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Determines whether the specified type is a collection type, i.e., it implements IEnumerable.
        /// Although System.String is derived from IEnumerable, it is considered as an exception.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>
        /// <value><c>true</c> if the specified type is a collection type; otherwise, <c>false</c>.</value>
        /// </returns>
        public static bool IsCollectionType(Type type)
        {
            if (type == typeof(string))
            {
                return false;
            }

            return IsIEnumerable(type);
        }

        /// <summary>
        /// Determines whether the specified type has implemented or is an <c>IEnumerable</c> or <c>IEnumerable&lt;&gt;</c>
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>
        /// 	<value><c>true</c> if the specified type is enumerable; otherwise, <c>false</c>.</value>
        /// </returns>
        public static bool IsIEnumerable(Type type)
        {
            Type seqType;
            return IsIEnumerable(type, out seqType);
        }

        public static bool IsDerivedFromGenericInterfaceType(Type givenType, Type genericInterfaceType, out Type genericType)
        {
            genericType = null;
            if ((givenType.IsClass || givenType.IsValueType) && !givenType.IsAbstract)
            {
                foreach (Type interfaceType in givenType.GetInterfaces())
                {
                    if (interfaceType.IsGenericType &&
                        interfaceType.GetGenericTypeDefinition() == genericInterfaceType)
                    {
                        Type[] genArgs = interfaceType.GetGenericArguments();
                        if (genArgs.Length != 1)
                            return false;

                        genericType = genArgs[0];
                        return true;
                    }
                }
            }
            return false;
        }


        /// <summary>
        /// Determines whether the specified type has implemented or is an <c>IEnumerable</c> or <c>IEnumerable&lt;&gt;</c> .
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <param name="seqType">Type of the sequence items.</param>
        /// <returns>
        /// <value><c>true</c> if the specified type is enumerable; otherwise, <c>false</c>.</value>
        /// </returns>
        public static bool IsIEnumerable(Type type, out Type seqType)
        {
            // detect arrays early
            if (IsArray(type, out seqType))
                return true;

            seqType = typeof(object);
            if (type == typeof(IEnumerable))
                return true;

            bool isNongenericEnumerable = false;

            if (type.IsInterface && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                seqType = type.GetGenericArguments()[0];
                return true;
            }

            foreach (Type interfaceType in type.GetInterfaces())
            {
                if (interfaceType.IsGenericType &&
                    interfaceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    Type[] genArgs = interfaceType.GetGenericArguments();
                    seqType = genArgs[0];
                    return true;
                }
                else if (interfaceType == typeof(IEnumerable))
                {
                    isNongenericEnumerable = true;
                }
            }

            // the second case is a direct reference to IEnumerable
            if (isNongenericEnumerable || type == typeof(IEnumerable))
            {
                seqType = typeof(object);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the type of the items of a collection type.
        /// </summary>
        /// <param name="type">The type of the collection.</param>
        /// <returns>The type of the items of a collection type.</returns>
        public static Type GetCollectionItemType(Type type)
        {
            Type itemType = typeof(object);
            if (IsIEnumerable(type, out itemType))
                return itemType;
            else
                throw new Exception("The specified type must be a collection");
        }

        /// <summary>
        /// Determines whether the specified type has implemented <c>IList</c>.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>
        /// <value><c>true</c> if the specified type has implemented <c>IList</c>; otherwise, <c>false</c>.</value>
        /// </returns>
        public static bool IsIList(Type type)
        {
            // a direct reference to the interface itself is also OK.
            if (type.IsInterface && type.GetGenericTypeDefinition() == typeof(IList<>))
            {
                return true;
            }

            foreach (Type interfaceType in type.GetInterfaces())
            {
                if (interfaceType.IsGenericType &&
                    interfaceType.GetGenericTypeDefinition() == typeof(IList<>))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines whether the specified type has implemented the <c>ICollection</c> interface.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <param name="itemType">Type of the member items.</param>
        /// <returns>
        /// <value><c>true</c> if the specified type has implemented the <c>ICollection</c> interface; otherwise, <c>false</c>.</value>
        /// </returns>
        public static bool IsICollection(Type type, out Type itemType)
        {
            itemType = typeof(object);

            // a direct reference to the interface itself is also OK.
            if (type.IsInterface && type.GetGenericTypeDefinition() == typeof(ICollection<>))
            {
                itemType = type.GetGenericArguments()[0];
                return true;
            }

            foreach (Type interfaceType in type.GetInterfaces())
            {
                if (interfaceType.IsGenericType &&
                    interfaceType.GetGenericTypeDefinition() == typeof(ICollection<>))
                {
                    itemType = interfaceType.GetGenericArguments()[0];
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines whether the specified type is a generic dictionary.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <param name="keyType">Type of the key.</param>
        /// <param name="valueType">Type of the value.</param>
        /// <returns>
        /// <value><c>true</c> if the specified type has implemented the IDictionary interface; otherwise, <c>false</c>.</value>
        /// </returns>
        public static bool IsIDictionary(Type type, out Type keyType, out Type valueType)
        {
            keyType = typeof(object);
            valueType = typeof(object);

            // a direct reference to the interface itself is also OK.
            if (type.IsInterface && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IDictionary<,>))
            {
                Type[] genArgs = type.GetGenericArguments();
                keyType = genArgs[0];
                valueType = genArgs[1];
                return true;
            }

            foreach (Type interfaceType in type.GetInterfaces())
            {
                if (interfaceType.IsGenericType &&
                    interfaceType.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                {
                    Type[] genArgs = interfaceType.GetGenericArguments();
                    keyType = genArgs[0];
                    valueType = genArgs[1];
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines whether the specified type is a generic dictionary.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>
        /// <value><c>true</c> if the specified type is dictionary; otherwise, <c>false</c>.</value>
        /// </returns>
        public static bool IsIDictionary(Type type)
        {
            Type keyType, valueType;
            return IsIDictionary(type, out keyType, out valueType);
        }

        /// <summary>
        /// Determines whether the specified type is a non generic IDictionary, e.g., a Hashtable.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>
        /// 	<c>true</c> if the specified type is a non generic IDictionary; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNonGenericIDictionary(Type type)
        {
            // a direct reference to the interface itself is also OK.
            if (type == typeof(IDictionary))
            {
                return true;
            }

            foreach (Type interfaceType in type.GetInterfaces())
            {
                if (interfaceType == typeof(IDictionary))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines whether the specified type is equal to this type,
        /// or is a nullable of this type, or this type is a nullable of 
        /// the other type.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool EqualsOrIsNullableOf(this Type self, Type other)
        {
            if (self == other)
                return true;

            Type selfBaseType;
            Type otherBaseType;
            if (!IsNullable(self, out selfBaseType))
                selfBaseType = self;
            if (!IsNullable(other, out otherBaseType))
                otherBaseType = other;

            return selfBaseType == otherBaseType;
        }

        /// <summary>
        /// Determines whether the specified type is equal or inherited from another specified type.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <param name="baseType">Another type that the specified type is checked whether it is equal or
        /// has been driven from.</param>
        /// <returns>
        /// 	<c>true</c> if the specified type is equal or inherited from another specified type; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsTypeEqualOrInheritedFromType(Type type, Type baseType)
        {
            if (type == baseType)
                return true;

            bool isTypeGenDef = type.IsGenericTypeDefinition;
            bool isBaseGenDef = baseType.IsGenericTypeDefinition;
            Type[] typeGenArgs = null;
            Type[] baseGenArgs = null;

            if (type.IsGenericType)
            {
                if (isBaseGenDef)
                {
                    if (!isTypeGenDef)
                    {
                        type = type.GetGenericTypeDefinition();
                        isTypeGenDef = true;
                    }
                }
                else
                {
                    typeGenArgs = type.GetGenericArguments();
                }
            }

            if (baseType.IsGenericType)
            {
                if (isTypeGenDef)
                {
                    if (!isBaseGenDef)
                    {
                        baseType = baseType.GetGenericTypeDefinition();
                        isBaseGenDef = true;
                    }
                }
                else
                {
                    baseGenArgs = baseType.GetGenericArguments();
                }
            }

            if (type == baseType)
                return true;

            if(typeGenArgs != null && baseGenArgs != null)
            {
                if(typeGenArgs.Length != baseGenArgs.Length)
                    return false;

                for(int i = 0; i < typeGenArgs.Length; i++)
                {
                    // TODO: check if I should call this method for type args recersively
                    if (typeGenArgs[i] != baseGenArgs[i])
                        return false;
                }
            }

            if (baseType.IsInterface)
            {
                foreach (var iface in type.GetInterfaces())
                {
                    if (iface.Name == baseType.Name)
                        return true;
                }
                return false;
            }
            else
            {
                Type curBaseType = type.BaseType;
                while (curBaseType != null)
                {
                    if (curBaseType.Name == baseType.Name)
                        return true;

                    curBaseType = curBaseType.BaseType;
                }

                return false;
            }
        }

        /// <summary>
        /// Determines whether the specified type is nullable.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <param name="valueType">The value type of the corresponding nullable type.</param>
        /// <returns>
        /// <c>true</c> if the specified type is nullable; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNullable(Type type, out Type valueType)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                valueType = type.GetGenericArguments()[0];
                return true;
            }

            valueType = null;
            return false;
        }

        /// <summary>
        /// Determines whether the specified type is nullable.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>
        /// <c>true</c> if the specified type is nullable; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNullable(Type type)
        {
            Type valueType;
            return IsNullable(type, out valueType);
        }

        /// <summary>
        /// Determines whether the specified type implements <c>IFormattable</c>
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>
        /// 	<c>true</c> if the specified type implements <c>IFormattable</c>; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsIFormattable(Type type)
        {
            // a direct reference to the interface itself is also OK.
            if (type == typeof(IFormattable))
            {
                return true;
            }

            foreach (Type interfaceType in type.GetInterfaces())
            {
                if (interfaceType == typeof(IFormattable))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines whether the type provides the functionality 
        /// to format the value of an object into a string representation.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>
        /// <value><c>true</c> if the specified type impliments the <c>IFormattable</c> interface; otherwise, <c>false</c>.</value>
        /// </returns>
        public static bool IsStringConvertibleIFormattable(Type type)
        {
            // is IFormattable
            // accept parameterless ToString
            // accept ctor of string
            if (IsIFormattable(type) && !HasOneReadWriteProperty(type))
            {
                if (null != type.GetConstructor(new Type[] { typeof(string) }))
                {
                    if (null != type.GetMethod("ToString", new Type[0]) &&
                        null != type.GetMethod("ToString", new Type[] { typeof(string) })) 
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Checks to see if the specified type has readable and writable properties.
        /// </summary>
        /// <param name="type">The type to check for.</param>
        /// <returns><value><c>true</c> if the specified type has readable and writable properties; otherwise, <c>false</c>.</value></returns>
        public static bool HasOneReadWriteProperty(Type type)
        {
            PropertyInfo[] props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo pi in props)
            {
                if (pi.CanRead && pi.CanWrite)
                {
                    MethodInfo getPi = pi.GetGetMethod(false);
                    MethodInfo setPi = pi.GetSetMethod(false);
                    if (setPi != null && getPi != null)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Tries to format the specified object using the format string provided.
        /// If the formatting operation is not applicable, the source object is returned intact.
        /// Note: The type of the returned object will be 'System.String' if formatting succeeds.
        /// </summary>
        /// <param name="src">The source object.</param>
        /// <param name="format">The format string.</param>
        /// <returns><code>System.String</code> if the format is successful; otherwise, the original object</returns>
        public static object TryFormatObject(object src, string format)
        {
            if (format == null || src == null)
            {
                return src;
            }

            object formattedObject = null;

            try
            {
                formattedObject = src.GetType().InvokeMember("ToString", BindingFlags.InvokeMethod, null, src, new object[] { format });
            }
            catch
            {
                return src;
                //throw new YAXInvalidFormatProvided(src.GetType(), format);
                //this.OnExceptionOccurred(new YAXInvalidFormatProvided(src.GetType(), format), this.m_defaultExceptionType);
            }

            return formattedObject ?? src;
        }

        /// <summary>
        /// Converts the specified object from a basic type to another type as specified.
        /// It is meant by basic types, primitive data types, strings, and enums.
        /// </summary>
        /// <param name="value">The object to be converted.</param>
        /// <param name="dstType">the destination type of conversion.</param>
        /// <returns>the converted object</returns>
        public static object ConvertBasicType(object value, Type dstType)
        {
            object convertedObj = null;
            if (dstType.IsEnum)
            {
                UdtWrapper typeWrapper = TypeWrappersPool.Pool.GetTypeWrapper(dstType, null);
                convertedObj = typeWrapper.EnumWrapper.ParseAlias(value.ToString());
            }
            else if (dstType == typeof(DateTime))
            {
                convertedObj = StringUtils.ParseDateTimeTimeZoneSafe(value.ToString(), CultureInfo.InvariantCulture);
            }
            else if (dstType == typeof(decimal))
            {
                // to fix the asymetry of used locales for this type between serialization and deseralization
                convertedObj = Convert.ChangeType(value, dstType, CultureInfo.InvariantCulture);
            }
            else if (dstType == typeof(bool))
            {
                string strValue = value.ToString().Trim().ToLower();
                if (strValue == "false" || strValue == "no" || strValue == "0")
                    convertedObj = false;
                else if (strValue == "true" || strValue == "yes" || strValue == "1")
                    convertedObj = true;
                else
                {
                    int boolIntValue = 0;
                    if (Int32.TryParse(strValue, out boolIntValue))
                        convertedObj = boolIntValue == 0 ? false : true;
                    else
                        throw new Exception("The specified value is not recognized as boolean: " + strValue);
                }
            }
            else if (dstType == typeof(Guid))
            {
                return new Guid(value.ToString());
            }
            else
            {
                Type nullableType;
                if (IsNullable(dstType, out nullableType))
                {
                    if (value == null || value.ToString() == String.Empty)
                        return null;
                    return ConvertBasicType(value, nullableType);
                }

                IFormatProvider ifProvider = CultureInfo.InvariantCulture;
                convertedObj = Convert.ChangeType(value, dstType, ifProvider);
            }

            return convertedObj;
        }

        /// <summary>
        /// Searches all loaded assemblies to find a type with a special name.
        /// </summary>
        /// <param name="name">The name of the type to find.</param>
        /// <returns>type found using the specified name</returns>
        public static Type GetTypeByName(string name)
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            // first search the 1st assembly (i.e. the mscorlib), then start from the last assembly backward, 
            // the last assemblies are user defined ones
            for(int i = assemblies.Length; i > 0; i--)
            {
                Assembly curAssembly = (i == assemblies.Length) ? assemblies[0] : assemblies[i];

                try
                {
                    Type type = curAssembly.GetType(name, false, true);
                    if (type != null)
                        return type;
                }
                catch
                {
                }
            }

            return null;
        }

        /// <summary>
        /// Determines whether the specified property is public.
        /// </summary>
        /// <param name="pi">The property.</param>
        /// <returns>
        /// <c>true</c> if the specified property is public; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsPublicProperty(PropertyInfo pi)
        {
            foreach(var m in pi.GetAccessors())
                if(m.IsPublic) 
                    return true;

            return false;
        }

        public static bool IsPartOfNetFx(MemberInfo memberInfo)
        {
            string moduleName = memberInfo.Module.Name;
            //memberInfo.Module.Assembly.

            return moduleName.Equals("mscorlib.dll", StringComparison.InvariantCultureIgnoreCase)
                || moduleName.Equals("System.dll", StringComparison.InvariantCultureIgnoreCase)
                || moduleName.Equals("System.Core.dll", StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool IsInstantiableCollection(Type colType)
        {
            try
            {
                var col = colType.InvokeMember(string.Empty, BindingFlags.CreateInstance, null, null, new object[0]);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static T InvokeGetProperty<T>(object srcObj, string propertyName)
        {
            return (T)srcObj.GetType().InvokeMember(propertyName, BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance, null, srcObj, null);
        }

        public static T InvokeIntIndexer<T>(object srcObj, string propertyName, int index)
        {
            var pi = srcObj.GetType().GetProperty("Item", new [] { typeof(int) });
            return (T) pi.GetValue(srcObj, new object[] { index });
        }

        public static object InvokeStaticMethod(Type type, string methodName, params object[] args)
        {
            var argTypes = args.Select(x => x.GetType()).ToArray();
            var method = type.GetMethod(methodName, argTypes);
            var result = method.Invoke(null, args);
            return result;
        }

        public static object InvokeMethod(object srcObj, string methodName, params object[] args)
        {
            var argTypes = args.Select(x => x.GetType()).ToArray();
            var method = srcObj.GetType().GetMethod(methodName, argTypes);
            var result = method.Invoke(srcObj, args);
            return result;
        }
    }
}
