using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuickGraph;

namespace GraphX.GraphSharp.Algorithms.Layout.Simple.Tree
{
    public partial class SimpleTreeLayoutAlgorithm<TVertex, TEdge, TGraph> : DefaultParameterizedLayoutAlgorithmBase<TVertex, TEdge, TGraph, SimpleTreeLayoutParameters>
        where TVertex : class
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
