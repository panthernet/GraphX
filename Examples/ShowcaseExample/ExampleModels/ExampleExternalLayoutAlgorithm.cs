using GraphX.GraphSharp.Algorithms.Layout;
using QuickGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GraphX.GraphSharp.Algorithms.Layout.Simple.Hierarchical;

/*
 External layout algorithm implementation example
 
 Also shows how to use internal algorithms inside the external one.
 
 */
namespace ShowcaseExample
{
    public class ExampleExternalLayoutAlgorithm: IExternalLayout<DataVertex>
    {
        private IVertexAndEdgeListGraph<DataVertex, DataEdge> Graph;
        public ExampleExternalLayoutAlgorithm(IVertexAndEdgeListGraph<DataVertex, DataEdge> graph)
        {
            Graph = graph;
        }

        public void Compute()
        {
            var pars = new EfficientSugiyamaLayoutParameters { LayerDistance = 200 };
            var algo = new EfficientSugiyamaLayoutAlgorithm<DataVertex, DataEdge, IVertexAndEdgeListGraph<DataVertex, DataEdge>>(Graph, pars, vertexPositions, VertexSizes);
            algo.Compute();

            // now you can use = algo.VertexPositions for custom manipulations

            //set this algo calculation results 
            vertexPositions = algo.VertexPositions;
        }

        IDictionary<DataVertex, System.Windows.Point> vertexPositions = new Dictionary<DataVertex, System.Windows.Point>();
        public IDictionary<DataVertex, System.Windows.Point> VertexPositions
        {
            get { return vertexPositions;  }
        }

        public IDictionary<DataVertex, System.Windows.Size> VertexSizes { get; set; }

        public bool NeedVertexSizes
        {
            get { return true; }
        }
    }
}
