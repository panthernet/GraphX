using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GraphX.Measure;
using QuickGraph;
using QuickGraph.Algorithms.Search;
using QuickGraph.Collections;

namespace GraphX.GraphSharp.Algorithms.Layout.Simple.Tree
{
    public partial class SimpleTreeLayoutAlgorithm<TVertex, TEdge, TGraph> : DefaultParameterizedLayoutAlgorithmBase<TVertex, TEdge, TGraph, SimpleTreeLayoutParameters>
        where TVertex : class
        where TEdge : IEdge<TVertex>
        where TGraph : IBidirectionalGraph<TVertex, TEdge>
    {
        private BidirectionalGraph<TVertex, Edge<TVertex>> spanningTree;
        readonly IDictionary<TVertex, Size> sizes;
        readonly IDictionary<TVertex, VertexData> data = new Dictionary<TVertex, VertexData>();
        readonly IList<Layer> layers = new List<Layer>();
        int direction;

        public SimpleTreeLayoutAlgorithm( TGraph visitedGraph, IDictionary<TVertex, Point> vertexPositions, IDictionary<TVertex, Size> vertexSizes, SimpleTreeLayoutParameters parameters )
            : base( visitedGraph, vertexPositions, parameters )
        {
            //Contract.Requires( vertexSizes != null );
            //Contract.Requires( visitedGraph.Vertices.All( v => vertexSizes.ContainsKey( v ) ) );

            sizes = new Dictionary<TVertex, Size>( vertexSizes );
        }

        public override void Compute(CancellationToken cancellationToken)
        {
            if ( Parameters.Direction == LayoutDirection.LeftToRight || Parameters.Direction == LayoutDirection.RightToLeft )
            {
                //change the sizes
                foreach ( var sizePair in sizes.ToArray() )
                    sizes[sizePair.Key] = new Size( sizePair.Value.Height, sizePair.Value.Width );
            }

            if ( Parameters.Direction == LayoutDirection.RightToLeft || Parameters.Direction == LayoutDirection.BottomToTop )
                direction = -1;
            else
                direction = 1;

            GenerateSpanningTree(cancellationToken);
            //DoWidthAndHeightOptimization();

            //first layout the vertices with 0 in-edge
            foreach ( var source in spanningTree.Vertices.Where( v => spanningTree.InDegree( v ) == 0 ) )
                CalculatePosition( source, null, 0 );

            //then the others
            foreach ( var source in spanningTree.Vertices )
                CalculatePosition( source, null, 0 );

            AssignPositions(cancellationToken);
        }

        private void GenerateSpanningTree(CancellationToken cancellationToken)
        {
            spanningTree = new BidirectionalGraph<TVertex, Edge<TVertex>>( false );
            spanningTree.AddVertexRange( VisitedGraph.Vertices );
            IQueue<TVertex> vb = new QuickGraph.Collections.Queue<TVertex>();
            vb.Enqueue( VisitedGraph.Vertices.OrderBy( v => VisitedGraph.InDegree( v ) ).First() );
            switch ( Parameters.SpanningTreeGeneration )
            {
                case SpanningTreeGeneration.BFS:
                    var bfsAlgo = new BreadthFirstSearchAlgorithm<TVertex, TEdge>( VisitedGraph, vb, new Dictionary<TVertex, GraphColor>() );
                    bfsAlgo.TreeEdge += e =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        spanningTree.AddEdge(new Edge<TVertex>(e.Source, e.Target));
                    };
                    bfsAlgo.Compute();
                    break;
                case SpanningTreeGeneration.DFS:
                    var dfsAlgo = new DepthFirstSearchAlgorithm<TVertex, TEdge>( VisitedGraph );
                    dfsAlgo.TreeEdge += e =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        spanningTree.AddEdge(new Edge<TVertex>(e.Source, e.Target));
                    };
                    dfsAlgo.Compute();
                    break;
            }
        }

        protected double CalculatePosition( TVertex v, TVertex parent, int l )
        {
            if ( data.ContainsKey( v ) )
                return -1; //this vertex is already layed out

            while ( l >= layers.Count )
                layers.Add( new Layer() );

            var layer = layers[l];
            var size = sizes[v];
            var d = new VertexData { parent = parent };
            data[v] = d;

            layer.NextPosition += size.Width / 2.0;
            if ( l > 0 )
            {
                layer.NextPosition += layers[l - 1].LastTranslate;
                layers[l - 1].LastTranslate = 0;
            }
            layer.Size = Math.Max( layer.Size, size.Height + Parameters.LayerGap );
            layer.Vertices.Add( v );
            if ( spanningTree.OutDegree( v ) == 0 )
            {
                d.position = layer.NextPosition;
            }
            else
            {
                double minPos = double.MaxValue;
                double maxPos = -double.MaxValue;
                //first put the children
                foreach ( var child in spanningTree.OutEdges( v ).Select( e => e.Target ) )
                {
                    double childPos = CalculatePosition( child, v, l + 1 );
                    if ( childPos >= 0 )
                    {
                        minPos = Math.Min( minPos, childPos );
                        maxPos = Math.Max( maxPos, childPos );
                    }
                }
                if ( minPos != double.MaxValue )
                    d.position = ( minPos + maxPos ) / 2.0;
                else
                    d.position = layer.NextPosition;
                d.translate = Math.Max( layer.NextPosition - d.position, 0 );

                layer.LastTranslate = d.translate;
                d.position += d.translate;
                layer.NextPosition = d.position;
            }
            layer.NextPosition += size.Width / 2.0 + Parameters.VertexGap;

            return d.position;
        }

        protected void AssignPositions(CancellationToken cancellationToken)
        {
            double layerSize = 0;
            bool changeCoordinates = ( Parameters.Direction == LayoutDirection.LeftToRight || Parameters.Direction == LayoutDirection.RightToLeft );

            foreach ( var layer in layers )
            {
                foreach ( var v in layer.Vertices )
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    Size size = sizes[v];
                    var d = data[v];
                    if ( d.parent != null )
                    {
                        d.position += data[d.parent].translate;
                        d.translate += data[d.parent].translate;
                    }

                    VertexPositions[v] =
                        changeCoordinates
                            ? new Point( direction * ( layerSize + size.Height / 2.0 ), d.position )
                            : new Point( d.position, direction * ( layerSize + size.Height / 2.0 ) );
                }
                layerSize += layer.Size;
            }

            if ( direction < 0 )
                NormalizePositions();
        }
    }
}