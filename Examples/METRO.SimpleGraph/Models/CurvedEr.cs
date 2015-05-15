using System;
using System.Collections.Generic;
using System.Threading;
using GraphX;
using GraphX.Measure;
using GraphX.PCL.Common.Interfaces;
using GraphX.PCL.Logic.Algorithms.EdgeRouting;
using METRO.SimpleGraph;

namespace InteractiveGraph.Models
{
    public class CurvedEr: EdgeRoutingAlgorithmBase<DataVertex, DataEdge, GraphExample>
    {
        private readonly int _curveOffset;

        public CurvedEr(GraphExample graph, IDictionary<DataVertex, Point> vertexPositions = null, IDictionary<DataVertex, Rect> vertexSizes = null, IEdgeRoutingParameters parameters = null) : 
            base(graph, vertexPositions, vertexSizes, parameters)
        {
            var prms = parameters as CurvedErParameters;
            _curveOffset = prms != null ? prms.VerticalCurveOffset : 20;
        }

        public override void Compute(CancellationToken cancellationToken)
        {
            EdgeRoutes.Clear();
            foreach (var edge in Graph.Edges)
            {
                EdgeRoutes.Add(edge, ComputeSingle(edge));
            }
        }

        public override Point[] ComputeSingle(DataEdge edge)
        {
            var halfSize = new Vector(VertexSizes[edge.Source].Size.Width / 2, VertexSizes[edge.Source].Size.Height / 2);
            var pos1 = VertexPositions[edge.Source] + halfSize;
            var pos2 = VertexPositions[edge.Target] + halfSize;

            //make sure we're always curve into one direction
            /*if (pos2.X > pos1.X)
            {
                var tmp = pos1;
                pos1 = pos2;
                pos2 = tmp;
            }*/
            //get middle point ob the line
            var midPoint = new Point(pos1.X + .5 * (pos2.X - pos1.X), pos1.Y + .5 * (pos2.Y - pos1.Y));// + halfSize;
            //var lowPoint = new Point(pos1.X + .25 * (pos2.X - pos1.X), pos1.Y + .25 * (pos2.Y - pos1.Y));// + halfSize;
            //var highPoint = new Point(pos1.X + .75 * (pos2.X - pos1.X), pos1.Y + .75 * (pos2.Y - pos1.Y));// + halfSize;

            //apply perpendicular offset to the middle point
            var dx = pos1.X-pos2.X;
            var dy = pos1.Y-pos2.Y;
            var dist = Math.Sqrt(dx*dx + dy*dy);
            dx /= dist;
            dy /= dist;
            var width = (_curveOffset) * dy;
            var height = (_curveOffset) * dx;
            var resPoint = new Point(midPoint.X + width, midPoint.Y - height);
            //var resPoint2 = new Point(lowPoint.X + width, lowPoint.Y - height * .5);
            //var resPoint3 = new Point(highPoint.X + width * .5, highPoint.Y - height * .5);

            return new[] { pos1, resPoint, pos2};
        }
    }
}
