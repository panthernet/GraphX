using QuickGraph;

namespace GraphX.PCL.Logic.Algorithms.LayoutAlgorithms
{
    public partial class SimpleTreeLayoutAlgorithm<TVertex, TEdge, TGraph> where TVertex : class
        where TEdge : IEdge<TVertex>
        where TGraph : IBidirectionalGraph<TVertex, TEdge>
    {
        /*private void DoWidthAndHeightOptimization()
        {
            CreateVertexWHOptInfos();
            CreateLayerWHOptInfos();

            if (_actualWidthPerHeight <= Parameters.WidthPerHeight)
                return;

            bool optimized = false;
            do
            {
                optimized = DoWHOptimizationStep();
            } while (optimized);
            RewriteLayerIndexes();
        }

        private void CreateVertexWHOptInfos()
        {

        }
        private void CreateLayerWHOptInfos()
        {
            
        }
        private bool DoWHOptimizationStep()
        {
            
        }
        private void RewriteLayerIndexes()
        {
            
        }*/
    }
}
