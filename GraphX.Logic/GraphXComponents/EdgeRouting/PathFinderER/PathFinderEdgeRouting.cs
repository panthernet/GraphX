using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using GraphX.GraphSharp.Algorithms.EdgeRouting;
using GustavoAlgorithms;
using QuickGraph;

namespace GraphX.GraphSharpComponents.EdgeRouting
{
    public class PathFinderEdgeRouting<TVertex, TEdge, TGraph> : EdgeRoutingAlgorithmBase<TVertex, TEdge, TGraph>
        where TGraph : class, IBidirectionalGraph<TVertex, TEdge>
        where TEdge : IEdge<TVertex>
        where TVertex : class, IIdentifiableGraphDataObject
    {

        public PathFinderEdgeRouting(TGraph graph, IDictionary<TVertex, Point> vertexPositions, IDictionary<TVertex, Rect> vertexSizes, IEdgeRoutingParameters parameters = null)
            : base(graph, vertexPositions, vertexSizes, parameters)
        {
            if (parameters != null && parameters is PathFinderEdgeRoutingParameters)
            {
                var prms = parameters as PathFinderEdgeRoutingParameters;
                _horizontalGS = prms.HorizontalGridSize;
                _verticalGS = prms.VerticalGridSize;
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
            calculateMatrix();//maybe shouldnt do this cause can be used from algo storage and already inited
            setupPathFinder();//
            ComputeER(edge);
            return EdgeRoutes.ContainsKey(edge) ? EdgeRoutes[edge] : null;
        }

        public override void Compute()
        {
            if(VertexPositions == null || VertexPositions.Count < 3) return;
            foreach (var item in VertexPositions.Values)
            {
                _minPoint.X = Math.Min(item.X, _minPoint.X);
                _minPoint.Y = Math.Min(item.Y, _minPoint.Y);
                _maxPoint.X = Math.Max(item.X, _maxPoint.X);
                _maxPoint.Y = Math.Max(item.Y, _maxPoint.Y);
            }

            EdgeRoutes.Clear();

            calculateMatrix();
            setupPathFinder();


            foreach (var item in _graph.Edges)
                ComputeER(item);
        }

        private Point _minPoint = new Point(double.PositiveInfinity, double.PositiveInfinity);
        private Point _maxPoint = new Point(double.NegativeInfinity, double.NegativeInfinity);
        private double _horizontalGS = 100;
        private double _verticalGS = 100;
        private double _sideAreaOffset = 500;
        private bool _useDiagonals = true;
        private double _vertexSafeDistance = 30;
        private bool _punishChangeDirection;
        private bool _useHeavyDiagonals;
        private int _pfHeuristic = 2;
        private bool _useTieBreaker;
        private int _searchLimit = 50000;
        private PathFindAlgorithm _pathAlgo = PathFindAlgorithm.Manhattan;

        private MatrixItem[,] resMatrix;
        private List<MatrixItem> validPoints;
        private PathFinder pathFinder;


        #region Setup pathfinder
        private void setupPathFinder()
        {
            pathFinder = new PathFinder(resMatrix);
            pathFinder.Diagonals = _useDiagonals;
            pathFinder.HeuristicEstimate = _pfHeuristic;
            pathFinder.HeavyDiagonals = _useHeavyDiagonals;
            pathFinder.PunishChangeDirection = _punishChangeDirection;
            pathFinder.TieBreaker = _useTieBreaker;
            pathFinder.SearchLimit = _searchLimit;

            switch (_pathAlgo)
            {
                case PathFindAlgorithm.Manhattan:
                    pathFinder.Formula = HeuristicFormula.Manhattan;
                    break;
                case PathFindAlgorithm.MaxDXDY:
                    pathFinder.Formula = HeuristicFormula.MaxDXDY;
                    break;
                case PathFindAlgorithm.Euclidean:
                    pathFinder.Formula = HeuristicFormula.Euclidean;
                    break;
                case PathFindAlgorithm.EuclideanNoSQR:
                    pathFinder.Formula = HeuristicFormula.EuclideanNoSQR;
                    break;
                case PathFindAlgorithm.DiagonalShortCut:
                    pathFinder.Formula = HeuristicFormula.DiagonalShortCut;
                    break;
                case PathFindAlgorithm.Custom1:
                    pathFinder.Formula = HeuristicFormula.Custom1;
                    break;
                default: throw new Exception("setupPathFinder() -> Unknown formula!");
            }
        }
        #endregion

        #region Calculate matrix
        private void calculateMatrix()
        {
            var tl = new Point(_minPoint.X - _sideAreaOffset, _minPoint.Y - _sideAreaOffset);
            var br = new Point(_maxPoint.X + _sideAreaOffset, _maxPoint.Y + _sideAreaOffset);

            var hCount = (int)((br.X - tl.X) / _horizontalGS) + 1;
            var vCount = (int)((br.Y - tl.Y) / _verticalGS) + 1;

            resMatrix = new MatrixItem[hCount, vCount];
            validPoints = new List<MatrixItem>();

            var lastPt = new Point(0, 0);

            //get the intersection matrix
            for (int i = 0; i < hCount; i++)
                for (int j = 0; j < vCount; j++)
                {
                    lastPt = new Point(tl.X + _horizontalGS * i, tl.Y + _verticalGS * j);
                    resMatrix[i, j] = new MatrixItem(lastPt, IsOverlapped(lastPt), i, j);
                    if (!resMatrix[i, j].IsIntersected) validPoints.Add(resMatrix[i, j]);
                }
            ////////////debug
            for (int i = 0; i < vCount; i++)
            {
                var str = "";
                for (int j = 0; j < hCount; j++)
                {
                    str += resMatrix[j, i].IsIntersected ? "0 " : "1 ";
                }
                Debug.WriteLine(str);
            }
            /////////
        }
        #endregion

        private void ComputeER(TEdge item)
        {
            var startPt = getClosestPoint(validPoints, VertexPositions[item.Source]);
            var endPt = getClosestPoint(validPoints, VertexPositions[item.Target]);
            var lst = pathFinder.FindPath(startPt, endPt);

            if (lst == null) return;
            var ptlst = new List<Point>();
            foreach (var pt in lst)
            {
                var mi = resMatrix[pt.X, pt.Y];
                ptlst.Add(mi.Point);
            }
            if (EdgeRoutes.ContainsKey(item))
                EdgeRoutes[item] = ptlst.ToArray();
            else EdgeRoutes.Add(new KeyValuePair<TEdge,Point[]>(item, ptlst.ToArray()));
        }

        private bool IsOverlapped(Point pt)
        {
            var trect = new Rect(pt.X, pt.Y, 2, 2);
            foreach (var item in VertexSizes)
            {
                var rect = new Rect(item.Value.X - _vertexSafeDistance, item.Value.Y - _vertexSafeDistance, item.Value.Width + _vertexSafeDistance, item.Value.Height + _vertexSafeDistance);
                if (rect.IntersectsWith(trect))
                    return true;
            }
            return false;
        }

        private System.Drawing.Point getClosestPoint(List<MatrixItem> points, Point pt)
        {
            var lst = points.Where(mi => mi.Point != pt).
                           OrderBy(mi => getFakeDistance(pt, mi.Point)).
                           Take(1);
            if (lst.Count() == 0) throw new Exception("GetClosestPoint() -> Can't find one!");
            return  new System.Drawing.Point(lst.First().PlaceX, lst.First().PlaceY);
        }

        private double getFakeDistance(Point source, Point target)
        {
            double dx = target.X - source.X; double dy = target.Y - source.Y;
            return dx * dx + dy * dy;
        }
    }
}
