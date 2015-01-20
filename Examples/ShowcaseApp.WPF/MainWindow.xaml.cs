using System;
using System.Diagnostics;
using System.Reflection;
using FirstFloor.ModernUI.Presentation;
using FirstFloor.ModernUI.Windows.Controls;
using ShowcaseApp.WPF.Models;

namespace ShowcaseApp.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ModernWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            Title = "GraphX for WPF showcase application v" + FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
#if DEBUG
            var lg = new LinkGroup {DisplayName = "Debug"};
            lg.Links.Add(new Link { DisplayName = "Debug", Source = new Uri("Pages/DebugGraph.xaml", UriKind.Relative) });          
            MenuLinkGroups.Add(lg);
#endif
        }
    }
}
