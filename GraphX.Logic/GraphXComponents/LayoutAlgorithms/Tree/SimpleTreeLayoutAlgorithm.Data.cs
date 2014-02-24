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
        class Layer
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

        class VertexData
        {
            public TVertex parent;
            public double translate;
            public double position;

            /* Width and Height Optimization */

        }
    }
}
