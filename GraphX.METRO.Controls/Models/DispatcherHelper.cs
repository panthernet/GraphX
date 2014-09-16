using System;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace GraphX
{
    public static class DispatcherHelper
    {
        public static CoreDispatcher UiDispatcher { get; private set; }

        public static async Task CheckBeginInvokeOnUi(Action action)
        {
            if (UiDispatcher.HasThreadAccess)
                action();
            else await UiDispatcher.RunAsync(CoreDispatcherPriority.Normal,
                                       () => action());
        }

        static DispatcherHelper()
        {
            if (UiDispatcher != null) return;
            UiDispatcher = Window.Current.Dispatcher;
        }
    }
}
