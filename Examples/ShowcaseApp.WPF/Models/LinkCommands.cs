using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace ShowcaseApp.WPF.Models
{
    public static class LinkCommands
    {
        private static readonly RoutedUICommand showMiniSpecialDialog = new RoutedUICommand("Show help", "ShowMiniSpecialDialog", typeof(LinkCommands));

        /// <summary>
        /// Gets the navigate link routed command.
        /// </summary>
        public static RoutedUICommand ShowMiniSpecialDialog
        {
            get { return showMiniSpecialDialog; }
        }
    }
}
