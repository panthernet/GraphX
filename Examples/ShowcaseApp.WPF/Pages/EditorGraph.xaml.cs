using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GraphX;
using GraphX.PCL.Common.Enums;
using GraphX.Controls;
using GraphX.Controls.Models;
using ShowcaseApp.WPF.Models;

namespace ShowcaseApp.WPF.Pages
{
    /// <summary>
    /// Interaction logic for DynamicGraph.xaml
    /// </summary>
    public partial class EditorGraph: IDisposable
    {
        /// <summary>
        /// tmp collection to speedup selected vertices search
        /// </summary>
        private readonly List<VertexControl> _selectedVertices = new List<VertexControl>();

        private EditorOperationMode _opMode = EditorOperationMode.Select;
        private VertexControl _ecFrom;
        private EditorObjectManager _editorManager;

        public EditorGraph()
        {
            InitializeComponent();
            _editorManager = new EditorObjectManager(graphArea, zoomCtrl);
            var dgLogic = new LogicCoreExample();
            graphArea.LogicCore = dgLogic;
            graphArea.VertexSelected += graphArea_VertexSelected;
            graphArea.EdgeSelected += graphArea_EdgeSelected;
            graphArea.SetVerticesMathShape(VertexShape.Circle);
           // addVertexButton.Click += addVertexButton_Click;
           // addEdgeButton.Click += addEdgeButton_Click;

            dgLogic.DefaultLayoutAlgorithm = LayoutAlgorithmTypeEnum.Custom;
            dgLogic.DefaultOverlapRemovalAlgorithm = OverlapRemovalAlgorithmTypeEnum.None;
            dgLogic.DefaultEdgeRoutingAlgorithm = EdgeRoutingAlgorithmTypeEnum.None;
            dgLogic.EdgeCurvingEnabled = true;
            

            //graphArea.MoveAnimation = AnimationFactory.CreateMoveAnimation(MoveAnimation.Move, TimeSpan.FromSeconds(0.5));
            //graphArea.MoveAnimation.Completed += MoveAnimation_Completed;
            //graphArea.VertexSelected += dg_Area_VertexSelected;
            
            

            zoomCtrl.IsAnimationDisabled = true;
            ZoomControl.SetViewFinderVisibility(zoomCtrl, Visibility.Visible);
            zoomCtrl.Zoom = 2;
            zoomCtrl.MinZoom = .5;
            zoomCtrl.MaxZoom = 50;
            zoomCtrl.ZoomSensitivity = 25;
            zoomCtrl.MouseDown += zoomCtrl_MouseDown;
            var tb = new TextBlock() {Text = "AAAA"};


            //zoomCtrl.ZoomToContent(new System.Windows.Rect(0,0, 500, 500));

            butDelete.Checked += ToolbarButton_Checked;
            butSelect.Checked += ToolbarButton_Checked;
            butEdit.Checked += ToolbarButton_Checked;

            butSelect.IsChecked = true;

        }

        void graphArea_EdgeSelected(object sender, EdgeSelectedEventArgs args)
        {
            if (args.MouseArgs.LeftButton == MouseButtonState.Pressed && _opMode == EditorOperationMode.Delete)
            {
                graphArea.LogicCore.Graph.RemoveEdge(args.EdgeControl.Edge as DataEdge);
                graphArea.RemoveEdge(args.EdgeControl.Edge as DataEdge);
            }
        }

        void graphArea_VertexSelected(object sender, VertexSelectedEventArgs args)
        {
             if(args.MouseArgs.LeftButton == MouseButtonState.Pressed)            
             {
                 if (_opMode == EditorOperationMode.Edit)
                    CreateEdgeControl(args.VertexControl);
                 else if(_opMode == EditorOperationMode.Delete)
                     SafeRemoveVertex(args.VertexControl);
                 else if (_opMode == EditorOperationMode.Select && args.Modifiers == ModifierKeys.Control)
                     SelectVertex(args.VertexControl);
             }
        }

        private void SelectVertex(VertexControl vc)
        {
            if (_selectedVertices.Contains(vc))
            {
                _selectedVertices.Remove(vc);
                HighlightBehaviour.SetHighlighted(vc, false);
                DragBehaviour.SetIsTagged(vc, false);
            }
            else
            {
                _selectedVertices.Add(vc);
                HighlightBehaviour.SetHighlighted(vc, true);
                DragBehaviour.SetIsTagged(vc, true);
            }
        }

        void zoomCtrl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //create vertices and edges only in Edit mode
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (_opMode == EditorOperationMode.Edit)
                {
                    var pos = zoomCtrl.TranslatePoint(e.GetPosition(zoomCtrl), graphArea);
                    pos.Offset(-22.5,-22.5);
                    var vc = CreateVertexControl(pos);
                    if (_ecFrom != null)
                        CreateEdgeControl(vc);
                }else if(_opMode == EditorOperationMode.Select)
                {
                    ClearSelectMode(true);
                }
            }
        }


        void ToolbarButton_Checked(object sender, RoutedEventArgs e)
        {
            if(butDelete.IsChecked == true && sender == butDelete)
            {
                butEdit.IsChecked = false;
                butSelect.IsChecked = false;
                zoomCtrl.Cursor = Cursors.Help;
                _opMode = EditorOperationMode.Delete;
                ClearEditMode();
                ClearSelectMode();
                return;
            }
            if (butEdit.IsChecked == true && sender == butEdit)
            {
                butDelete.IsChecked = false;
                butSelect.IsChecked = false;
                zoomCtrl.Cursor = Cursors.Pen;
                _opMode = EditorOperationMode.Edit;
                ClearSelectMode();
                return;
            }
            if (butSelect.IsChecked == true && sender == butSelect)
            {
                butEdit.IsChecked = false;
                butDelete.IsChecked = false;
                zoomCtrl.Cursor = Cursors.Hand;
                _opMode = EditorOperationMode.Select;
                ClearEditMode();
                graphArea.SetVerticesDrag(true, true);
                return;
            }
        }

        private void ClearSelectMode(bool soft = false)
        {
            if (_selectedVertices != null && _selectedVertices.Any())
            {
                _selectedVertices.ForEach(a =>
                {
                    HighlightBehaviour.SetHighlighted(a, false);
                    DragBehaviour.SetIsTagged(a, false);
                });
                _selectedVertices.Clear();
            }
            if (!soft)
            {
                graphArea.SetVerticesDrag(false);
            }
        }

        private void ClearEditMode()
        {
            if (_ecFrom != null) HighlightBehaviour.SetHighlighted(_ecFrom, false);
            _editorManager.DestroyVirtualEdge();
            _ecFrom = null;
        }

        private VertexControl CreateVertexControl(Point position)
        {
            var data = new DataVertex("Vertex " + (graphArea.VertexList.Count + 1)) { ImageId = ShowcaseHelper.Rand.Next(0, ThemedDataStorage.EditorImages.Count) };
            graphArea.LogicCore.Graph.AddVertex(data);
            var vc = new VertexControl(data);
            graphArea.AddVertex(data, vc);
            GraphAreaBase.SetX(vc, position.X, true);
            GraphAreaBase.SetY(vc, position.Y, true);
            return vc;
        }

        private void CreateEdgeControl(VertexControl vc)
        {
            if(_ecFrom == null)
            {
                _editorManager.CreateVirtualEdge(vc, vc.GetPosition());
                _ecFrom = vc;
                HighlightBehaviour.SetHighlighted(_ecFrom, true);
                return;
            }
            if(_ecFrom == vc) return;

            var data = new DataEdge((DataVertex)_ecFrom.Vertex, (DataVertex)vc.Vertex);
            graphArea.LogicCore.Graph.AddEdge(data);
            var ec = new EdgeControl(_ecFrom, vc, data);
            graphArea.InsertEdge(data, ec);

            HighlightBehaviour.SetHighlighted(_ecFrom, false);
            _ecFrom = null;
            _editorManager.DestroyVirtualEdge();
        }

        private void SafeRemoveVertex(VertexControl vc, bool removeFromSelected = false)
        {
            //remove all adjacent edges
            foreach (var ec in graphArea.GetRelatedControls(vc, GraphControlType.Edge, EdgesType.All).OfType<EdgeControl>())
            {
                graphArea.LogicCore.Graph.RemoveEdge(ec.Edge as DataEdge);
                graphArea.RemoveEdge(ec.Edge as DataEdge);
            }
            graphArea.LogicCore.Graph.RemoveVertex(vc.Vertex as DataVertex);
            graphArea.RemoveVertex(vc.Vertex as DataVertex);
            if (removeFromSelected && _selectedVertices.Contains(vc))
                _selectedVertices.Remove(vc);
        }

        public void Dispose()
        {
            if(_editorManager != null)
                _editorManager.Dispose();
            if(graphArea != null)
                graphArea.Dispose();                       
        }
    }


}
