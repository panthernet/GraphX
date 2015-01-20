using System.Windows.Media;

namespace ShowcaseApp.WPF.Models
{
    public class ColorModel
    {
        public Color Color {get;set;}
        public string Text { get; set; }

        public ColorModel(string text, Color color)
        {
            Text = text; Color = color;
        }
    }
}
