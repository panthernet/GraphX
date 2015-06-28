using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GraphX.Measure;
using QuickGraph;
using QuickGraph.Algorithms.Search;

namespace GraphX.PCL.Logic.Algorithms.LayoutAlgorithms
{
    public partial class SugiyamaLayoutAlgorithm<TVertex, TEdge, TGraph> : DefaultParameterizedLayoutAlgorithmBase<TVertex, TEdge, TGraph, SugiyamaLayoutParameters>
        where TVertex : class
        where TEdge : IEdge<TVertex>
        where TGraph : IVertexAndEdgeListGraph<TVertex, TEdge>
    {
        #region Private fields, constants

        SoftMutableHierarchicalGraph<SugiVertex, SugiEdge> _graph;

        readonly Func<TEdge, EdgeTypes> _edgePredicate;
        readonly VertexLayerCollection _layers = new VertexLayerCollection();
        //private int _iteration;
        double _statusInPercent;

        private const int PERCENT_OF_PREPARATION = 5;
        private const int PERCENT_OF_SUGIYAMA = 60;
        private const int PERCENT_OF_INCREMENTAL_EXTENSION = 30;

        private const string PARALLEL_EDGES_TAG = "ParallelEdges";
        /*
                private const string CyclesTag = "Cycles";
        */
        private const string ISOLATED_VERTICES_TAG = "IsolatedVertices";
        private const string LOOPS_TAG = "Loops";
        private const string GENERAL_EDGES_TAG = "GeneralEdges";
        private const string GENERAL_EDGES_BETWEEN_DIFFERENT_LAYERS_TAG = "GeneralEdgesBetweenDifferentLayers";
        private const string LONG_EDGES_TAG = "LongEdges"; //long edges will be replaced with dummy vertices
        #endregion

        public IDictionary<TEdge, Point[]> EdgeRoutes { get; private set; }

        #region Constructors
        public SugiyamaLayoutAlgorithm(
            TGraph visitedGraph,
            IDictionary<TVertex, Size> vertexSizes,
            IDictionary<TVertex, Point> vertexPositions,
            SugiyamaLayoutParameters parameters,
            Func<TEdge, EdgeTypes> edgePredicate )
            : base( visitedGraph, vertexPositions, parameters )
        {
            _edgePredicate = edgePredicate;
            EdgeRoutes = new Dictionary<TEdge, Point[]>();

            ConvertGraph( vertexSizes );
        }

        public SugiyamaLayoutAlgorithm(
            TGraph visitedGraph,
            IDictionary<TVertex, Size> vertexSizes,
            SugiyamaLayoutParameters parameters,
            Func<TEdge, EdgeTypes> edgePredicate )
            : this( visitedGraph, vertexSizes, null, parameters, edgePredicate )
        {
        }

        /// <summary>
        /// Converts the VisitedGraph to the inner type (which is a mutable graph representation).
        /// Wraps the vertices, converts the edges.
        /// </summary>
        protected void ConvertGraph( IDictionary<TVertex, Size> vertexSizes )
        {
            //creating the graph with the new type
            _graph = new SoftMutableHierarchicalGraph<SugiVertex, SugiEdge>( true );

            var vertexDict = new Dictionary<TVertex, SugiVertex>();

            //wrapping the vertices
            foreach ( TVertex v in VisitedGraph.Vertices )
            {
                var size = vertexSizes[v];
                size.Height += Parameters.VerticalGap;
                size.Width += Parameters.HorizontalGap;
                var wrapped = new SugiVertex( v, size );

                _graph.AddVertex( wrapped );
                vertexDict[v] = wrapped;
            }

            //creating the new edges
            foreach ( TEdge e in VisitedGraph.Edges )
            {
                var wrapped = new SugiEdge( e, vertexDict[e.Source], vertexDict[e.Target], _edgePredicate( e ) );
                _graph.AddEdge( wrapped );
            }
        }
        #endregion

        #region Filters - used in the preparation phase
        /// <summary>
        /// Removes the cycles from the given graph.
        /// It reverts some edges, so the cycles disappeares.
        /// </summary>
        private void FilterCycles()
        {
            var cycleEdges = new List<SugiEdge>();
            var dfsAlgo = new DepthFirstSearchAlgorithm<SugiVertex, SugiEdge>( _graph );
            dfsAlgo.BackEdge += cycleEdges.Add;
            //non-tree edges selected
            dfsAlgo.Compute();

            //put back the reverted ones
            foreach ( var edge in cycleEdges )
            {
                _graph.RemoveEdge( edge );

                var revertEdge = new SugiEdge( edge.Original, edge.Target, edge.Source, edge.Type );
                _graph.AddEdge( revertEdge );
            }
        }

        protected static void FilterParallelEdges<TVertexType, TEdgeType>( ISoftMutableGraph<TVertexType, TEdgeType> graph )
            where TEdgeType : class, IEdge<TVertexType>
        {
            foreach ( TVertexType v in graph.Vertices )
            {
                var neighbours = new HashSet<TVertexType>();
                foreach ( var e in graph.OutEdges( v ).ToList() )
                {
                    if ( !neighbours.Add( e.Target ) )
                        //target already a neighbour, it is a parallel edge
                        graph.HideEdge( e, PARALLEL_EDGES_TAG );
                }
                foreach ( var e in graph.InEdges( v ).ToList() )
                {
                    if ( !neighbours.Add( e.Source ) )
                        //source already a neighbour, it is a parallel edge
                        graph.HideEdge( e, PARALLEL_EDGES_TAG );
                }
            }
        }

        protected static void FilterIsolatedVertices<TVertexType, TEdgeType>( ISoftMutableGraph<TVertexType, TEdgeType> graph )
            where TEdgeType : class, IEdge<TVertexType>
        {
            graph.HideVerticesIf( v => graph.Degree( v ) == 0, ISOLATED_VERTICES_TAG );
        }

        protected static void FilterLoops<TVertexType, TEdgeType>( ISoftMutableGraph<TVertexType, TEdgeType> graph )
            where TEdgeType : class, IEdge<TVertexType>
            where TVertexType : class
        {
            graph.HideEdgesIf( e => e.Source == e.Target, LOOPS_TAG );
        }

        /// <summary>
        /// First step of the algorithm.
        /// Filters the unappropriate vertices and edges.
        /// </summary>
        protected void FiltersAndRemovals()
        {
            //hide every edge but hierarchical ones
            _graph.HideEdges( _graph.GeneralEdges, GENERAL_EDGES_TAG );

            //Remove the cycles from the graph
            FilterCycles();

            //remove parallel edges
            //FilterParallelEdges( _graph );

            //remove every isolated vertex
            FilterIsolatedVertices( _graph );

            //filter loops - edges with source = target
            FilterLoops( _graph );
        }
        #endregion

        /// <summary>
        /// Creates the layering of the graph. (Assigns every vertex to a layer.)
        /// </summary>
        protected void AssignLayers()
        {
            var lts = new LayeredTopologicalSortAlgorithm<SugiVertex, SugiEdge>( _graph );
            lts.Compute();

            for ( int i = 0; i < lts.LayerCount; i++ )
            {
                var vl = new VertexLayer( _graph, i, lts.Layers[i].ToList() );
                _layers.Add( vl );
            }
        }

        #region Preparation for Sugiyama
        /// <summary>
        /// Minimizes the long of the hierarchical edges by 
        /// putting down the vertices to the layer above  
        /// its descendants.
        /// </summary>
        protected void MinimizeHierarchicalEdgeLong(CancellationToken cancellationToken)
        {
            if ( !Parameters.MinimizeHierarchicalEdgeLong )
                return;

            for ( int i = _layers.Count - 1; i >= 0; i-- )
            {
                var layer = _layers[i];
                foreach ( var v in layer.ToList() )
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if ( _graph.OutHierarchicalEdgeCount( v ) == 0 ) continue;

                    //put the vertex above the descendant on the highest layer
                    int newLayerIndex = _graph.OutHierarchicalEdges( v ).Min( edge => edge.Target.LayerIndex - 1 );

                    if ( newLayerIndex != v.LayerIndex )
                    {
                        //we're changing layer
                        layer.Remove( v );
                        _layers[newLayerIndex].Add( v );
                    }
                }
            }
        }

        /// <summary>
        /// Long edges ( span(e) > 1 ) will be replaced by 
        /// span(e) edges (1 edge between every 2 neighbor layer)
        /// and span(e)-1 dummy vertices will be added to graph.
        /// </summary>
        protected void ReplaceLongEdges(CancellationToken cancellationToken)
        {
            //if an edge goes through multiple layers, we split the edge at every layer and insert a dummy node
            //  (only for the hierarchical edges)
            foreach ( var edge in _graph.HierarchicalEdges.ToArray() )
            {
                int sourceLayerIndex = edge.Source.LayerIndex;
                int targetLayerIndex = edge.Target.LayerIndex;

                if ( Math.Abs( sourceLayerIndex - targetLayerIndex ) <= 1 )
                    continue; //span(e) <= 1, not long edge

                //the edge goes through multiple layers
                edge.IsLongEdge = true;
                _graph.HideEdge( edge, LONG_EDGES_TAG );

                //sourcelayer must be above the targetlayer
                if ( targetLayerIndex < sourceLayerIndex )
                {
                    int c = targetLayerIndex;
                    targetLayerIndex = sourceLayerIndex;
                    sourceLayerIndex = c;
                }

                SugiVertex prev = edge.Source;
                for ( int i = sourceLayerIndex + 1; i <= targetLayerIndex; i++ )
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    //the last vertex is the Target, the other ones are dummy vertices
                    SugiVertex dummy;
                    if ( i == targetLayerIndex )
                        dummy = edge.Target;
                    else
                    {
                        dummy = new SugiVertex( null, new Size( 0, 0 ) );
                        _graph.AddVertex( dummy );
                        _layers[i].Add( dummy );
                        edge.DummyVertices.Add( dummy );
                    }
                    _graph.AddEdge( new SugiEdge( edge.Original, prev, dummy, EdgeTypes.Hierarchical ) );
                    prev = dummy;
                }
            }
        }

        protected void ConstrainWidth()
        {
            //TODO implement
        }

        protected void PrepareForSugiyama(CancellationToken cancellationToken)
        {
            MinimizeHierarchicalEdgeLong(cancellationToken);

            #region 1) Unhide general edges between vertices participating in the hierarchy
            var analyze = new HashSet<SugiVertex>();
            EdgeAction<SugiVertex, SugiEdge> eeh =
                edge =>
                {
                    analyze.Add( edge.Source );
                    analyze.Add( edge.Target );
                };
            _graph.EdgeUnhidden += eeh;
            _graph.UnhideEdgesIf( e => e.Type == EdgeTypes.General && _graph.ContainsVertex( e.Source ) && _graph.ContainsVertex( e.Target ) );
            _graph.EdgeUnhidden -= eeh;
            #endregion

            #region 2) Move the vertices with general edges if possible
            foreach ( var v in analyze )
            {
                //csak lejjebb lehet rakni
                //csak akkor, ha nincs hierarchikus kimeno el lefele
                //a legkozelebbi lehetseges szintre
                if ( _graph.OutHierarchicalEdgeCount( v ) == 0 )
                {
                    //az altalanos elek kozul az alattalevok kozul a legkozelebbibre kell rakni
                    int newLayerIndex = _layers.Count;
                    foreach ( var e in _graph.InGeneralEdges( v ) )
                    {
                        //nem erdemes tovabb folytatni, lejebb nem kerulhet
                        if ( newLayerIndex == v.LayerIndex ) break;
                        if ( e.Source.LayerIndex >= v.LayerIndex && e.Source.LayerIndex < newLayerIndex )
                            newLayerIndex = e.Source.LayerIndex;
                    }
                    foreach ( var e in _graph.OutGeneralEdges( v ) )
                    {
                        //nem erdemes tovabb folytatni, lejebb nem kerulhet
                        if ( newLayerIndex == v.LayerIndex ) break;
                        if ( e.Target.LayerIndex >= v.LayerIndex && e.Target.LayerIndex < newLayerIndex )
                            newLayerIndex = e.Target.LayerIndex;
                    }
                    if ( newLayerIndex < _layers.Count )
                    {
                        _layers[v.LayerIndex].Remove( v );
                        _layers[newLayerIndex].Add( v );
                    }
                }
            }
            #endregion

            // 3) Hide the general edges between different layers
            _graph.HideEdgesIf( e => ( e.Type == EdgeTypes.General && e.Source.LayerIndex != e.Target.LayerIndex ), GENERAL_EDGES_BETWEEN_DIFFERENT_LAYERS_TAG );

            //replace long edges with more segments and dummy vertices
            ReplaceLongEdges(cancellationToken);

            ConstrainWidth();

            CopyPositions(cancellationToken);
            OnIterationEnded( "Preparation of the positions done." );
        }
        #endregion

        #region Sugiyama Layout
        /// <summary>
        /// Sweeps in one direction in the 1st Phase of the Sugiyama's algorithm.
        /// </summary>
        /// <param name="start">The index of the layer where the sweeping starts.</param>
        /// <param name="end">The index of the layer where the sweeping ends.</param>
        /// <param name="step">Stepcount.</param>
        /// <param name="baryCenter">Kind of the barycentering (Up/Down-barycenter).</param>
        /// <param name="dirty">If this is a dirty sweep</param>
        /// <param name="byRealPosition"></param>
        /// <returns></returns>
        protected bool SugiyamaPhase1Sweep( int start, int end, int step, BaryCenter baryCenter, bool dirty, bool byRealPosition, CancellationToken cancellationToken )
        {
            bool hasOptimization = false;
            CrossCount crossCounting = baryCenter == BaryCenter.Up ? CrossCount.Up : CrossCount.Down;
            bool sourceByMeasure = crossCounting == CrossCount.Down;
            for ( int i = start; i != end; i += step )
            {
                var layer = _layers[i];
                int modifiedCrossing = 0;
                int originalCrossing = 0;

                if ( !dirty )
                    //get the count of the edge crossings
                    originalCrossing = layer.CalculateCrossCount( crossCounting );

                //measure the vertices by the given barycenter
                layer.Measure( baryCenter, byRealPosition );

                if ( !dirty )
                    //get the modified crossing count
                    modifiedCrossing = layer.CalculateCrossCount( crossCounting, sourceByMeasure, !sourceByMeasure );

                if ( modifiedCrossing < originalCrossing || dirty )
                {
                    layer.SortByMeasure();
                    hasOptimization = true;
                }

                if ( byRealPosition )
                {
                    HorizontalPositionAssignmentOnLayer( i, baryCenter, cancellationToken );
                    CopyPositionsSilent( false );
                }
                else
                {
                    CopyPositions(cancellationToken);
                }
                OnIterationEnded( " Phase 1 sweepstep finished [" + baryCenter + "-barycentering on layer " + i + "]" );
            }
            return hasOptimization;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="step"></param>
        /// <param name="baryCenter"></param>
        /// <param name="byRealPosition"></param>
        /// <returns>The index of the layer which is not ordered by <paramref name="baryCenter"/> anymore.
        /// If all of the layers ordered, and phase2 sweep done it returns with -1.</returns>
        protected int SugiyamaPhase2Sweep( int start, int end, int step, BaryCenter baryCenter, bool byRealPosition, CancellationToken cancellationToken )
        {
            CrossCount crossCountDirection = baryCenter == BaryCenter.Up ? CrossCount.Up : CrossCount.Down;
            for ( int i = start; i != end; i += step )
            {
                var layer = _layers[i];

                //switch the vertices with the same barycenters, if and only if there will be less barycenters
                layer.Measure( baryCenter, byRealPosition );
                layer.FindBestPermutation( crossCountDirection );

                if ( byRealPosition )
                {
                    HorizontalPositionAssignmentOnLayer( i, baryCenter, cancellationToken );
                    CopyPositionsSilent( false );
                }
                else
                {
                    CopyPositions(cancellationToken);
                }
                OnIterationEnded( " Phase 2 sweepstep finished [" + baryCenter + "-barycentering on layer " + i + "]" );
                if ( i + step != end )
                {
                    var nextLayer = _layers[i + step];
                    if ( !nextLayer.IsOrderedByBaryCenters( baryCenter, byRealPosition ) )
                        return ( i + step );
                }
            }
            return -1;
        }

        protected void SugiyamaDirtyPhase( bool byRealPosition, CancellationToken cancellationToken )
        {
            if ( _layers.Count < 2 )
                return;

            const bool dirty = true;
            SugiyamaPhase1Sweep( 1, _layers.Count, 1, BaryCenter.Up, dirty, byRealPosition, cancellationToken );
            SugiyamaPhase1Sweep( _layers.Count - 2, -1, -1, BaryCenter.Down, dirty, byRealPosition, cancellationToken );
        }

        protected bool SugiyamaPhase1( int startLayerIndex, BaryCenter startBaryCentering, bool ByRealPosition, CancellationToken cancellationToken )
        {
            if ( _layers.Count < 2 ) return false;

            const bool dirty = false;
            bool sweepDownOptimized = false;

            if ( startBaryCentering == BaryCenter.Up )
            {
                sweepDownOptimized = SugiyamaPhase1Sweep( startLayerIndex == -1 ? 1 : startLayerIndex, _layers.Count, 1, BaryCenter.Up, dirty, ByRealPosition, cancellationToken );
                startLayerIndex = -1;
            }

            bool sweepUpOptimized = SugiyamaPhase1Sweep( startLayerIndex == -1 ? _layers.Count - 2 : startLayerIndex, -1, -1, BaryCenter.Down, dirty, ByRealPosition, cancellationToken );

            return sweepUpOptimized || sweepDownOptimized;
        }

        protected bool SugiyamaPhase1( bool byRealPosition, CancellationToken cancellationToken )
        {
            return SugiyamaPhase1( -1, BaryCenter.Up, byRealPosition, cancellationToken );
        }

        protected bool SugiyamaPhase2( out int unorderedLayerIndex, out BaryCenter baryCentering, bool byRealPosition, CancellationToken cancellationToken )
        {
            //Sweeping up
            unorderedLayerIndex = SugiyamaPhase2Sweep( 1, _layers.Count, 1, BaryCenter.Up, byRealPosition, cancellationToken );
            if ( unorderedLayerIndex != -1 )
            {
                baryCentering = BaryCenter.Up;
                return false;
            }

            //Sweeping down
            unorderedLayerIndex = SugiyamaPhase2Sweep( _layers.Count - 2, -1, -1, BaryCenter.Down, byRealPosition, cancellationToken );
            baryCentering = BaryCenter.Down;
            if ( unorderedLayerIndex != -1 )
                return false;

            //Phase 2 done
            return true;
        }

        protected void SugiyamaLayout(CancellationToken cancellationToken)
        {
            bool baryCenteringByRealPositions = Parameters.PositionCalculationMethod == PositionCalculationMethodTypes.PositionBased;
            if ( Parameters.DirtyRound )
                //start with a dirty round (sort by barycenters, even if the number of the crossings will rise)
                SugiyamaDirtyPhase( baryCenteringByRealPositions, cancellationToken );

            bool changed = true;
            int iteration1Left = Parameters.Phase1IterationCount;
            int iteration2Left = Parameters.Phase2IterationCount;
            double maxIterations = iteration1Left * iteration2Left;

            int startLayerIndex = -1;
            BaryCenter startBaryCentering = BaryCenter.Up;

            while ( changed && ( iteration1Left > 0 || iteration2Left > 0 ) )
            {
                changed = false;

                //
                // Phase 1 - while there's any optimization
                //
                while ( iteration1Left > 0 && SugiyamaPhase1( startLayerIndex, startBaryCentering, baryCenteringByRealPositions, cancellationToken ) )
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    iteration1Left--;
                    changed = true;
                }

                startLayerIndex = -1;
                startBaryCentering = BaryCenter.Up;

                //
                // Phase 2
                //
                if ( iteration2Left > 0 )
                {
                    SugiyamaPhase2( out startLayerIndex, out startBaryCentering, baryCenteringByRealPositions, cancellationToken );
                    iteration2Left--;
                }

                // Phase fallback
                if ( startLayerIndex != -1 )
                {
                    iteration1Left = Parameters.Phase1IterationCount;
                    changed = true;
                }

                _statusInPercent += PERCENT_OF_SUGIYAMA / maxIterations;
            }

            #region Mark the neighbour vertices connected with associative edges
            foreach ( SugiEdge e in _graph.GeneralEdges )
            {
                int sourceIndex = _layers[e.Source.LayerIndex].IndexOf( e.Source );
                int targetIndex = _layers[e.Target.LayerIndex].IndexOf( e.Target );
                bool shouldBeMarked = e.Source.LayerIndex == e.Target.LayerIndex && Math.Abs( sourceIndex - targetIndex ) == 1;
                if ( shouldBeMarked )
                {
                    if ( sourceIndex < targetIndex )
                    {
                        e.Source.RightGeneralEdgeCount += 1;
                        e.Target.LeftGeneralEdgeCount += 1;
                    }
                    else
                    {
                        e.Target.RightGeneralEdgeCount += 1;
                        e.Source.LeftGeneralEdgeCount += 1;
                    }
                }
            }
            #endregion
        }
        #endregion

        #region Last phase - Horizontal Assignment, edge routing, copying of the positions
        protected void AssignPriorities(CancellationToken cancellationToken)
        {
            foreach (var v in _graph.Vertices)
            {
                cancellationToken.ThrowIfCancellationRequested();
                v.Priority = (v.IsDummyVertex ? int.MaxValue : _graph.HierarchicalEdgeCountFor(v));
            }
        }

        private double CalculateOverlap( SugiVertex a, SugiVertex b )
        {
            return CalculateOverlap( a, b, 0 );
        }

        private double CalculateOverlap( SugiVertex a, SugiVertex b, double plusGap )
        {
            return Math.Max( 0, ( ( b.Size.Width + a.Size.Width ) * 0.5 + plusGap + Parameters.HorizontalGap ) - ( b.RealPosition.X - a.RealPosition.X ) );
        }

        protected void HorizontalPositionAssignmentOnLayer( int layerIndex, BaryCenter baryCenter, CancellationToken cancellationToken )
        {
            var layer = _layers[layerIndex];

            //compute where the vertices should be placed
            layer.Measure( baryCenter, true );
            layer.CalculateSubPriorities();

            //set the RealPositions to NaN
            foreach ( var v in layer )
                v.RealPosition.X = float.NaN;

            //set the positions in the order of the priorities, start with the lower priorities
            foreach ( var v in from vertex in layer
                               orderby vertex.Priority ascending, vertex.SubPriority ascending
                               select vertex )
            {
                //first set the new position
                v.RealPosition.X = v.Measure;

                //check if there's any overlap between the actual vertex and the vertices which position has already been set
                SugiVertex v1 = v;
                var alreadySetVertices = layer.Where( vertex => ( !double.IsNaN( vertex.RealPosition.X ) && vertex != v1 ) ).ToArray();

                if ( alreadySetVertices.Length == 0 )
                {
                    //there can't be any overlap
                    continue;
                }

                //get the index of the 'v' vertex between the vertices which position has already been set
                int indexOfV;
                for (indexOfV = 0;
                    indexOfV < alreadySetVertices.Length && alreadySetVertices[indexOfV].Position < v.Position;
                    indexOfV++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }

                SugiVertex leftNeighbor = null, rightNeighbor = null;
                double leftOverlap = 0, rightOverlap = 0;

                //check the overlap with vertex on the left
                if ( indexOfV > 0 )
                {
                    leftNeighbor = alreadySetVertices[indexOfV - 1];
                    leftOverlap = CalculateOverlap( leftNeighbor, v );
                }
                if ( indexOfV < alreadySetVertices.Length )
                {
                    rightNeighbor = alreadySetVertices[indexOfV];
                    rightOverlap = CalculateOverlap( v, rightNeighbor );
                }

                // ReSharper disable PossibleNullReferenceException
                //only one neighbor overlaps
                if ( leftOverlap > 0 && rightOverlap == 0 )
                {
                    if ( leftNeighbor.Priority == v.Priority )
                    {
                        double leftMove = leftOverlap * 0.5;
                        if ( rightNeighbor != null )
                            rightOverlap = CalculateOverlap( v, rightNeighbor, leftMove );
                        leftNeighbor.RealPosition.X -= leftMove;
                        v.RealPosition.X += leftMove;
                        if ( rightOverlap > 0 )
                        {
                            if ( v.Priority == rightNeighbor.Priority )
                            {
                                double rightMove = rightOverlap * 0.5;
                                rightNeighbor.RealPosition.X += rightMove;
                                v.RealPosition.X -= rightMove;
                                leftNeighbor.RealPosition.X -= rightMove;
                            }
                            else
                            {
                                rightNeighbor.RealPosition.X += rightOverlap;
                            }
                        }
                    }
                    else
                    {
                        leftNeighbor.RealPosition.X -= leftOverlap;
                    }
                }
                else if ( leftOverlap == 0 && rightOverlap > 0 )
                {
                    if ( v.Priority == rightNeighbor.Priority )
                    {
                        double rightMove = rightOverlap * 0.5;
                        if ( leftNeighbor != null )
                            leftOverlap = CalculateOverlap( leftNeighbor, v, rightMove );
                        rightNeighbor.RealPosition.X += rightMove;
                        v.RealPosition.X -= rightMove;
                        if ( leftOverlap > 0 )
                        {
                            if ( leftNeighbor.Priority == v.Priority )
                            {
                                double leftMove = leftOverlap * 0.5;
                                leftNeighbor.RealPosition.X -= leftMove;
                                v.RealPosition.X += leftMove;
                                rightNeighbor.RealPosition.X += leftMove;
                            }
                            else
                            {
                                leftNeighbor.RealPosition.X -= leftOverlap;
                            }
                        }
                    }
                    else
                    {
                        rightNeighbor.RealPosition.X += rightOverlap;
                    }
                }
                else if ( leftOverlap > 0 && rightOverlap > 0 )
                {
                    //if both neighbor overlapped
                    //priorities equals, 1 priority lower, 2 priority lower
                    if ( leftNeighbor.Priority < v.Priority && v.Priority == rightNeighbor.Priority )
                    {
                        double rightMove = rightOverlap * 0.5;
                        rightNeighbor.RealPosition.X += rightMove;
                        v.RealPosition.X -= rightMove;
                        leftNeighbor.RealPosition.X -= ( leftOverlap + rightMove );
                    }
                    else if ( leftNeighbor.Priority == v.Priority && v.Priority > rightNeighbor.Priority )
                    {
                        double leftMove = leftOverlap * 0.5;
                        leftNeighbor.RealPosition.X -= leftMove;
                        v.RealPosition.X += leftMove;
                        rightNeighbor.RealPosition.X = ( rightOverlap + leftMove );
                    }
                    else
                    {
                        //priorities of the neighbors are lower, or equal
                        leftNeighbor.RealPosition.X -= leftOverlap;
                        rightNeighbor.RealPosition.X += rightOverlap;
                    }
                }
                // ReSharper restore PossibleNullReferenceException

                //the vertices on the left side of the leftNeighbor will be moved, if they overlap
                if ( leftOverlap > 0 )
                    for ( int index = indexOfV - 1;
                          index > 0
                          && ( leftOverlap = CalculateOverlap( alreadySetVertices[index - 1], alreadySetVertices[index] ) ) > 0;
                          index-- )
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        alreadySetVertices[index - 1].RealPosition.X -= leftOverlap;
                    }

                //the vertices on the right side of the rightNeighbor will be moved, if they overlap
                if ( rightOverlap > 0 )
                    for ( int index = indexOfV;
                          index < alreadySetVertices.Length - 1
                          && ( rightOverlap = CalculateOverlap( alreadySetVertices[index], alreadySetVertices[index + 1] ) ) > 0;
                          index++ )
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        alreadySetVertices[index + 1].RealPosition.X += rightOverlap;
                    }
            }
        }

        protected void HorizontalPositionAssignmentSweep( int start, int end, int step, BaryCenter baryCenter, CancellationToken cancellationToken )
        {
            for ( int i = start; i != end; i += step )
                HorizontalPositionAssignmentOnLayer( i, baryCenter, cancellationToken );
        }

        protected void HorizontalPositionAssignment(CancellationToken cancellationToken)
        {
            //sweeping up & down, assigning the positions for the vertices in the order of the priorities
            //positions computed with the barycenter method, based on the realpositions
            AssignPriorities(cancellationToken);

            if ( _layers.Count > 1 )
            {
                HorizontalPositionAssignmentSweep( 1, _layers.Count, 1, BaryCenter.Up, cancellationToken );
                HorizontalPositionAssignmentSweep( _layers.Count - 2, -1, -1, BaryCenter.Down, cancellationToken );
            }
        }

        protected void AssignPositions(CancellationToken cancellationToken)
        {
            //initialize positions
            double verticalPos = 0;
            for ( int i = 0; i < _layers.Count; i++ )
            {
                double pos = 0;
                double layerHeight = _layers[i].Height;
                foreach ( var v in _layers[i] )
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    v.RealPosition.X = pos;
                    v.RealPosition.Y =
                        ( ( i == 0 )
                            ? ( layerHeight - v.Size.Height )
                            : verticalPos + layerHeight * (float)0.5 );

                    pos += v.Size.Width + Parameters.HorizontalGap;
                }
                verticalPos += layerHeight + Parameters.VerticalGap;
            }

            //assign the horizontal positions
            HorizontalPositionAssignment(cancellationToken);
        }

        protected void CopyPositionsSilent()
        {
            CopyPositionsSilent( true );
        }

        protected void CopyPositionsSilent( bool shouldTranslate )
        {
            //calculate the topLeft position
            var translation = new Vector( float.PositiveInfinity, float.PositiveInfinity );
            if ( shouldTranslate )
            {
                foreach ( var v in _graph.Vertices )
                {
                    if ( double.IsNaN( v.RealPosition.X ) || double.IsNaN( v.RealPosition.Y ) )
                        continue;

                    translation.X = Math.Min( v.RealPosition.X, translation.X );
                    translation.Y = Math.Min( v.RealPosition.Y, translation.Y );
                }
                translation *= -1;
                translation.X += Parameters.VerticalGap / 2;
                translation.Y += Parameters.HorizontalGap / 2;

                //translate with the topLeft position
                foreach ( var v in _graph.Vertices )
                    v.RealPosition += translation;
            }
            else
            {
                translation = new Vector( 0, 0 );
            }

            //copy the positions of the vertices
            VertexPositions.Clear();
            foreach ( var v in _graph.Vertices )
            {
                if ( v.IsDummyVertex )
                    continue;

                Point pos = v.RealPosition;
                if ( !shouldTranslate )
                {
                    pos.X += v.Size.Width * 0.5 + translation.X;
                    pos.Y += v.Size.Height * 0.5 + translation.Y;
                }
                VertexPositions[v.Original] = pos;
            }

            //copy the edge routes
            EdgeRoutes.Clear();
            foreach ( var e in _graph.HiddenEdges )
            {
                if ( !e.IsLongEdge )
                    continue;

                EdgeRoutes[e.Original] =
                    e.IsReverted
                        ? e.DummyVertices.Reverse().Select( dv => dv.RealPosition ).ToArray()
                        : e.DummyVertices.Select( dv => dv.RealPosition ).ToArray();
            }
        }

        /// <summary>
        /// Copies the coordinates of the vertices to the VertexPositions dictionary.
        /// </summary>
        protected void CopyPositions(CancellationToken cancellationToken)
        {
            AssignPositions(cancellationToken);

            CopyPositionsSilent();
        }
        #endregion

        public override void Compute(CancellationToken cancellationToken)
        {
            if(_graph.VertexCount == 0) return;
            if (_graph.VertexCount == 1)
            {
                VertexPositions.Clear();
                VertexPositions[_graph.Vertices.First().Original] = new Point(0,0);
                return;
            }
            //
            //Phase 1 - Filters & Removals
            //
            FiltersAndRemovals();
            _statusInPercent = PERCENT_OF_PREPARATION;

            //
            //Phase 2 - Layer assignment
            //
            AssignLayers();

            //
            //Phase 3 - Crossing reduction
            //
            PrepareForSugiyama(cancellationToken);
            SugiyamaLayout(cancellationToken);
            _statusInPercent = PERCENT_OF_PREPARATION + PERCENT_OF_SUGIYAMA;

            //
            //Phase 4 - Horizontal position assignment
            //
            CopyPositions(cancellationToken);
            OnIterationEnded( "Position adjusting finished" );

            //Phase 5 - Incremental extension, add vertices connected with only general edges
            //IncrementalExtensionImproved();
            _statusInPercent = PERCENT_OF_PREPARATION + PERCENT_OF_SUGIYAMA + PERCENT_OF_INCREMENTAL_EXTENSION;
            _statusInPercent = 100;
        }

        protected void OnIterationEnded( string message )
        {
            //OnIterationEnded( _iteration, _statusInPercent, message, true );
        }
    }
}