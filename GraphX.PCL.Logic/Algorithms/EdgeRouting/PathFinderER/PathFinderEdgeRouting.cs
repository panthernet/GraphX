using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using GraphX.Measure;
using GraphX.PCL.Common.Interfaces;
using QuickGraph;

namespace GraphX.PCL.Logic.Algorithms.EdgeRouting
{
    public class PathFinderEdgeRouting<TVertex, TEdge, TGraph> : EdgeRoutingAlgorithmBase<TVertex, TEdge, TGraph>
        where TGraph : class, IMutableBidirectionalGraph<TVertex, TEdge>
        where TEdge : class, IGraphXEdge<TVertex>
        where TVertex : class, IGraphXVertex
    {

        public PathFinderEdgeRouting(TGraph graph, IDictionary<TVertex, Point> vertexPositions, IDictionary<TVertex, Rect> vertexSizes, IEdgeRoutingParameters parameters = null)
            : base(graph, vertexPositions, vertexSizes, parameters)
        {
            if (parameters is PathFinderEdgeRoutingParameters)
            {
                var prms = parameters as PathFinderEdgeRoutingParameters;
                _horizontalGs = prms.HorizontalGridSize;
                _verticalGs = prms.VerticalGridSize;
                _sideAreaOffset = prms.SideGridOffset;
                _useDiagonals = prms.UseDiagonals;
                _pathAlgo = prms.PathFinderAlgorithm;
                _punishChangeDirection = prms.PunishChangeDirection;
                _useHeavyDiagonals = prms.UseHeavyDiagonals;
                _pfHeuristic = prms.Heuristic;
                _useTieBreaker = prms.UseTieBreaker;
            }
        }
         
        public override void UpdateVertexData(TVertex vertex, Point position, Rect size)
        {
            VertexPositions[vertex] = position;
            VertexSizes[vertex] = size;
        }

        public override Point[] ComputeSingle(TEdge edge)
        {
            CalculateMatrix(CancellationToken.None);//maybe shouldnt do this cause can be used from algo storage and already inited
            SetupPathFinder();//
            ComputeER(edge, CancellationToken.None);
            return EdgeRoutes.ContainsKey(edge) ? EdgeRoutes[edge] : null;
        }

        public override void Compute(CancellationToken cancellationToken)
        {
            if(VertexPositions == null || VertexPositions.Count < 3) return;
            foreach (var item in VertexPositions.Values)
            {
                cancellationToken.ThrowIfCancellationRequested();

                _minPoint.X = Math.Min(item.X, _minPoint.X);
                _minPoint.Y = Math.Min(item.Y, _minPoint.Y);
                _maxPoint.X = Math.Max(item.X, _maxPoint.X);
                _maxPoint.Y = Math.Max(item.Y, _maxPoint.Y);
            }

            EdgeRoutes.Clear();

            CalculateMatrix(cancellationToken);
            SetupPathFinder();


            foreach (var item in Graph.Edges)
                ComputeER(item, cancellationToken);
        }

        private Point _minPoint = new Point(double.PositiveInfinity, double.PositiveInfinity);
        private Point _maxPoint = new Point(double.NegativeInfinity, double.NegativeInfinity);
        private double _horizontalGs = 100;
        private double _verticalGs = 100;
        private double _sideAreaOffset = 500;
        private bool _useDiagonals = true;
        private double _vertexSafeDistance = 30;
        private bool _punishChangeDirection;
        private bool _useHeavyDiagonals;
        private int _pfHeuristic = 2;
        private bool _useTieBreaker;
        private int _searchLimit = 50000;
        private PathFindAlgorithm _pathAlgo = PathFindAlgorithm.Manhattan;

        private MatrixItem[,] _resMatrix;
        private List<MatrixItem> _validPoints;
        private PathFinder _pathFinder;


        #region Setup pathfinder
        private void SetupPathFinder()
        {
            _pathFinder = new PathFinder(_resMatrix);
            _pathFinder.Diagonals = _useDiagonals;
            _pathFinder.HeuristicEstimate = _pfHeuristic;
            _pathFinder.HeavyDiagonals = _useHeavyDiagonals;
            _pathFinder.PunishChangeDirection = _punishChangeDirection;
            _pathFinder.TieBreaker = _useTieBreaker;
            _pathFinder.SearchLimit = _searchLimit;

            switch (_pathAlgo)
            {
                case PathFindAlgorithm.Manhattan:
                    _pathFinder.Formula = HeuristicFormula.Manhattan;
                    break;
                case PathFindAlgorithm.MaxDXDY:
                    _pathFinder.Formula = HeuristicFormula.MaxDXDY;
                    break;
                case PathFindAlgorithm.Euclidean:
                    _pathFinder.Formula = HeuristicFormula.Euclidean;
                    break;
                case PathFindAlgorithm.EuclideanNoSQR:
                    _pathFinder.Formula = HeuristicFormula.EuclideanNoSQR;
                    break;
                case PathFindAlgorithm.DiagonalShortCut:
                    _pathFinder.Formula = HeuristicFormula.DiagonalShortCut;
                    break;
                case PathFindAlgorithm.Custom1:
                    _pathFinder.Formula = HeuristicFormula.Custom1;
                    break;
                default: throw new Exception("setupPathFinder() -> Unknown formula!");
            }
        }
        #endregion

        #region Calculate matrix
        private void CalculateMatrix(CancellationToken cancellationToken)
        {
            var tl = new Point(_minPoint.X - _sideAreaOffset, _minPoint.Y - _sideAreaOffset);
            var br = new Point(_maxPoint.X + _sideAreaOffset, _maxPoint.Y + _sideAreaOffset);

            var hCount = (int)((br.X - tl.X) / _horizontalGs) + 1;
            var vCount = (int)((br.Y - tl.Y) / _verticalGs) + 1;

            _resMatrix = new MatrixItem[hCount, vCount];
            _validPoints = new List<MatrixItem>();

            var lastPt = new Point(0, 0);

            //get the intersection matrix
            for (int i = 0; i < hCount; i++)
                for (int j = 0; j < vCount; j++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    lastPt = new Point(tl.X + _horizontalGs * i, tl.Y + _verticalGs * j);
                    _resMatrix[i, j] = new MatrixItem(lastPt, IsOverlapped(lastPt), i, j);
                    if (!_resMatrix[i, j].IsIntersected) _validPoints.Add(_resMatrix[i, j]);
                }
            ////////////debug
#if DEBUG
            for (int i = 0; i < vCount; i++)
            {
                var str = "";
                for (int j = 0; j < hCount; j++)
                {
                    str += _resMatrix[j, i].IsIntersected ? "0 " : "1 ";
                }
                Debug.WriteLine(str);
            }
#endif
            /////////
        }
        #endregion

        private void ComputeER(TEdge item, CancellationToken cancellationToken)
        {
            var startPt = GetClosestPoint(_validPoints, VertexPositions[item.Source]);
            var endPt = GetClosestPoint(_validPoints, VertexPositions[item.Target]);
            var lst = _pathFinder.FindPath(startPt, endPt);

            if (lst == null) return;
            var ptlst = new List<Point>();
            foreach (var pt in lst)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var mi = _resMatrix[pt.X, pt.Y];
                ptlst.Add(mi.Point);
            }
            if (EdgeRoutes.ContainsKey(item))
                EdgeRoutes[item] = ptlst.ToArray();
            else EdgeRoutes.Add(new KeyValuePair<TEdge,Point[]>(item, ptlst.ToArray()));
        }

        private bool IsOverlapped(Point pt)
        {
            var trect = new Rect(pt.X, pt.Y, 2, 2);
            return VertexSizes.Select(item => new Rect(item.Value.X - _vertexSafeDistance, item.Value.Y - _vertexSafeDistance, item.Value.Width + _vertexSafeDistance, item.Value.Height + _vertexSafeDistance)).Any(rect => rect.IntersectsWith(trect));
        }

        private Point GetClosestPoint(IEnumerable<MatrixItem> points, Point pt)
        {
            var lst = points.Where(mi => mi.Point != pt).
                           OrderBy(mi => GetFakeDistance(pt, mi.Point)).
                           Take(1);
            if (!lst.Any()) throw new Exception("GetClosestPoint() -> Can't find one!");
            return  new Point(lst.First().PlaceX, lst.First().PlaceY);
        }

        private double GetFakeDistance(Point source, Point target)
        {
            double dx = target.X - source.X; double dy = target.Y - source.Y;
            return dx * dx + dy * dy;
        }
    }
}
