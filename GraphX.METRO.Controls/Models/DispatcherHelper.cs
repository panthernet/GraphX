using System;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace GraphX.Controls.Models
{
    public static class DispatcherHelper
    {
        public static async Task CheckBeginInvokeOnUi(Action action)
        {
            var dispatcher = Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher;

            if (dispatcher.HasThreadAccess)
                action();
            else await dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                                       () => action());
        }
    }
}
