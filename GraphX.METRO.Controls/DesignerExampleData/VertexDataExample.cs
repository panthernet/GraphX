using GraphX.PCL.Common.Models;

namespace GraphX.METRO.Controls.DesignerExampleData
{
    internal sealed class VertexDataExample : VertexBase
    {
        public VertexDataExample(int id, string name)
        {
            ID = id; Name = name;
            //DataImage = new BitmapImage(new Uri(@"pack://application:,,,/GraphX.WPF.Controls;component/Images/help_black.png", UriKind.Absolute));
        }

        public string Name { get; set; }
        //public ImageSource DataImage{ get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
