using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace GraphX.Controls
{
    public static class DependencyObjectExtensions
    {
        public static IDisposable WatchProperty(this DependencyObject target,
            string propertyPath,
            DependencyPropertyChangedEventHandler handler)
        {
            return new DependencyPropertyWatcher(target, propertyPath, handler);
        }

        private class DependencyPropertyWatcher : DependencyObject, IDisposable
        {
            private DependencyPropertyChangedEventHandler _handler;

            public DependencyPropertyWatcher(DependencyObject target,
                string propertyPath,
                DependencyPropertyChangedEventHandler handler)
            {
                if (target == null) throw new ArgumentNullException("target");
                if (propertyPath == null) throw new ArgumentNullException("propertyPath");
                if (handler == null) throw new ArgumentNullException("handler");

                _handler = handler;

                var binding = new Binding
                {
                    Source = target,
                    Path = new PropertyPath(propertyPath),
                    Mode = BindingMode.OneWay,
                };
                BindingOperations.SetBinding(this, ValueProperty, binding);
            }

            private static readonly DependencyProperty ValueProperty =
                DependencyProperty.Register(
                    "Value",
                    typeof (object),
                    typeof (DependencyPropertyWatcher),
                    new PropertyMetadata(null, ValuePropertyChanged));

            private static void ValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            {
                var watcher = d as DependencyPropertyWatcher;
                if (watcher == null)
                    return;

                watcher.OnValueChanged(e);
            }

            private void OnValueChanged(DependencyPropertyChangedEventArgs e)
            {
                var handler = _handler;
                if (handler != null)
                    handler(this, e);
            }

            public void Dispose()
            {
                _handler = null;
                // There is no ClearBinding method, so set a dummy binding instead
                BindingOperations.SetBinding(this, ValueProperty, new Binding());
            }
        }
    }
}
