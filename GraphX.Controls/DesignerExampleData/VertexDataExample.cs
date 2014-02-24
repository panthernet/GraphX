using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GraphX.DesignerExampleData
{
    internal sealed class VertexDataExample : VertexBase
    {
        public VertexDataExample(int id, string name)
        {
            ID = id; Name = name;
            DataImage = new BitmapImage(new Uri(@"pack://application:,,,/GraphX.Controls;component/Images/help_black.png", UriKind.Absolute)) { CacheOption = BitmapCacheOption.OnLoad };
        }

        public string Name { get; set; }
        public ImageSource DataImage{ get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
