using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;

/* Code here is partially used from NodeXL (https://nodexl.codeplex.com/)
 * 
 * 
 * 
 * */

namespace GraphX
{
    public static class GeometryHelper
    {

        /// <summary>
        /// Get Intersection point on a rectangular surface
        /// </summary>
        /// <param name="a1">a1 is line1 start</param>
        /// <param name="a2">a2 is line1 end</param>
        /// <param name="b1">b1 is line2 start</param>
        /// <param name="b2">b2 is line2 end</param>
        /// <returns></returns>
        public static Vector? Intersects(Vector a1, Vector a2, Vector b1, Vector b2)
        {
            Vector b = a2 - a1;
            Vector d = b2 - b1;
            var bDotDPerp = b.X * d.Y - b.Y * d.X;

            // if b dot d == 0, it means the lines are parallel so have infinite intersection points
            if (bDotDPerp == 0)
                return null;

            Vector c = b1 - a1;
            var t = (c.X * d.Y - c.Y * d.X) / bDotDPerp;
            if (t < 0 || t > 1)
            {
                return null;
            }

            var u = (c.X * b.Y - c.Y * b.X) / bDotDPerp;
            if (u < 0 || u > 1)
            {
                return null;
            }

            return a1 + t * b;
        }

        /// <summary>
        /// Generate PathGeometry object with curved Path using supplied route points
        /// </summary>
        /// <param name="points">Route points</param>
        /// <param name="tension"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public static PolyLineSegment GetCurveThroughPoints(Point[] points, Double tension, Double tolerance)
        {
            Debug.Assert(points != null);
            Debug.Assert(points.Length >= 2);
            Debug.Assert(tolerance > 0);

            PolyLineSegment oPolyLineSegment = new PolyLineSegment();

            if (points.Length == 2)
            {
                AddPointsToPolyLineSegment(oPolyLineSegment, points[0], points[0],
                    points[1], points[1], tension, tolerance);
            }
            else
            {
                Int32 iPoints = points.Length;

                for (Int32 i = 0; i < iPoints; i++)
                {
                    if (i == 0)
                    {
                        AddPointsToPolyLineSegment(oPolyLineSegment, points[0],
                            points[0], points[1], points[2], tension, tolerance);
                    }

                    else if (i == iPoints - 2)
                    {
                        AddPointsToPolyLineSegment(oPolyLineSegment, points[i - 1],
                            points[i], points[i + 1], points[i + 1], tension,
                            tolerance);
                    }
                    else if (i != iPoints - 1)
                    {
                        AddPointsToPolyLineSegment(oPolyLineSegment, points[i - 1],
                            points[i], points[i + 1], points[i + 2], tension,
                            tolerance);
                    }
                }
                oPolyLineSegment.Points.Insert(0, points[0]);
            }

            return oPolyLineSegment;
        }

        private static void AddPointsToPolyLineSegment(PolyLineSegment oPolyLineSegment, Point oPoint0, Point oPoint1, Point oPoint2, Point oPoint3, Double dTension, Double dTolerance)
        {
            Debug.Assert(oPolyLineSegment != null);
            Debug.Assert(dTolerance > 0);

            Int32 iPoints = (Int32)((Math.Abs(oPoint1.X - oPoint2.X) +
                Math.Abs(oPoint1.Y - oPoint2.Y)) / dTolerance);

            PointCollection oPolyLineSegmentPoints = oPolyLineSegment.Points;

            if (iPoints <= 2)
            {
                oPolyLineSegmentPoints.Add(oPoint2);
            }
            else
            {
                Double dSX1 = dTension * (oPoint2.X - oPoint0.X);
                Double dSY1 = dTension * (oPoint2.Y - oPoint0.Y);
                Double dSX2 = dTension * (oPoint3.X - oPoint1.X);
                Double dSY2 = dTension * (oPoint3.Y - oPoint1.Y);

                Double dAX = dSX1 + dSX2 + 2 * oPoint1.X - 2 * oPoint2.X;
                Double dAY = dSY1 + dSY2 + 2 * oPoint1.Y - 2 * oPoint2.Y;
                Double dBX = -2 * dSX1 - dSX2 - 3 * oPoint1.X + 3 * oPoint2.X;
                Double dBY = -2 * dSY1 - dSY2 - 3 * oPoint1.Y + 3 * oPoint2.Y;

                Double dCX = dSX1;
                Double dCY = dSY1;
                Double dDX = oPoint1.X;
                Double dDY = oPoint1.Y;

                // Note that this starts at 1, not 0.

                for (int i = 1; i < iPoints; i++)
                {
                    Double t = (Double)i / (iPoints - 1);

                    Point oPoint = new Point(
                        dAX * t * t * t + dBX * t * t + dCX * t + dDX,
                        dAY * t * t * t + dBY * t * t + dCY * t + dDY
                        );

                    oPolyLineSegmentPoints.Add(oPoint);
                }

            }
        }

        public static PathFigure GetPathFigureFromPathSegments(Point oStartPoint, Boolean bPathFigureIsFilled, bool freezeAll, params PathSegment[] aoPathSegments)
        {
            Debug.Assert(aoPathSegments != null);

            PathFigure oPathFigure = new PathFigure() { StartPoint = oStartPoint, IsFilled = bPathFigureIsFilled };
            PathSegmentCollection oSegments = oPathFigure.Segments;

            foreach (PathSegment oPathSegment in aoPathSegments)
            {
                if (freezeAll) TryFreeze(oPathSegment);
                oSegments.Add(oPathSegment);
            }
            if (freezeAll) TryFreeze(oPathFigure);
            return oPathFigure;
        }

        public static PathGeometry GetPathGeometryFromPathSegments(Point oStartPoint, Boolean bPathFigureIsFilled, params PathSegment[] aoPathSegments)
        {
            Debug.Assert(aoPathSegments != null);

            PathFigure oPathFigure = new PathFigure() { StartPoint = oStartPoint, IsFilled = bPathFigureIsFilled };
            PathSegmentCollection oSegments = oPathFigure.Segments;

            foreach (PathSegment oPathSegment in aoPathSegments)
            {
                TryFreeze(oPathSegment);
                oSegments.Add(oPathSegment);
            }
            TryFreeze(oPathFigure);
            PathGeometry oPathGeometry = new PathGeometry();
            oPathGeometry.Figures.Add(oPathFigure);
           // FreezeIfFreezable(oPathGeometry);

            return (oPathGeometry);
        }

        /// <summary>
        /// Try to freeze object 
        /// </summary>
        /// <param name="freezable">Freezable object</param>
        /// <returns></returns>
        public static Boolean TryFreeze(Freezable freezable)
        {
            Debug.Assert(freezable != null);

            if (freezable.CanFreeze)
            {
                freezable.Freeze();
                return true;
            }else return false;
        }

        public static Point GetEdgeEndpoint(Point source, Rect sourceSize, Point target, VertexShape shape)
        {
            switch (shape)
            {
                case VertexShape.Circle:
                    return GetEdgeEndpointOnCircle(source, Math.Max(sourceSize.Height, sourceSize.Width) * .5, target);
                case VertexShape.Diamond:
                    return GetEdgeEndpointOnDiamond(source, sourceSize.Width * .5, target);
                case VertexShape.Triangle:
                    return GetEdgeEndpointOnTriangle(source, sourceSize.Width * .5, target);
                default:
                    return GetEdgeEndpointOnRectangle(source, sourceSize, target);
            }
        }

        public static Point GetEdgeEndpointOnCircle(Point oVertexALocation, Double dVertexARadius, Point oVertexBLocation)
        {
            Debug.Assert(dVertexARadius >= 0);

            Double dEdgeAngle = MathHelper.GetAngleBetweenPointsRadians(oVertexALocation, oVertexBLocation);

            return new Point(
                oVertexALocation.X + (dVertexARadius * Math.Cos(dEdgeAngle)),
                oVertexALocation.Y - (dVertexARadius * Math.Sin(dEdgeAngle))
                );
        }

        public static Point GetEdgeEndpointOnTriangle(Point oVertexLocation, Double m_dHalfWidth, Point otherEndpoint)
        {
            // Instead of doing geometry calculations similar to what is done in 
            // VertexDrawingHistory.GetEdgePointOnRectangle(), make use of that
            // method by making the triangle look like a rectangle.  First, figure
            // out how to rotate the triangle about the vertex location so that the
            // side containing the endpoint is vertical and to the right of the
            // vertex location.

            Double dEdgeAngle = MathHelper.GetAngleBetweenPointsRadians(
                oVertexLocation, otherEndpoint);

            Double dEdgeAngleDegrees = MathHelper.RadiansToDegrees(dEdgeAngle);

            Double dAngleToRotateDegrees;

            if (dEdgeAngleDegrees >= -30.0 && dEdgeAngleDegrees < 90.0)
            {
                dAngleToRotateDegrees = 30.0;
            }
            else if (dEdgeAngleDegrees >= -150.0 && dEdgeAngleDegrees < -30.0)
            {
                dAngleToRotateDegrees = 270.0;
            }
            else
            {
                dAngleToRotateDegrees = 150.0;
            }

            // Now create a rotated rectangle that is centered on the vertex
            // location and that has the vertical, endpoint-containing triangle
            // side as the rectangle's right edge.

            Double dWidth = 2.0 * m_dHalfWidth;

            Rect oRotatedRectangle = new Rect(
                oVertexLocation.X,
                oVertexLocation.Y - m_dHalfWidth,
                dWidth * MathHelper.Tangent30Degrees,
                dWidth
                );

            Matrix oMatrix = GetRotatedMatrix(oVertexLocation,
                dAngleToRotateDegrees);

            // Rotate the other vertex location.
            Point oRotatedOtherVertexLocation = oMatrix.Transform(otherEndpoint);

            // GetEdgeEndpointOnRectangle will compute an endpoint on the
            // rectangle's right edge.
            var oRotatedEdgeEndpoint = GetEdgeEndpointOnRectangle(oVertexLocation, oRotatedRectangle,
                oRotatedOtherVertexLocation);

            // Now rotate the edge endpoint in the other direction.
            oMatrix = GetRotatedMatrix(oVertexLocation,
                -dAngleToRotateDegrees);

            return oMatrix.Transform(oRotatedEdgeEndpoint);
        }

        public static Point GetEdgeEndpointOnDiamond(Point oVertexLocation, Double m_dHalfWidth, Point otherEndpoint)
        {
            // A diamond is just a rotated square, so the
            // GetEdgePointOnRectangle() can be used if the
            // diamond and the other vertex location are first rotated 45 degrees
            // about the diamond's center.

            Double dHalfSquareWidth = m_dHalfWidth / Math.Sqrt(2.0);

            Rect oRotatedDiamond = new Rect(
                oVertexLocation.X - dHalfSquareWidth,
                oVertexLocation.Y - dHalfSquareWidth,
                2.0 * dHalfSquareWidth,
                2.0 * dHalfSquareWidth
                );

            Matrix oMatrix = GetRotatedMatrix(oVertexLocation, 45);
            Point oRotatedOtherVertexLocation = oMatrix.Transform(otherEndpoint);

            var oRotatedEdgeEndpoint = GetEdgeEndpointOnRectangle(oVertexLocation, oRotatedDiamond, oRotatedOtherVertexLocation);

            // Now rotate the computed edge endpoint in the other direction.

            oMatrix = GetRotatedMatrix(oVertexLocation, -45);

            return oMatrix.Transform(oRotatedEdgeEndpoint);
            //
        }

        public static Point GetEdgeEndpointOnRectangle(Point oVertexALocation, Rect oVertexARectangle, Point oVertexBLocation)
        {
           /* if (oVertexALocation == oVertexBLocation)
                return oVertexALocation;

            Double dVertexAX = oVertexALocation.X;
            Double dVertexAY = oVertexALocation.Y;

            Double dVertexBX = oVertexBLocation.X;
            Double dVertexBY = oVertexBLocation.Y;

            Double dHalfVertexARectangleWidth = oVertexARectangle.Width / 2.0;
            Double dHalfVertexARectangleHeight = oVertexARectangle.Height / 2.0;

            // Get the angle between vertex A and vertex B.

            Double dEdgeAngle = MathHelper.GetAngleBetweenPointsRadians(
                oVertexALocation, oVertexBLocation);

            // Get the angle that defines the aspect ratio of vertex A's rectangle.

            Double dAspectAngle = Math.Atan2(
                dHalfVertexARectangleHeight, dHalfVertexARectangleWidth);

            if (dEdgeAngle >= -dAspectAngle && dEdgeAngle < dAspectAngle)
            {
                // For a square, this is -45 degrees to 45 degrees.
                Debug.Assert(dVertexBX != dVertexAX);
                return new Point(
                    dVertexAX + dHalfVertexARectangleWidth,
                    dVertexAY + dHalfVertexARectangleWidth *
                        ((dVertexBY - dVertexAY) / (dVertexBX - dVertexAX))
                    );
            }

            if (dEdgeAngle >= dAspectAngle && dEdgeAngle < Math.PI - dAspectAngle)
            {
                // For a square, this is 45 degrees to 135 degrees.
                //Debug.Assert(dVertexBY != dVertexAY);
                return new Point(
                    dVertexAX + dHalfVertexARectangleHeight *
                        ((dVertexBX - dVertexAX) / (dVertexAY - dVertexBY)),
                    dVertexAY - dHalfVertexARectangleHeight
                    );
            }

            if (dEdgeAngle < -dAspectAngle && dEdgeAngle >= -Math.PI + dAspectAngle)
            {
                // For a square, this is -45 degrees to -135 degrees.
                Debug.Assert(dVertexBY != dVertexAY);
                return new Point(
                    dVertexAX + dHalfVertexARectangleHeight *
                        ((dVertexBX - dVertexAX) / (dVertexBY - dVertexAY)),
                    dVertexAY + dHalfVertexARectangleHeight
                    );
            }

            // For a square, this is 135 degrees to 180 degrees and -135 degrees to
            // -180 degrees.
            Debug.Assert(dVertexAX != dVertexBX);
            return new Point(
                dVertexAX - dHalfVertexARectangleWidth,
                dVertexAY + dHalfVertexARectangleWidth *
                    ((dVertexBY - dVertexAY) / (dVertexAX - dVertexBX))
                );*/

            var leftSide = Intersects(new Vector(oVertexALocation.X, oVertexALocation.Y), new Vector(oVertexBLocation.X, oVertexBLocation.Y), new Vector(oVertexARectangle.X, oVertexARectangle.Y), new Vector(oVertexARectangle.X, oVertexARectangle.Y + oVertexARectangle.Height));
            var bottomSide = Intersects(new Vector(oVertexALocation.X, oVertexALocation.Y), new Vector(oVertexBLocation.X, oVertexBLocation.Y), new Vector(oVertexARectangle.X, oVertexARectangle.Y + oVertexARectangle.Height), new Vector(oVertexARectangle.X + oVertexARectangle.Width, oVertexARectangle.Y + oVertexARectangle.Height));
            var rightSide = Intersects(new Vector(oVertexALocation.X, oVertexALocation.Y), new Vector(oVertexBLocation.X, oVertexBLocation.Y), new Vector(oVertexARectangle.X + oVertexARectangle.Width, oVertexARectangle.Y), new Vector(oVertexARectangle.X + oVertexARectangle.Width, oVertexARectangle.Y + oVertexARectangle.Height));
            var topSide = Intersects(new Vector(oVertexALocation.X, oVertexALocation.Y), new Vector(oVertexBLocation.X, oVertexBLocation.Y), new Vector(oVertexARectangle.X, oVertexARectangle.Y), new Vector(oVertexARectangle.X + oVertexARectangle.Width, oVertexARectangle.Y));

            var pt = new Point(oVertexALocation.X, oVertexALocation.Y);

            // Get the rectangle side where intersection of the proposed Edge path occurred.
            if (leftSide != null)
                pt = new Point(leftSide.Value.X, leftSide.Value.Y);
            else if (bottomSide != null)
                pt = new Point(bottomSide.Value.X, bottomSide.Value.Y);
            else if (rightSide != null)
                pt = new Point(rightSide.Value.X, rightSide.Value.Y);
            else if (topSide != null)
                pt = new Point(topSide.Value.X, topSide.Value.Y);

            return pt;
        }

        public static PathFigure GenerateOldArrow(Point p1, Point p2)
        {
            Vector v = p1 - p2; v = v / v.Length * 5;
            Vector n = new Vector(-v.Y, v.X) * 0.7;
            var fig = new PathFigure(p2, new PathSegment[] {	new LineSegment(p2 + v - n, true),
			                                           	    new LineSegment(p2 + v + n, true)}, true);
            TryFreeze(fig);
            return fig;
        }

        public static PathFigure GenerateArrow(Point oArrowTipLocation, Point start, Point end, double customAngle = 0.1)
        {
            //Debug.Assert(dEdgeWidth > 0);

            // Compute the arrow's dimensions.  The width factor is arbitrary and
            // was determined experimentally.

            //const Double WidthFactor = 1.5;
            var dArrowAngle = customAngle == 0.1 ? MathHelper.GetAngleBetweenPointsRadians(start, end) : customAngle;
            Double dArrowTipX = oArrowTipLocation.X;
            Double dArrowTipY = oArrowTipLocation.Y;
            Double dArrowWidth = 3.0; //TODO dynamic width
            Double dArrowHalfHeight = dArrowWidth / 2.0;
            Double dX = dArrowTipX - dArrowWidth;

            // Compute the arrow's three points as if the arrow were at an angle of
            // zero degrees, then use a rotated transform to adjust for the actual
            // specified angle.

            Point[] aoPoints = new Point[] {

            // Index 0: Arrow tip.

            oArrowTipLocation,

            // Index 1: Arrow bottom.

            new Point(dX, dArrowTipY - dArrowHalfHeight),

            // Index 2: Arrow top.

            new Point(dX, dArrowTipY + dArrowHalfHeight),

            // Index 3: Center of the flat end of the arrow.
            //
            // Note: The 0.2 is to avoid a gap between the edge endcap and the
            // flat end of the arrow, but it sometimes causes the two to
            // overlap slightly, and that can show if the edge isn't opaque.
            // What is the correct way to get the endcap to merge invisibly
            // with the arrow?

            new Point(dX + 0.2, dArrowTipY)
            };

            Matrix oMatrix = GetRotatedMatrix(oArrowTipLocation,
                -MathHelper.RadiansToDegrees(dArrowAngle));

            oMatrix.Transform(aoPoints);

            return GeometryHelper.GetPathFigureFromPoints(aoPoints[0], aoPoints[1], aoPoints[2]);
        }

        public static Matrix GetRotatedMatrix(Point centerOfRotation, Double angleToRotateDegrees)
        {
            Matrix oMatrix = Matrix.Identity;

            oMatrix.RotateAt(angleToRotateDegrees,
                centerOfRotation.X, centerOfRotation.Y);

            return (oMatrix);
        }

        public static PathFigure GetPathFigureFromPoints(Point startPoint, params Point[] otherPoints)
        {
            Int32 iOtherPoints = otherPoints.Length;
            var oPathFigure = new PathFigure() { StartPoint = startPoint };
            var oPathSegmentCollection = new PathSegmentCollection(iOtherPoints);

            for (Int32 i = 0; i < iOtherPoints; i++)
                oPathSegmentCollection.Add(new LineSegment(otherPoints[i], true));

            oPathFigure.Segments = oPathSegmentCollection;
            oPathFigure.IsClosed = true;
            GeometryHelper.TryFreeze(oPathFigure);
            return oPathFigure;
        }

        public static PathGeometry GetPathGeometryFromPoints(System.Windows.Point startPoint, params Point[] otherPoints)
        {
            Debug.Assert(otherPoints != null);

            Int32 iOtherPoints = otherPoints.Length;

            Debug.Assert(iOtherPoints > 0);

            PathFigure oPathFigure = new PathFigure() { StartPoint = startPoint };

            PathSegmentCollection oPathSegmentCollection =
                new PathSegmentCollection(iOtherPoints);

            for (Int32 i = 0; i < iOtherPoints; i++)
            {
                oPathSegmentCollection.Add(
                    new LineSegment(otherPoints[i], true));
            }

            oPathFigure.Segments = oPathSegmentCollection;
            oPathFigure.IsClosed = true;
            GeometryHelper.TryFreeze(oPathFigure);

            PathGeometry oPathGeometry = new PathGeometry();

            oPathGeometry.Figures.Add(oPathFigure);
            GeometryHelper.TryFreeze(oPathGeometry);

            return (oPathGeometry);
        }
    }
}
