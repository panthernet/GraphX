using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Markup;

namespace FancyDevelopment.WinRtPluginSystem.MVVM
{
    public class XamlTypeProvider
    {
        private static XamlTypeProvider _instance;

        public static XamlTypeProvider Instance
        {
            get
            {
                if(_instance == null)
                    _instance = new XamlTypeProvider();

                return _instance;
            }
        }

        private IDictionary<Type, IXamlType> _xamlTypesByType;
        private IDictionary<string, IXamlType> _xamlTypesByName; 

        public XamlTypeProvider()
        {
            _xamlTypesByName = new Dictionary<string, IXamlType>();
            _xamlTypesByType = new Dictionary<Type, IXamlType>();
        }

        public void RegisterViewModelType<T>(IXamlType baseType = null) where T : ViewModelBase, new ()
        {
            IXamlType xamlType = new ViewModelXamlType<T>(baseType);
            AddType(typeof(T), xamlType);
        }

        public void AddType(Type underlyingType, IXamlType xamlType)
        {
            _xamlTypesByType[underlyingType] = xamlType;
            _xamlTypesByName[underlyingType.FullName] = xamlType;
        }

        public IXamlType GetType(Type underlyingType)
        {
            if(_xamlTypesByType.ContainsKey(underlyingType))
            {
                return _xamlTypesByType[underlyingType];
            }

            return CreateSystemType(underlyingType);
        }

        public IXamlType GetType(string fullName)
        {
            if(_xamlTypesByName.ContainsKey(fullName))
            {
                return _xamlTypesByName[fullName];
            }

            return null;
        }

        private IXamlType CreateSystemType(Type systemType)
        {
            XamlSystemType xamlType = new XamlSystemType(systemType);
            _xamlTypesByType[systemType] = xamlType;
            return xamlType;
        }
    }
}
