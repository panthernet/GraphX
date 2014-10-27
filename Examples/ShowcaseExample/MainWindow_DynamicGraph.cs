using GraphX;
using GraphX.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using System.Diagnostics;
using System.Windows.Shapes;
using GraphX.Controls;

namespace ShowcaseExample
{
    public partial class MainWindow
    {
        /// <summary>
        /// tmp collection to speedup selected vertices search
        /// </summary>
        private List<VertexControl> DG_SelectedVertices = new List<VertexControl>();
        
        public void DynamicGraph_Constructor()
        {
            var dg_Logic = new LogicCoreExample();
            dg_Area.LogicCore = dg_Logic;

            dg_addvertex.Click += dg_addvertex_Click;
            dg_remvertex.Click += dg_remvertex_Click;
            dg_addedge.Click += dg_addedge_Click;
            dg_remedge.Click += dg_remedge_Click;
            dg_Logic.DefaultLayoutAlgorithm = LayoutAlgorithmTypeEnum.KK;
            dg_Logic.DefaultOverlapRemovalAlgorithm = OverlapRemovalAlgorithmTypeEnum.FSA;
            dg_Logic.DefaultEdgeRoutingAlgorithm = EdgeRoutingAlgorithmTypeEnum.None;
            dg_Logic.EdgeCurvingEnabled = true;
            dg_Logic.Graph = new GraphExample();
            dg_Area.MoveAnimation = AnimationFactory.CreateMoveAnimation(MoveAnimation.Move, TimeSpan.FromSeconds(0.5));
            dg_Area.MoveAnimation.Completed += MoveAnimation_Completed;
            //dg_Area.DeleteAnimation = AnimationFactory.CreateDeleteAnimation(DeleteAnimation.Fade, TimeSpan.FromSeconds(0.3));
           // dg_Area.MouseOverAnimation = AnimationFactory.CreateMouseOverAnimation(MouseOverAnimation.Scale, TimeSpan.FromSeconds(0.3));
            dg_Area.VertexSelected += dg_Area_VertexSelected;
            dg_test.Visibility = System.Windows.Visibility.Collapsed;
            dg_test.Click += dg_test_Click;
            dg_zoomctrl.AreaSelected += dg_zoomctrl_AreaSelected;
            //dg_Area.UseNativeObjectArrange = true;


            dg_dragsource.PreviewMouseLeftButtonDown += dg_dragsource_PreviewMouseLeftButtonDown;
            dg_zoomctrl.AllowDrop = true;
            dg_zoomctrl.PreviewDrop += dg_Area_Drop;
            dg_zoomctrl.DragEnter += dg_Area_DragEnter;

            dg_zoomctrl.IsAnimationDisabled = true;
            ZoomControl.SetViewFinderVisibility(dg_zoomctrl, System.Windows.Visibility.Visible);


            dg_Area.VertexSelected += dg_Area_VertexSelectedForED;
            dg_zoomctrl.PreviewMouseMove += dg_Area_MouseMove;
            dg_zoomctrl.MouseDown += dg_zoomctrl_MouseDown;

            //dg_zoomctrl.KeepContentInBounds = true;
        }

        void MoveAnimation_Completed(object sender, EventArgs e)
        {
            dg_zoomctrl.ZoomToFill();
        }

        #region Manual edge drawing

        private bool _isInEDMode;
        private PathGeometry _edGeo;
        private VertexControl _edVertex;
        private EdgeControl _edEdge;
        private DataVertex _edFakeDV;

        void dg_zoomctrl_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_isInEDMode && _edGeo != null && _edEdge != null && _edVertex != null && e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                //place point
                var pos = dg_zoomctrl.TranslatePoint(e.GetPosition(dg_zoomctrl), dg_Area);
                var lastseg = _edGeo.Figures[0].Segments[_edGeo.Figures[0].Segments.Count - 1] as PolyLineSegment;
                lastseg.Points.Add(pos);
                _edEdge.SetEdgePathManually(_edGeo);
            }
        }

        void dg_Area_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_isInEDMode && _edGeo != null && _edEdge != null && _edVertex != null)
            {
                var pos = dg_zoomctrl.TranslatePoint(e.GetPosition(dg_zoomctrl), dg_Area);
                var lastseg = _edGeo.Figures[0].Segments[_edGeo.Figures[0].Segments.Count - 1] as PolyLineSegment;
                lastseg.Points[lastseg.Points.Count - 1] = pos;
                _edEdge.SetEdgePathManually(_edGeo);
            }

        }

        void dg_Area_VertexSelectedForED(object sender, GraphX.Models.VertexSelectedEventArgs args)
        {
            if (_isInEDMode)
            {
                if (_edVertex == null) //select starting vertex
                {
                    _edVertex = args.VertexControl;
                    _edFakeDV = new DataVertex() { ID = -666 };
                    _edGeo = new PathGeometry(new PathFigureCollection() { new PathFigure() { IsClosed = false, StartPoint = _edVertex.GetPosition(), Segments = new PathSegmentCollection() { new PolyLineSegment(new List<Point>() { new Point() }, true) } } });
                    var dedge = new DataEdge(_edVertex.Vertex as DataVertex, _edFakeDV);
                    _edEdge = new EdgeControl(_edVertex, null, dedge) { ManualDrawing = true };
                    dg_Area.AddEdge(dedge, _edEdge);
                    dg_Area.LogicCore.Graph.AddVertex(_edFakeDV);
                    dg_Area.LogicCore.Graph.AddEdge(dedge);
                    _edEdge.SetEdgePathManually(_edGeo);
                }
                else if (_edVertex != args.VertexControl) //finish draw
                {
                    _edEdge.Target = args.VertexControl;
                    var dedge = _edEdge.Edge as DataEdge;
                    dedge.Target = args.VertexControl.Vertex as DataVertex;
                    var fig = _edGeo.Figures[0];
                    var seg = fig.Segments[_edGeo.Figures[0].Segments.Count - 1] as PolyLineSegment;

                    if (seg.Points.Count > 0)
                    {
                        var targetPos = _edEdge.Target.GetPosition();
                        var sourcePos = _edEdge.Source.GetPosition();
                        //get the size of the source
                        var sourceSize = new Size()
                        {
                            Width = _edEdge.Source.ActualWidth,
                            Height = _edEdge.Source.ActualHeight
                        };
                        var targetSize = new Size()
                        {
                            Width = _edEdge.Target.ActualWidth,
                            Height = _edEdge.Target.ActualHeight
                        };

                        var src_start = seg.Points.Count == 0 ? fig.StartPoint : seg.Points[0];
                        var src_end = seg.Points.Count > 1 ? (seg.Points[seg.Points.Count - 1] == targetPos ? seg.Points[seg.Points.Count - 2] : seg.Points[seg.Points.Count - 1]) : fig.StartPoint;
                        Point p1 = GeometryHelper.GetEdgeEndpoint(sourcePos, new Rect(sourceSize), src_start, _edEdge.Source.MathShape);
                        Point p2 = GeometryHelper.GetEdgeEndpoint(targetPos, new Rect(targetSize), src_end, _edEdge.Target.MathShape);


                        fig.StartPoint = p1;
                        if (seg.Points.Count > 1)
                            seg.Points[seg.Points.Count - 1] = p2;
                    }
                    GeometryHelper.TryFreeze(_edGeo);
                    _edEdge.SetEdgePathManually(new PathGeometry(_edGeo.Figures));
                    _isInEDMode = false;
                    clearEdgeDrawing();
                }
            }
        }


        void clearEdgeDrawing()
        {
            _edGeo = null;
            if (_edFakeDV != null)
                dg_Area.LogicCore.Graph.RemoveVertex(_edFakeDV);
            _edFakeDV = null;
            _edVertex = null;
            _edEdge = null;
        }

        private void dg_butdraw_Click(object sender, RoutedEventArgs e)
        {
            if (!_isInEDMode)
            {
                if (dg_Area.VertexList.Count() < 2)
                {
                    MessageBox.Show("Please add more vertices before proceed with this action!", "Starting to draw custom edge...", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                MessageBox.Show("Please select any vertex to define edge starting point!", "Starting to draw custom edge...", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Edge drawing mode has been canceled!");
                if (_edEdge != null)
                    _edEdge.SetEdgePathManually(null);
                clearEdgeDrawing();
            }
            _isInEDMode = !_isInEDMode;
        }

        #endregion


        private int _selIndex = 0;
        private void dg_findrandom_Click(object sender, RoutedEventArgs e)
        {
            if (dg_Area.VertexList.Count() == 0) return;
            _selIndex++;
            if (_selIndex >= dg_Area.VertexList.Count()) _selIndex = 0;
            var vc = dg_Area.VertexList.ToList()[_selIndex].Value;

            var offset = 100;
            var pos = vc.GetPosition();
            dg_zoomctrl.ZoomToContent(new Rect(pos.X - offset, pos.Y - offset, vc.ActualWidth + offset*2, vc.ActualHeight + offset*3));
        }

        void dg_zoomctrl_AreaSelected(object sender, AreaSelectedEventArgs args)
        {
            var r = args.Rectangle;
            foreach(var item in dg_Area.VertexList)
            {
                var offset = item.Value.GetPosition();
                var irect = new Rect(offset.X, offset.Y, item.Value.ActualWidth, item.Value.ActualHeight);
                if (irect.IntersectsWith(r))
                    SelectVertex(item.Value);
            }
        }

        #region Dragging example
        void dg_dragsource_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var data = new DataObject(typeof(object), new object());
            DragDrop.DoDragDrop(dg_dragsource, data, DragDropEffects.Link);
        }

        void dg_Area_DragEnter(object sender, System.Windows.DragEventArgs e)
        {
            //don't show drag effect if we are on drag source or don't have any item of needed type dragged
            if (!e.Data.GetDataPresent(typeof(object)) || sender == e.Source)
            {
                e.Effects = DragDropEffects.None;
            }
        }

        void dg_Area_Drop(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(object)))
            {
                //how to get dragged data by its type
                var myobject = e.Data.GetData(typeof(object)) as object;

                var pos = dg_zoomctrl.TranslatePoint(e.GetPosition(dg_zoomctrl), dg_Area);
                var data = new DataVertex();
                dg_Area.LogicCore.Graph.AddVertex(data);
                var vc = new VertexControl(data);
                dg_Area.AddVertex(data, vc);
                //dg_Area.RelayoutGraph(true);
                GraphAreaBase.SetX(vc, pos.X, true);
                GraphAreaBase.SetY(vc, pos.Y, true);
                if (dg_Area.VertexList.Count() == 1)
                {
                    var of = System.Windows.Media.VisualTreeHelper.GetOffset(vc);
                    int offset = 400;
                    dg_zoomctrl.ZoomTo(new Rect(of.X - offset, of.Y - offset, vc.ActualWidth + offset * 2, vc.ActualHeight + offset * 2));

                    /* //FOR ZoomControl
                    zoomctrl.Mode = GraphX.Controls.ZoomControlModes.Original;
                    */
                }else dg_zoomctrl.ZoomToFill();
            }
        }

        #endregion

        void dg_test_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            //dg_zoomctrl.Zoom(50);
            //dg_zoomctrl.ZoomOn = Xceed.Wpf.Toolkit.Zoombox.ZoomboxZoomOn.View;
        }

        private void SelectVertex(VertexControl vc)
        {
            if (DG_SelectedVertices.Contains(vc))
            {
                DG_SelectedVertices.Remove(vc);
                HighlightBehaviour.SetHighlighted(vc, false);
                DragBehaviour.SetIsTagged(vc, false);
            }
            else
            {
                DG_SelectedVertices.Add(vc);
                HighlightBehaviour.SetHighlighted(vc, true);
                DragBehaviour.SetIsTagged(vc, true);
            }
        }

        void dg_Area_VertexSelected(object sender, VertexSelectedEventArgs args)
        {
            if (args.MouseArgs.LeftButton == MouseButtonState.Pressed)
            {
                if (Keyboard.IsKeyDown(Key.LeftCtrl))
                {
                    //if (DragBehaviour.GetIsDragging(args.VertexControl)) return;

                    SelectVertex(args.VertexControl);
                }
            }
            else if (args.MouseArgs.RightButton == MouseButtonState.Pressed)
            {
                args.VertexControl.ContextMenu = new System.Windows.Controls.ContextMenu();
                var mi = new System.Windows.Controls.MenuItem() { Header = "Delete item", Tag = args.VertexControl };
                mi.Click += mi_Click;
                args.VertexControl.ContextMenu.Items.Add(mi);
            }
        }

        void mi_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var vc = (sender as System.Windows.Controls.MenuItem).Tag as VertexControl;
            if(vc != null) SafeRemoveVertex(vc, true);
        }

        void dg_remedge_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (dg_Area.EdgesList.Count() > 0)
            {
                dg_Area.LogicCore.Graph.RemoveEdge(dg_Area.EdgesList.Last().Key);
                dg_Area.RemoveEdge(dg_Area.EdgesList.Last().Key);
            }
        }

        void dg_addedge_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (dg_Area.VertexList.Count() < 2) return;
            var vlist = (dg_Area.VertexList as Dictionary<DataVertex, VertexControl>).Keys.ToList();
            var rnd1 = vlist[Rand.Next(0, dg_Area.VertexList.Count() - 1)];
            var rnd2 = vlist[Rand.Next(0, dg_Area.VertexList.Count() - 1)];
            var data = new DataEdge(rnd1, rnd2);
            dg_Area.LogicCore.Graph.AddEdge(data);
            var ec = new EdgeControl(dg_Area.VertexList.FirstOrDefault(a => a.Key == rnd1).Value, dg_Area.VertexList.FirstOrDefault(a => a.Key == rnd2).Value, data) { DataContext = data };
            dg_Area.InsertEdge(data, ec);
            //dg_Area.RelayoutGraph(true);
        }

        void dg_remvertex_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DG_SelectedVertices.Count > 0)
            {
                foreach (var vc in DG_SelectedVertices)
                    SafeRemoveVertex(vc);
                DG_SelectedVertices.Clear();
            }
        }

        private void SafeRemoveVertex(VertexControl vc, bool removeFromSelected = false)
        {
            //remove all adjacent edges
            foreach (var item in dg_Area.GetRelatedControls(vc, GraphControlType.Edge, EdgesType.All))
            {
                var ec = item as EdgeControl;
                dg_Area.LogicCore.Graph.RemoveEdge(ec.Edge as DataEdge);
                dg_Area.RemoveEdge(ec.Edge as DataEdge);
            }
            dg_Area.LogicCore.Graph.RemoveVertex(vc.Vertex as DataVertex);
            dg_Area.RemoveVertex(vc.Vertex as DataVertex);
            if (removeFromSelected && DG_SelectedVertices.Contains(vc))
                DG_SelectedVertices.Remove(vc);
            dg_zoomctrl.ZoomToFill();

        }

        void dg_addvertex_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            /*var data = new DataVertex("Vertex "+ dg_Area.VertexList.Count + 1);
            dg_Area.LogicCore.Graph.AddVertex(data);
            dg_Area.AddVertex(data, new VertexControl(data));
            dg_Area.VertexList.Values.Last().SetPosition(new Point(0,0));

            data = new DataVertex("Vertex " + dg_Area.VertexList.Count + 2);
            dg_Area.LogicCore.Graph.AddVertex(data);
            dg_Area.AddVertex(data, new VertexControl(data));
            dg_Area.VertexList.Values.Last().SetPosition(new Point(300, 300));
            data = new DataVertex("Vertex " + dg_Area.VertexList.Count + 2);
            dg_Area.LogicCore.Graph.AddVertex(data);
            dg_Area.AddVertex(data, new VertexControl(data));
            dg_Area.VertexList.Values.Last().SetPosition(new Point(0, 300));

            dg_zoomctrl.ZoomToFill();
            return;*/
             
             
            var data = new DataVertex("Vertex " + dg_Area.VertexList.Count() + 1);
            dg_Area.LogicCore.Graph.AddVertex(data);
            dg_Area.AddVertex(data, new VertexControl(data));

            //we have to check if there is only one vertex and set coordinates manulay 
            //because layout algorithms skip all logic if there are less than two vertices
            if (dg_Area.VertexList.Count() == 1)
                dg_Area.VertexList.First().Value.SetPosition(0,0);
            else dg_Area.RelayoutGraph(true);
            dg_zoomctrl.ZoomToFill();
        }
    }
}
