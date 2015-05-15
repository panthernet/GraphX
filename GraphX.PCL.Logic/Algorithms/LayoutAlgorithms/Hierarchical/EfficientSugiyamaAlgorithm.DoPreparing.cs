using System.Collections.Generic;
using System.Linq;
using QuickGraph;
using QuickGraph.Algorithms.Search;

namespace GraphX.PCL.Logic.Algorithms.LayoutAlgorithms
{
    public partial class EfficientSugiyamaLayoutAlgorithm<TVertex, TEdge, TGraph>
        where TVertex : class
        where TEdge : IEdge<TVertex>
        where TGraph : IVertexAndEdgeListGraph<TVertex, TEdge>
    {
        private void DoPreparing()
        {
            RemoveCycles();
            RemoveLoops();
            RemoveIsolatedVertices(); //it must run after the two method above           
        }

        private void RemoveIsolatedVertices()
        {
            _isolatedVertices = _graph.Vertices.Where(v => _graph.Degree(v) == 0).ToList();
            foreach (var isolatedVertex in _isolatedVertices)
                _graph.RemoveVertex(isolatedVertex);
        }

        /// <summary>
        /// Removes the edges which source and target is the same vertex.
        /// </summary>
        private void RemoveLoops()
        {
            _graph.RemoveEdgeIf(edge => edge.Source == edge.Target);
        }

        /// <summary>
        /// Removes the cycles from the original graph with simply reverting
        /// some edges.
        /// </summary>
        private void RemoveCycles()
        {
            //find the cycle edges with dfs
            var cycleEdges = new List<SugiEdge>();
            var dfsAlgo = new DepthFirstSearchAlgorithm<SugiVertex, SugiEdge>(_graph);
            dfsAlgo.BackEdge += cycleEdges.Add;
            dfsAlgo.Compute();

            //and revert them
            foreach (var edge in cycleEdges)
            {
                _graph.RemoveEdge(edge);
                _graph.AddEdge(new SugiEdge(edge.OriginalEdge, edge.Target, edge.Source));
            }
        }
	}
}
