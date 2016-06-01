using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GraphX.Measure;
using GraphX.PCL.Common.Exceptions;
using QuickGraph;
using QuickGraph.Algorithms.Search;
using QuickGraph.Collections;

namespace GraphX.PCL.Logic.Algorithms.LayoutAlgorithms
{
    public partial class SimpleTreeLayoutAlgorithm<TVertex, TEdge, TGraph> : DefaultParameterizedLayoutAlgorithmBase<TVertex, TEdge, TGraph, SimpleTreeLayoutParameters>
        where TVertex : class
        where TEdge : IEdge<TVertex>
        where TGraph : IBidirectionalGraph<TVertex, TEdge>, IMutableVertexAndEdgeSet<TVertex, TEdge>
    {
        protected BidirectionalGraph<TVertex, Edge<TVertex>> SpanningTree;
        protected IDictionary<TVertex, Size> Sizes;
        protected readonly IDictionary<TVertex, VertexData> Data = new Dictionary<TVertex, VertexData>();
        protected readonly IList<Layer> Layers = new List<Layer>();
        private int _direction;

        public SimpleTreeLayoutAlgorithm( TGraph visitedGraph, IDictionary<TVertex, Point> vertexPositions, IDictionary<TVertex, Size> vertexSizes, SimpleTreeLayoutParameters parameters )
            : base( visitedGraph, vertexPositions, parameters )
        {
            //Contract.Requires( vertexSizes != null );
            //Contract.Requires( visitedGraph.Vertices.All( v => vertexSizes.ContainsKey( v ) ) );

        }

        public override void Compute(CancellationToken cancellationToken)
        {
            Sizes = new Dictionary<TVertex, Size>( VertexSizes );
            if ( Parameters.Direction == LayoutDirection.LeftToRight || Parameters.Direction == LayoutDirection.RightToLeft )
            {
                //change the sizes
                foreach ( var sizePair in Sizes.ToArray() )
                    Sizes[sizePair.Key] = new Size( sizePair.Value.Height, sizePair.Value.Width );
            }

            if ( Parameters.Direction == LayoutDirection.RightToLeft || Parameters.Direction == LayoutDirection.BottomToTop )
                _direction = -1;
            else
                _direction = 1;

            GenerateSpanningTree(cancellationToken);
            //DoWidthAndHeightOptimization();

            //first layout the vertices with 0 in-edge
            foreach ( var source in SpanningTree.Vertices.Where( v => SpanningTree.InDegree( v ) == 0 ) )
                CalculatePosition( source, null, 0 );

            //then the others
            foreach ( var source in SpanningTree.Vertices )
                CalculatePosition( source, null, 0 );

            AssignPositions(cancellationToken);
        }

        public override void ResetGraph(IEnumerable<TVertex> vertices, IEnumerable<TEdge> edges)
        {
            if (VisitedGraph == null && !TryCreateNewGraph())
                throw new GX_GeneralException("Can't create new graph through reflection. Make sure it support default constructor.");
            VisitedGraph.Clear();
            VisitedGraph.AddVertexRange(vertices);
            VisitedGraph.AddEdgeRange(edges);
        }

        protected virtual void GenerateSpanningTree(CancellationToken cancellationToken)
        {
            SpanningTree = new BidirectionalGraph<TVertex, Edge<TVertex>>( false );
            SpanningTree.AddVertexRange( VisitedGraph.Vertices );
            IQueue<TVertex> vb = new QuickGraph.Collections.Queue<TVertex>();
            vb.Enqueue( VisitedGraph.Vertices.OrderBy( v => VisitedGraph.InDegree( v ) ).First() );
            switch ( Parameters.SpanningTreeGeneration )
            {
                case SpanningTreeGeneration.BFS:
                    var bfsAlgo = new BreadthFirstSearchAlgorithm<TVertex, TEdge>( VisitedGraph, vb, new Dictionary<TVertex, GraphColor>() );
                    bfsAlgo.TreeEdge += e =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        SpanningTree.AddEdge(new Edge<TVertex>(e.Source, e.Target));
                    };
                    bfsAlgo.Compute();
                    break;
                case SpanningTreeGeneration.DFS:
                    var dfsAlgo = new DepthFirstSearchAlgorithm<TVertex, TEdge>( VisitedGraph );
                    dfsAlgo.TreeEdge += e =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        SpanningTree.AddEdge(new Edge<TVertex>(e.Source, e.Target));
                    };
                    dfsAlgo.Compute();
                    break;
            }
        }

        protected virtual double CalculatePosition( TVertex v, TVertex parent, int l )
        {
            if ( Data.ContainsKey( v ) )
                return -1; //this vertex is already layed out

            while ( l >= Layers.Count )
                Layers.Add( new Layer() );

            var layer = Layers[l];
            var size = Sizes[v];
            var d = new VertexData { Parent = parent };
            Data[v] = d;

            layer.NextPosition += size.Width / 2.0;
            if ( l > 0 )
            {
                layer.NextPosition += Layers[l - 1].LastTranslate;
                Layers[l - 1].LastTranslate = 0;
            }
            layer.Size = Math.Max( layer.Size, size.Height + Parameters.LayerGap );
            layer.Vertices.Add( v );
            if ( SpanningTree.OutDegree( v ) == 0 )
            {
                d.Position = layer.NextPosition;
            }
            else
            {
                double minPos = double.MaxValue;
                double maxPos = -double.MaxValue;
                //first put the children
                foreach ( var child in SpanningTree.OutEdges( v ).Select( e => e.Target ) )
                {
                    double childPos = CalculatePosition( child, v, l + 1 );
                    if ( childPos >= 0 )
                    {
                        minPos = Math.Min( minPos, childPos );
                        maxPos = Math.Max( maxPos, childPos );
                    }
                }
                if ( minPos != double.MaxValue )
                    d.Position = ( minPos + maxPos ) / 2.0;
                else
                    d.Position = layer.NextPosition;
                d.Translate = Math.Max( layer.NextPosition - d.Position, 0 );

                layer.LastTranslate = d.Translate;
                d.Position += d.Translate;
                layer.NextPosition = d.Position;
            }
            layer.NextPosition += size.Width / 2.0 + Parameters.VertexGap;

            return d.Position;
        }

        protected virtual void AssignPositions(CancellationToken cancellationToken)
        {
            double layerSize = 0;
            bool changeCoordinates = ( Parameters.Direction == LayoutDirection.LeftToRight || Parameters.Direction == LayoutDirection.RightToLeft );

            foreach ( var layer in Layers )
            {
                foreach ( var v in layer.Vertices )
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    Size size = Sizes[v];
                    var d = Data[v];
                    if ( d.Parent != null )
                    {
                        d.Position += Data[d.Parent].Translate;
                        d.Translate += Data[d.Parent].Translate;
                    }

                    VertexPositions[v] =
                        changeCoordinates
                            ? new Point( _direction * ( layerSize + size.Height / 2.0 ), d.Position )
                            : new Point( d.Position, _direction * ( layerSize + size.Height / 2.0 ) );
                }
                layerSize += layer.Size;
            }

            if ( _direction < 0 )
                NormalizePositions();
        }
    }
}