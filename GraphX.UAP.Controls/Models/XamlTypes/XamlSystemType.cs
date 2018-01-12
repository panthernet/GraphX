using System;
using Windows.UI.Xaml.Markup;

namespace FancyDevelopment.WinRtPluginSystem.MVVM
{
    class XamlSystemType : IXamlType
    {
        private readonly Type _type;

        public XamlSystemType(Type type)
        {
            _type = type;
        }

        public object ActivateInstance()
        {
            throw new NotSupportedException();
        }

        public void AddToMap(object instance, object key, object value)
        {
            throw new NotSupportedException();
        }

        public void AddToVector(object instance, object value)
        {
            throw new NotSupportedException();
        }

        public IXamlType BaseType
        {
            get { throw new NotSupportedException(); }
        }

        public IXamlMember ContentProperty
        {
            get { throw new NotSupportedException(); }
        }

        public object CreateFromString(string value)
        {
            throw new NotSupportedException();
        }

        public string FullName
        {
            get { return _type.FullName; }
        }

        public IXamlMember GetMember(string name)
        {
            throw new NotSupportedException();
        }

        public bool IsArray
        {
            get { throw new NotSupportedException(); }
        }

        public bool IsBindable
        {
            get { throw new NotSupportedException(); }
        }

        public bool IsCollection
        {
            get { throw new NotSupportedException(); }
        }

        public bool IsConstructible
        {
            get { throw new NotSupportedException(); }
        }

        public bool IsDictionary
        {
            get { throw new NotSupportedException(); }
        }

        public bool IsMarkupExtension
        {
            get { throw new NotSupportedException(); }
        }

        public IXamlType ItemType
        {
            get { throw new NotSupportedException(); }
        }

        public IXamlType KeyType
        {
            get { throw new NotSupportedException(); }
        }

        public void RunInitializer()
        {
            throw new NotSupportedException();
        }

        public Type UnderlyingType
        {
            get { return _type; }
        }
    }
}
