using ShowcaseExample.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ShowcaseExample
{
    /// <summary>
    /// Interaction logic for GraphLegend.xaml
    /// </summary>
    public partial class GraphEdgeLegend : UserControl
    {
        public GraphEdgeLegend():this(null)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
                DefineData(new Dictionary<int, ColorModel>() { { 0, new ColorModel("AAAA", Colors.Black) }, { 10, new ColorModel("BBBB", Colors.Red) }, { 30, new ColorModel("CCCC", Colors.Yellow) } });
        }
        public GraphEdgeLegend(Dictionary<int, ColorModel> colordic)
        {
            InitializeComponent();
            DefineData(colordic);
        }

        /// <summary>
        /// Is legend available (have any colors defined)
        /// </summary>
        public bool IsLegendAvailable
        {
            get { return spmain.Children.Count > 0; }
        }

        public void DefineData(Dictionary<int, ColorModel> colordic)
        {
            if (colordic == null) return;

            foreach (var item in colordic)
            {
                var border = new Border() { Width = 15, Height = 15, BorderBrush = new SolidColorBrush(Colors.Black), BorderThickness = new Thickness(1), Background = new SolidColorBrush(item.Value.Color), Margin = new Thickness(2) };
                var text = new TextBlock() { MinWidth = 50, Text = item.Value.Text, Margin = new Thickness(2), VerticalAlignment = System.Windows.VerticalAlignment.Center,
                                             Foreground = new SolidColorBrush(Colors.White)};
                spmain.Children.Add(border); spmain.Children.Add(text);
            }
        }
    }
}
