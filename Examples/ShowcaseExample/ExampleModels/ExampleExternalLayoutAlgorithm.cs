using GraphX.GraphSharp.Algorithms.Layout;
using GraphX.Measure;
using QuickGraph;
using System.Collections.Generic;
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

        IDictionary<DataVertex, Point> vertexPositions = new Dictionary<DataVertex, Point>();
        public IDictionary<DataVertex, Point> VertexPositions
        {
            get { return vertexPositions;  }
        }

        public IDictionary<DataVertex, Size> VertexSizes { get; set; }

        public bool NeedVertexSizes
        {
            get { return true; }
        }
    }
}
