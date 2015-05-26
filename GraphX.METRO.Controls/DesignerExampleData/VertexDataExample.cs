using GraphX.PCL.Common.Models;

namespace GraphX.Controls.DesignerExampleData
{
    internal sealed class VertexDataExample : VertexBase
    {
        public VertexDataExample(int id, string name)
        {
            ID = id; Name = name;
            //DataImage = new BitmapImage(new Uri(@"pack://application:,,,/GraphX.Controls;component/Images/help_black.png", UriKind.Absolute));
        }

        public string Name { get; set; }
        //public ImageSource DataImage{ get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
