using System;
using Windows.UI.Xaml.Markup;

namespace FancyDevelopment.WinRtPluginSystem.MVVM
{
    class ViewModelXamlMember<T> : IXamlMember where T : ViewModelBase, new ()
    {
        private readonly string _propertyName;
        private readonly ViewModelXamlType<T> _targetType;
        private readonly IXamlType _propertyType;

        public ViewModelXamlMember(string propertyName, ViewModelXamlType<T> targetType, IXamlType propertyType)
        {
            _propertyName = propertyName;
            _targetType = targetType;
            _propertyType = propertyType;
        }

        public object GetValue(object instance)
        {
            ViewModelBase viewModel = instance as ViewModelBase;

            if(viewModel == null)
                throw new InvalidOperationException("Only view model types are supported");

            return viewModel.GetPropertyValue(_propertyName);
        }

        public bool IsAttachable
        {
            get { return false; }
        }

        public bool IsDependencyProperty
        {
            get { return false; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public string Name
        {
            get { return _propertyName; }
        }

        public void SetValue(object instance, object value)
        {
            ViewModelBase viewModel = instance as ViewModelBase;

            if (viewModel == null)
                throw new InvalidOperationException("Only view model types are supported");

            viewModel.SetPropertyValue(_propertyName, value);
        }

        public IXamlType TargetType
        {
            get { return _targetType; }
        }

        public IXamlType Type
        {
            get { return _propertyType; }
        }
    }
}
