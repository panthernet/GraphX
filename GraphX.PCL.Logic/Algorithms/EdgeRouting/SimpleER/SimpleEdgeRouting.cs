using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GraphX.Measure;
using GraphX.PCL.Common;
using GraphX.PCL.Common.Exceptions;
using GraphX.PCL.Common.Interfaces;
using QuickGraph;

namespace GraphX.PCL.Logic.Algorithms.EdgeRouting
{
    public class SimpleEdgeRouting<TVertex, TEdge, TGraph> : EdgeRoutingAlgorithmBase<TVertex, TEdge, TGraph>
        where TGraph : class, IMutableBidirectionalGraph<TVertex, TEdge>
        where TEdge : class, IGraphXEdge<TVertex>
        where TVertex : class, IGraphXVertex
    {
        public SimpleEdgeRouting(TGraph graph, IDictionary<TVertex, Point> vertexPositions, IDictionary<TVertex, Rect> vertexSizes, IEdgeRoutingParameters parameters = null)
            : base(graph, vertexPositions, vertexSizes, parameters)
        {
            if (parameters is SimpleERParameters)
            {
                drawback_distance = (parameters as SimpleERParameters).BackStep;
                side_distance = (parameters as SimpleERParameters).SideStep;
            }
        }

        public override void UpdateVertexData(TVertex vertex, Point position, Rect size)
        {
            VertexPositions.AddOrUpdate(vertex, position);
            VertexSizes.AddOrUpdate(vertex, size);
        }

        public override Point[] ComputeSingle(TEdge edge)
        {
            EdgeRoutingTest(edge, CancellationToken.None);
            return EdgeRoutes.ContainsKey(edge) ? EdgeRoutes[edge] : null;
        }

        public override void Compute(CancellationToken cancellationToken)
        {
            EdgeRoutes.Clear();
            foreach (var item in Graph.Edges)
                EdgeRoutingTest(item, cancellationToken);
        }

        double drawback_distance = 10;
        double side_distance = 5;
        double vertex_margin_distance = 35;

        private IDictionary<TVertex, KeyValuePair<TVertex, Rect>> getSizesCollection(TEdge ctrl, Point end_point)
        {
            var list = VertexSizes.Where(a => a.Key.ID != ctrl.Source.ID && a.Key.ID != ctrl.Target.ID).OrderByDescending(a => GetDistance(VertexPositions[a.Key], end_point)).ToDictionary(a => a.Key); // new Dictionary<TVertex, Rect>();
            foreach ( var item in list.Values)
                item.Value.Inflate(vertex_margin_distance * 2, vertex_margin_distance * 2);
            return list;
        }

        private void EdgeRoutingTest(TEdge ctrl, CancellationToken cancellationToken)
        {
            //bad edge data check
            if (ctrl.Source.ID == -1 || ctrl.Target.ID == -1)
                throw new GX_InvalidDataException("SimpleEdgeRouting() -> You must assign unique ID for each vertex to use SimpleER algo!");
            if (ctrl.Source.ID == ctrl.Target.ID || !VertexPositions.ContainsKey(ctrl.Target)) return;

            var startPoint = VertexPositions[ctrl.Source];// new Point(GraphAreaBase.GetX(ctrl.Source), GraphAreaBase.GetY(ctrl.Source));
            var endPoint = VertexPositions[ctrl.Target];// new Point(GraphAreaBase.GetX(ctrl.Target), GraphAreaBase.GetY(ctrl.Target));

            if (startPoint == endPoint) return;

            var originalSizes = getSizesCollection(ctrl, endPoint);
            var checklist = new Dictionary<TVertex, KeyValuePair<TVertex, Rect>>(originalSizes);
            var leftSizes = new Dictionary<TVertex, KeyValuePair<TVertex, Rect>>(originalSizes);


            var tempList = new List<Point>();
            tempList.Add(startPoint);

            bool haveIntersections = true;

            //while we have some intersections - proceed
            while (haveIntersections)
            {
                var curDrawback = drawback_distance;
                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var item = checklist.Keys.FirstOrDefault();
                    //set last route point as current start point
                    startPoint = tempList.Last();
                    if (item == null)
                    {
                        //checked all vertices and no intersection was found - quit
                        haveIntersections = false;
                        break;
                    }
                    else
                    {
                        var r = originalSizes[item].Value;
                        Point checkpoint;
                        //check for intersection point. if none found - remove vertex from checklist
                        if (GetIntersectionPoint(r, startPoint, endPoint, out checkpoint) == -1)
                        {
                            checklist.Remove(item); continue;
                        }
                        var mainVector = new Vector(endPoint.X - startPoint.X, endPoint.Y - startPoint.Y);
                        double X = 0; double Y = 0;
                        //calculate drawback X coordinate
                        if (startPoint.X == checkpoint.X || Math.Abs(startPoint.X - checkpoint.X) < curDrawback) X = startPoint.X;
                        else if (startPoint.X < checkpoint.X) X = checkpoint.X - curDrawback;
                        else X = checkpoint.X + curDrawback;
                        //calculate drawback Y coordinate
                        if (startPoint.Y == checkpoint.Y || Math.Abs(startPoint.Y - checkpoint.Y) < curDrawback) Y = startPoint.Y;
                        else if (startPoint.Y < checkpoint.Y) Y = checkpoint.Y - curDrawback;
                        else Y = checkpoint.Y + curDrawback;
                        //set drawback checkpoint
                        checkpoint = new Point(X, Y);
                        bool isStartPoint = checkpoint == startPoint;

                        bool routeFound = false;
                        bool viceversa = false;
                        int counter = 1;
                        var joint = new Point();
                        bool? blocked_direction = null;
                        while (!routeFound)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            //choose opposite vector side each cycle
                            var signedDistance = viceversa ? side_distance : -side_distance;
                            //get new point coordinate
                            joint = new Point(
                                 checkpoint.X + signedDistance * counter * (mainVector.Y / mainVector.Length),
                                 checkpoint.Y - signedDistance * counter * (mainVector.X / mainVector.Length));

                            //now check if new point is in some other vertex
                            var iresult = false;
                            var forcedBreak = false;
                            if (originalSizes.Any(sz => sz.Value.Value.Contains(joint))) 
                            {
                                iresult = true;
                                //block this side direction
                                if (blocked_direction == null)
                                    blocked_direction = viceversa;
                                else
                                {
                                    //both sides blocked - need to drawback
                                    forcedBreak = true;
                                }
                            }
                            if (forcedBreak) break;

                            //get vector intersection if its ok
                            if(!iresult) iresult = IsIntersected(r, joint, endPoint);
                            
                            //if no vector intersection - we've found it!
                            if (!iresult)
                            {
                                routeFound = true;
                                blocked_direction = null;
                            }
                            else
                            {
                                //still have an intersection with current vertex
                                haveIntersections = true;
                                //skip point search if too many attempts was made (bad logic hack)
                                if (counter > 300) break;
                                counter++;
                                //switch vector search side
                                if (blocked_direction == null || (blocked_direction == viceversa))
                                    viceversa = !viceversa;
                            }
                        }

                        //if blocked and this is not start point (nowhere to drawback) - then increase drawback distance
                        if (blocked_direction != null && !isStartPoint)
                        {
                            //search has been blocked - need to drawback
                            curDrawback += drawback_distance;
                        }
                        else
                        {
                            //add new route point if we found it
                            // if(routeFound) 
                            tempList.Add(joint);
                            leftSizes.Remove(item);
                        }
                    }
                    //remove currently evaded obstacle vertex from the checklist
                    checklist.Remove(item);
                }
                //assign possible left vertices as a new checklist if any intersections was found
                if (haveIntersections)
                    checklist = new Dictionary<TVertex, KeyValuePair<TVertex, Rect>>(leftSizes);
            }
            //finally, add an end route point

            tempList.Add(endPoint);


            if (EdgeRoutes.ContainsKey(ctrl))
                EdgeRoutes[ctrl] = tempList.Count > 2 ? tempList.ToArray() : null;
            else EdgeRoutes.Add(ctrl, tempList.Count > 2 ? tempList.ToArray() : null);

        }

        #region Math helper implementation
        public static Point GetCloserPoint(Point start, Point a, Point b)
        {
            var r1 = GetDistance(start, a);
            var r2 = GetDistance(start, b);
            return r1 < r2 ? a : b;
        }

        public static double GetDistance(Point a, Point b)
        {
            return ((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));
        }

        public static sides GetIntersectionData(Rect r, Point p)
        {
            return new sides() { Left = p.X < r.Left, Right = p.X > r.Right, Bottom = p.Y > r.Bottom, Top = p.Y < r.Top };
        }

        public static bool IsIntersected(Rect r, Point a, Point b)
        {
            // var start = new Point(a.X, a.Y);
            /* код конечных точек отрезка */
            var codeA = GetIntersectionData(r, a);
            var codeB = GetIntersectionData(r, b);

            if (codeA.IsInside() && codeB.IsInside())
                return true;

            /* пока одна из точек отрезка вне прямоугольника */
            while (!codeA.IsInside() || !codeB.IsInside())
            {
                /* если обе точки с одной стороны прямоугольника, то отрезок не пересекает прямоугольник */
                if (codeA.SameSide(codeB))
                    return false;

                /* выбираем точку c с ненулевым кодом */
                sides code;
                Point c; /* одна из точек */
                if (!codeA.IsInside())
                {
                    code = codeA;
                    c = a;
                }
                else
                {
                    code = codeB;
                    c = b;
                }

                /* если c левее r, то передвигаем c на прямую x = r->x_min
                   если c правее r, то передвигаем c на прямую x = r->x_max */
                if (code.Left)
                {
                    c.Y += (a.Y - b.Y) * (r.Left - c.X) / (a.X - b.X);
                    c.X = r.Left;
                }
                else if (code.Right)
                {
                    c.Y += (a.Y - b.Y) * (r.Right - c.X) / (a.X - b.X);
                    c.X = r.Right;
                }/* если c ниже r, то передвигаем c на прямую y = r->y_min
                    если c выше r, то передвигаем c на прямую y = r->y_max */
                else if (code.Bottom)
                {
                    c.X += (a.X - b.X) * (r.Bottom - c.Y) / (a.Y - b.Y);
                    c.Y = r.Bottom;
                }
                else if (code.Top)
                {
                    c.X += (a.X - b.X) * (r.Top - c.Y) / (a.Y - b.Y);
                    c.Y = r.Top;
                }

                /* обновляем код */
                if (code == codeA)
                {
                    a = c;
                    codeA = GetIntersectionData(r, a);
                }
                else
                {
                    b = c;
                    codeB = GetIntersectionData(r, b);
                }
            }
            return true;
        }

        public static int GetIntersectionPoint(Rect r, Point a, Point b, out Point pt)
        {
            sides code;
            Point c; /* одна из точек */
            var start = new Point(a.X, a.Y);
            /* код конечных точек отрезка */
            var code_a = GetIntersectionData(r, a);
            var code_b = GetIntersectionData(r, b);

            /* пока одна из точек отрезка вне прямоугольника */
            while (!code_a.IsInside() || !code_b.IsInside())
            {
                /* если обе точки с одной стороны прямоугольника, то отрезок не пересекает прямоугольник */
                if (code_a.SameSide(code_b))
                {
                    pt = new Point();
                    return -1;
                }

                /* выбираем точку c с ненулевым кодом */
                if (!code_a.IsInside())
                {
                    code = code_a;
                    c = a;
                }
                else
                {
                    code = code_b;
                    c = b;
                }

                /* если c левее r, то передвигаем c на прямую x = r->x_min
                   если c правее r, то передвигаем c на прямую x = r->x_max */
                if (code.Left)
                {
                    c.Y += (a.Y - b.Y) * (r.Left - c.X) / (a.X - b.X);
                    c.X = r.Left;
                }
                else if (code.Right)
                {
                    c.Y += (a.Y - b.Y) * (r.Right - c.X) / (a.X - b.X);
                    c.X = r.Right;
                }/* если c ниже r, то передвигаем c на прямую y = r->y_min
                    если c выше r, то передвигаем c на прямую y = r->y_max */
                else if (code.Bottom)
                {
                    c.X += (a.X - b.X) * (r.Bottom - c.Y) / (a.Y - b.Y);
                    c.Y = r.Bottom;
                }
                else if (code.Top)
                {
                    c.X += (a.X - b.X) * (r.Top - c.Y) / (a.Y - b.Y);
                    c.Y = r.Top;
                }

                /* обновляем код */
                if (code == code_a)
                {
                    a = c;
                    code_a = GetIntersectionData(r, a);
                }
                else
                {
                    b = c;
                    code_b = GetIntersectionData(r, b);
                }
            }
            pt = GetCloserPoint(start, a, b);
            return 0;
        }

        public sealed class sides
        {
            public bool Left;
            public bool Right;
            public bool Top;
            public bool Bottom;

            public bool IsInside()
            {
                return Left == false && Right == false && Top == false && Bottom == false;
            }

            public bool SameSide(sides o)
            {
                return (Left == true && o.Left == true) || (Right == true && o.Right == true) || (Top == true && o.Top == true)
                    || (Bottom == true && o.Bottom == true);
            }
        }
        #endregion

    }

}
