using GraphX.PCL.Common.Models;

namespace GraphX.WPF.Controls.DesignerExampleData
{
    internal sealed class EdgeDataExample<TVertex> : EdgeBase<TVertex>
    {
        public EdgeDataExample(TVertex source, TVertex target)
            : base(source, target)
        {
            
        }
        public EdgeDataExample(TVertex source, TVertex target, double weight)
            : base(source, target, weight)
        {
            
        }

        public string Text { get; set; }
    }
}
