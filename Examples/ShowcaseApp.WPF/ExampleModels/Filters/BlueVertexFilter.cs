using GraphX.PCL.Common.Interfaces;
using QuickGraph;

namespace ShowcaseApp.WPF.Filters
{
    public class BlueVertexFilter: IGraphFilter<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>>
    {
        public BidirectionalGraph<DataVertex, DataEdge> ProcessFilter(BidirectionalGraph<DataVertex, DataEdge> inputGraph)
        {
            inputGraph.RemoveVertexIf(a => !a.IsBlue);
            return inputGraph;
        }
    }

    public class YellowVertexFilter : IGraphFilter<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>>
    {
        public BidirectionalGraph<DataVertex, DataEdge> ProcessFilter(BidirectionalGraph<DataVertex, DataEdge> inputGraph)
        {
            inputGraph.RemoveVertexIf(a => a.IsBlue);
            return inputGraph;
        }
    }
}
