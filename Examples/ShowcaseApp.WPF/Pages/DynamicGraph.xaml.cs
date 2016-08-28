using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using GraphX;
using GraphX.PCL.Common.Enums;
using GraphX.Controls;
using GraphX.Controls.Animations;
using GraphX.Controls.Models;
using ShowcaseApp.WPF.Models;

namespace ShowcaseApp.WPF.Pages
{
    /// <summary>
    /// Interaction logic for DynamicGraph.xaml
    /// </summary>
    public partial class DynamicGraph
    {
        private int _selIndex;

        public DynamicGraph()
        {
            InitializeComponent();
            var dgLogic = new LogicCoreExample();
            dg_Area.LogicCore = dgLogic;

            dg_addvertex.Click += dg_addvertex_Click;
            dg_remvertex.Click += dg_remvertex_Click;
            dg_addedge.Click += dg_addedge_Click;
            dg_remedge.Click += dg_remedge_Click;
            dgLogic.DefaultLayoutAlgorithm = LayoutAlgorithmTypeEnum.KK;
            dgLogic.DefaultOverlapRemovalAlgorithm = OverlapRemovalAlgorithmTypeEnum.FSA;
            dgLogic.DefaultOverlapRemovalAlgorithmParams.HorizontalGap = 50;
            dgLogic.DefaultOverlapRemovalAlgorithmParams.VerticalGap = 50;

            dgLogic.DefaultEdgeRoutingAlgorithm = EdgeRoutingAlgorithmTypeEnum.None;
            dgLogic.EdgeCurvingEnabled = true;
            dgLogic.Graph = new GraphExample();
            dg_Area.MoveAnimation = AnimationFactory.CreateMoveAnimation(MoveAnimation.Move, TimeSpan.FromSeconds(0.5));
            dg_Area.MoveAnimation.Completed += MoveAnimation_Completed;
            dg_Area.VertexSelected += dg_Area_VertexSelected;
            dg_test.Visibility = Visibility.Collapsed;
            dg_zoomctrl.AreaSelected += dg_zoomctrl_AreaSelected;

            dg_dragsource.PreviewMouseLeftButtonDown += dg_dragsource_PreviewMouseLeftButtonDown;
            dg_zoomctrl.AllowDrop = true;
            dg_zoomctrl.PreviewDrop += dg_Area_Drop;
            dg_zoomctrl.DragEnter += dg_Area_DragEnter;

            dg_zoomctrl.IsAnimationEnabled = false;
           /* dg_Area.VertexSelected += dg_Area_VertexSelectedForED;
            dg_zoomctrl.PreviewMouseMove += dg_Area_MouseMove;
            dg_zoomctrl.MouseDown += dg_zoomctrl_MouseDown;*/
            dg_Area.SetVerticesDrag(true, true);

            Loaded += DynamicGraph_Loaded;
            Unloaded += DynamicGraph_Unloaded;
        }

        private bool loaded = false;
        private Predicate<DependencyObject> _originalGlobalIsSnapping = null;
        private Predicate<DependencyObject> _originalGlobalIsSnappingIndividually = null;

        private void DynamicGraph_Loaded(object sender, RoutedEventArgs e)
        {
            if (loaded)
                return;
            loaded = true;

            _originalGlobalIsSnapping = DragBehaviour.GlobalIsSnappingPredicate;
            _originalGlobalIsSnappingIndividually = DragBehaviour.GlobalIsIndividualSnappingPredicate;

            DragBehaviour.GlobalIsSnappingPredicate = IsSnapping;
            DragBehaviour.GlobalIsIndividualSnappingPredicate = IsSnappingIndividually;
        }

        private void DynamicGraph_Unloaded(object sender, RoutedEventArgs e)
        {
            loaded = false;

            DragBehaviour.GlobalIsSnappingPredicate = _originalGlobalIsSnapping;
            DragBehaviour.GlobalIsIndividualSnappingPredicate = _originalGlobalIsSnappingIndividually;
        }

        void MoveAnimation_Completed(object sender, EventArgs e)
        {
            dg_zoomctrl.ZoomToFill();
        }

        #region Manual edge drawing

      /*  private bool _isInEdMode;
        private PathGeometry _edGeo;
        private VertexControl _edVertex;
        private EdgeControl _edEdge;
        private DataVertex _edFakeDv;

        void dg_zoomctrl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!_isInEdMode || _edGeo == null || _edEdge == null || _edVertex == null || e.LeftButton != MouseButtonState.Pressed) return;
            //place point
            var pos = dg_zoomctrl.TranslatePoint(e.GetPosition(dg_zoomctrl), dg_Area);
            var lastseg = _edGeo.Figures[0].Segments[_edGeo.Figures[0].Segments.Count - 1] as PolyLineSegment;
            if (lastseg != null) lastseg.Points.Add(pos);
            _edEdge.SetEdgePathManually(_edGeo);
        }

        void dg_Area_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isInEdMode || _edGeo == null || _edEdge == null || _edVertex == null) return;
            var pos = dg_zoomctrl.TranslatePoint(e.GetPosition(dg_zoomctrl), dg_Area);
            var lastseg = _edGeo.Figures[0].Segments[_edGeo.Figures[0].Segments.Count - 1] as PolyLineSegment;
            if (lastseg != null) lastseg.Points[lastseg.Points.Count - 1] = pos;
            _edEdge.SetEdgePathManually(_edGeo);
        }

        void dg_Area_VertexSelectedForED(object sender, VertexSelectedEventArgs args)
        {
            if (!_isInEdMode) return;
            if (_edVertex == null) //select starting vertex
            {
                _edVertex = args.VertexControl;
                _edFakeDv = new DataVertex { ID = -666 };
                _edGeo = new PathGeometry(new PathFigureCollection { new PathFigure { IsClosed = false, StartPoint = _edVertex.GetPosition(), Segments = new PathSegmentCollection { new PolyLineSegment(new List<Point> { new Point() }, true) } } });
                var dedge = new DataEdge(_edVertex.Vertex as DataVertex, _edFakeDv);
                _edEdge = new EdgeControl(_edVertex, null, dedge) { ManualDrawing = true };
                dg_Area.AddEdge(dedge, _edEdge);
                dg_Area.LogicCore.Graph.AddVertex(_edFakeDv);
                dg_Area.LogicCore.Graph.AddEdge(dedge);
                _edEdge.SetEdgePathManually(_edGeo);
            }
            else if (!Equals(_edVertex, args.VertexControl)) //finish draw
            {
                _edEdge.Target = args.VertexControl;
                var dedge = _edEdge.Edge as DataEdge;
                if (dedge != null) dedge.Target = args.VertexControl.Vertex as DataVertex;
                var fig = _edGeo.Figures[0];
                var seg = fig.Segments[_edGeo.Figures[0].Segments.Count - 1] as PolyLineSegment;

                if (seg != null && seg.Points.Count > 0)
                {
                    var targetPos = _edEdge.Target.GetPosition();
                    var sourcePos = _edEdge.Source.GetPosition();
                    //get the size of the source
                    var sourceSize = new Size
                    {
                        Width = _edEdge.Source.ActualWidth,
                        Height = _edEdge.Source.ActualHeight
                    };
                    var targetSize = new Size
                    {
                        Width = _edEdge.Target.ActualWidth,
                        Height = _edEdge.Target.ActualHeight
                    };

                    var srcStart = seg.Points.Count == 0 ? fig.StartPoint : seg.Points[0];
                    var srcEnd = seg.Points.Count > 1 ? (seg.Points[seg.Points.Count - 1] == targetPos ? seg.Points[seg.Points.Count - 2] : seg.Points[seg.Points.Count - 1]) : fig.StartPoint;
                    var p1 = GeometryHelper.GetEdgeEndpoint(sourcePos, new Rect(sourceSize), srcStart, _edEdge.Source.VertexShape);
                    var p2 = GeometryHelper.GetEdgeEndpoint(targetPos, new Rect(targetSize), srcEnd, _edEdge.Target.VertexShape);


                    fig.StartPoint = p1;
                    if (seg.Points.Count > 1)
                        seg.Points[seg.Points.Count - 1] = p2;
                }
                GeometryHelper.TryFreeze(_edGeo);
                _edEdge.SetEdgePathManually(new PathGeometry(_edGeo.Figures));
                _isInEdMode = false;
                ClearEdgeDrawing();
            }
        }


        void ClearEdgeDrawing()
        {
            _edGeo = null;
            if (_edFakeDv != null)
                dg_Area.LogicCore.Graph.RemoveVertex(_edFakeDv);
            _edFakeDv = null;
            _edVertex = null;
            _edEdge = null;
        }

        private void dg_butdraw_Click(object sender, RoutedEventArgs e)
        {
            if (!_isInEdMode)
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
                ClearEdgeDrawing();
            }
            _isInEdMode = !_isInEdMode;
        }
        */
        #endregion

        private void dg_findrandom_Click(object sender, RoutedEventArgs e)
        {
            if (!dg_Area.VertexList.Any()) return;
            _selIndex++;
            if (_selIndex >= dg_Area.VertexList.Count()) _selIndex = 0;
            var vc = dg_Area.VertexList.ToList()[_selIndex].Value;

            const int offset = 100;
            var pos = vc.GetPosition();
            dg_zoomctrl.ZoomToContent(new Rect(pos.X - offset, pos.Y - offset, vc.ActualWidth + offset * 2, vc.ActualHeight + offset * 3));
        }

        void dg_zoomctrl_AreaSelected(object sender, AreaSelectedEventArgs args)
        {
            var r = args.Rectangle;
            foreach (var item in from item in dg_Area.VertexList let offset = item.Value.GetPosition() let irect = new Rect(offset.X, offset.Y, item.Value.ActualWidth, item.Value.ActualHeight) where irect.IntersectsWith(r) select item) {
                SelectVertex(item.Value);
            }
        }

        #region Dragging example
        void dg_dragsource_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var data = new DataObject(typeof(object), new object());
            DragDrop.DoDragDrop(dg_dragsource, data, DragDropEffects.Link);
        }

        static void dg_Area_DragEnter(object sender, DragEventArgs e)
        {
            //don't show drag effect if we are on drag source or don't have any item of needed type dragged
            if (!e.Data.GetDataPresent(typeof(object)) || sender == e.Source)
            {
                e.Effects = DragDropEffects.None;
            }
        }

        void dg_Area_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof (object))) return;
            //how to get dragged data by its type
            var pos = dg_zoomctrl.TranslatePoint(e.GetPosition(dg_zoomctrl), dg_Area);
            var data = ThemedDataStorage.FillDataVertex(new DataVertex());
            var vc = new VertexControl(data);
            vc.SetPosition(pos);
            dg_Area.AddVertexAndData(data, vc);
        }

        #endregion

        void dg_Area_VertexSelected(object sender, VertexSelectedEventArgs args)
        {
            if (args.MouseArgs.LeftButton == MouseButtonState.Pressed)
            {
                if (Keyboard.IsKeyDown(Key.LeftCtrl))
                    SelectVertex(args.VertexControl);
            }
            else if (args.MouseArgs.RightButton == MouseButtonState.Pressed)
            {
                var countTagged = dg_Area.VertexList.Values.Count(DragBehaviour.GetIsTagged);
                var isTagged = DragBehaviour.GetIsTagged(args.VertexControl);
                args.VertexControl.ContextMenu = new ContextMenu();
                var mi = new MenuItem { Header = "Delete item" + (isTagged && countTagged > 1 ? "s" : ""), Tag = args.VertexControl, Margin = new Thickness(5) };
                mi.Click += mi_Click;
                args.VertexControl.ContextMenu.Items.Add(mi);
                args.VertexControl.ContextMenu.IsOpen = true;
            }
        }

        void mi_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem == null) return;
            var vc = menuItem.Tag as VertexControl;
            //if clicked vertex is tagged then remove all tagged vertices
            if (DragBehaviour.GetIsTagged(vc))
                dg_remvertex_Click(null, null);
            else //else remove only selected vertex
                if (vc != null) SafeRemoveVertex(vc);
        }

        void dg_remedge_Click(object sender, RoutedEventArgs e)
        {
            if (!dg_Area.EdgesList.Any()) return;
            //remove visual and data edge
            dg_Area.RemoveEdge(dg_Area.EdgesList.Last().Key, true);
        }

        void dg_addedge_Click(object sender, RoutedEventArgs e)
        {
            //add new edge between random vertices
            var dataEdge = GenerateRandomEdge();
            if (dataEdge == null) return;
            var ec = new EdgeControl(dg_Area.VertexList.FirstOrDefault(a => a.Key == dataEdge.Source).Value, dg_Area.VertexList.FirstOrDefault(a => a.Key == dataEdge.Target).Value, dataEdge);
            dg_Area.InsertEdgeAndData(dataEdge, ec);
        }

        void dg_remvertex_Click(object sender, RoutedEventArgs e)
        {
            //remove all tagged vertices from the graph entirely
            dg_Area.VertexList.Values
                .Where(DragBehaviour.GetIsTagged)
                .ToList()
                .ForEach(SafeRemoveVertex);
        }

        void dg_addvertex_Click(object sender, RoutedEventArgs e)
        {
            var data = ThemedDataStorage.FillDataVertex(new DataVertex());
            dg_Area.AddVertexAndData(data, new VertexControl(data));

            //we have to check if there is only one vertex and set coordinates manulay 
            //because layout algorithms skip all logic if there are less than two vertices
            if (dg_Area.VertexList.Count == 1)
            {
                dg_Area.VertexList.First().Value.SetPosition(0, 0);
                dg_Area.UpdateLayout(); //update layout to update vertex size
            } else dg_Area.RelayoutGraph(true);
            dg_zoomctrl.ZoomToFill();
        }

        /// <summary>
        /// Remove vertex and do all cleanup necessary for current demo
        /// </summary>
        /// <param name="vc">vertexControl object</param>
        private void SafeRemoveVertex(VertexControl vc)
        {
            dg_Area.RemoveVertexAndEdges(vc.Vertex as DataVertex);
            dg_zoomctrl.ZoomToFill();
        }

        /// <summary>
        /// generates random edge based on the current vertices data in the graph
        /// </summary>
        private DataEdge GenerateRandomEdge()
        {
            if (dg_Area.VertexList.Count < 2) return null;
            var vlist = dg_Area.LogicCore.Graph.Vertices.ToList();
            var rnd1 = vlist[ShowcaseHelper.Rand.Next(0, vlist.Count - 1)];
            vlist.Remove(rnd1);
            var rnd2 = vlist[ShowcaseHelper.Rand.Next(0, vlist.Count - 1)];
            return new DataEdge(rnd1, rnd2);
        }

        /// <summary>
        /// Select vertex by setting its tag and highlight value
        /// </summary>
        /// <param name="vc">VertexControl object</param>
        private void SelectVertex(DependencyObject vc)
        {
            if (DragBehaviour.GetIsTagged(vc))
            {
                HighlightBehaviour.SetHighlighted(vc, false);
                DragBehaviour.SetIsTagged(vc, false);
                vc.ClearValue(DragBehaviour.XSnapModifierProperty);
                vc.ClearValue(DragBehaviour.YSnapModifierProperty);
            }
            else
            {
                HighlightBehaviour.SetHighlighted(vc, true);
                DragBehaviour.SetIsTagged(vc, true);
                DragBehaviour.SetXSnapModifier(vc, ExaggeratedSnappingXModifier);
                DragBehaviour.SetYSnapModifier(vc, ExaggeratedSnappingYModifier);
            }
        }

        private bool IsSnapping(DependencyObject obj)
        {
            return dg_snap.IsChecked ?? false;
        }

        private bool IsSnappingIndividually(DependencyObject obj)
        {
            return dg_snapIndividually.IsChecked ?? false;
        }

        private double ExaggeratedSnappingXModifier(GraphAreaBase area, DependencyObject obj, double val)
        {
            if (dg_snapExaggerate.IsChecked ?? false)
            {
                return System.Math.Round(val * 0.01) * 100.0;
            }
            return DragBehaviour.GlobalXSnapModifier(area, obj, val);
        }

        private double ExaggeratedSnappingYModifier(GraphAreaBase area, DependencyObject obj, double val)
        {
            if (dg_snapExaggerate.IsChecked ?? false)
            {
                return System.Math.Round(val * 0.01) * 100.0;
            }
            return DragBehaviour.GlobalYSnapModifier(area, obj, val);
        }
    }
}
