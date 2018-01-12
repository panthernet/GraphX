using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Markup;

namespace FancyDevelopment.WinRtPluginSystem.MVVM
{
    class ViewModelXamlType<T> : IXamlType where T : ViewModelBase, new()
    {
        private readonly IXamlType _baseType;
        private IDictionary<string, ViewModelXamlMember<T>> _members;

        public ViewModelXamlType(IXamlType baseType)
        {
            _baseType = baseType;
        }

        public object ActivateInstance()
        {
            return Activator.CreateInstance<T>();
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
            get { return _baseType; }
        }

        public IXamlMember ContentProperty
        {
            get { return null; }
        }

        public object CreateFromString(string value)
        {
            throw new NotSupportedException();
        }

        public string FullName
        {
            get { return typeof(T).FullName; }
        }

        public IXamlMember GetMember(string name)
        {
            return _members[name];
        }

        public bool IsArray
        {
            get { return false; }
        }

        public bool IsBindable
        {
            get { return true; }
        }

        public bool IsCollection
        {
            get { return false; }
        }

        public bool IsConstructible
        {
            get { return true; }
        }

        public bool IsDictionary
        {
            get { return false; }
        }

        public bool IsMarkupExtension
        {
            get { return false; }
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
            _members = new Dictionary<string, ViewModelXamlMember<T>>();

            // Create a dummy view model to read the property information
            T dummy = Activator.CreateInstance<T>();

            foreach (KeyValuePair<string, Type> property in dummy.PropertyTypes)
            {
                IXamlType propertyType = XamlTypeProvider.Instance.GetType(property.Value);

                _members.Add(property.Key, new ViewModelXamlMember<T>(property.Key, this, propertyType));
            }
        }

        public Type UnderlyingType
        {
            get { return typeof (T); }
        }
    }
}
