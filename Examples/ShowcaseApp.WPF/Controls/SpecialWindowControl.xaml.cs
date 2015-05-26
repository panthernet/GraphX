using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using FirstFloor.ModernUI;
using FirstFloor.ModernUI.Windows;
using ShowcaseApp.WPF.Models;

namespace ShowcaseApp.WPF.Controls
{
    /// <summary>
    /// Interaction logic for SpecialWindowControl.xaml
    /// </summary>
    public partial class SpecialWindowControl : UserControl
    {
        public SpecialWindowControl(object type)
        {
            InitializeComponent();
            tabControl.ContentLoader = new SpecialContentLoader(type == null ? MiniSpecialType.None : (MiniSpecialType)type);
        }
    }

    internal class SpecialContentLoader: IContentLoader
    {
        public MiniSpecialType OpType { get; private set; }

        public SpecialContentLoader(MiniSpecialType type)
        {
            OpType = type;
        }

        public Task<object> LoadContentAsync(Uri uri, CancellationToken cancellationToken)
        {
            if (!Application.Current.Dispatcher.CheckAccess())
                throw new InvalidOperationException(Resources.UIThreadRequired);

            // scheduler ensures LoadContent is executed on the current UI thread
            var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
            return Task.Factory.StartNew(() => LoadContent(uri), cancellationToken, TaskCreationOptions.None, scheduler);
        }

        protected virtual object LoadContent(Uri uri)
        {
            // don't do anything in design mode
            if (ModernUIHelper.IsInDesignMode) return null;

            var result = Application.LoadComponent(uri);
            var spContent = result as ISpecialWindowContentIntro;
            if(spContent != null)
                spContent.IntroText = Properties.Resources.ResourceManager.GetString(OpType + "Text");
            var spContent2 = result as ISpecialWindowContentXaml;
            if (spContent2 != null)
                spContent2.XamlText = Properties.Resources.ResourceManager.GetString(OpType.ToString());
            var spContent3 = result as ISpecialWindowContentXamlTemplate;
            if (spContent3 != null)
                spContent3.XamlText = Properties.Resources.ResourceManager.GetString(OpType+"Template");

            return result;
        }
    }

    internal interface ISpecialWindowContentIntro
    {
        string IntroText { get; set; }
    }

    internal interface ISpecialWindowContentXaml
    {
        string XamlText { get; set; }
    }

    internal interface ISpecialWindowContentXamlTemplate
    {
        string XamlText { get; set; }
    }
}
