using System.Collections.Generic;
using QuickGraph;

namespace GraphX.PCL.Logic.Algorithms.LayoutAlgorithms
{
    public partial class SimpleTreeLayoutAlgorithm<TVertex, TEdge, TGraph> where TVertex : class
        where TEdge : IEdge<TVertex>
        where TGraph : IBidirectionalGraph<TVertex, TEdge>, IMutableVertexAndEdgeSet<TVertex, TEdge>
    {
        protected class Layer
        {
            public double Size;
            public double NextPosition;
            public readonly IList<TVertex> Vertices = new List<TVertex>();
            public double LastTranslate;

            public Layer()
            {
                LastTranslate = 0;
            }

            /* Width and Height Optimization */

        }

       /// <summary>
       /// Vertex data class
       /// </summary>
       protected class VertexData
        {
            public TVertex Parent;
            public double Translate;
            public double Position;

            /* Width and Height Optimization */

        }
    }
}
