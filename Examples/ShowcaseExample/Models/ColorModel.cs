using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace ShowcaseExample.Models
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
