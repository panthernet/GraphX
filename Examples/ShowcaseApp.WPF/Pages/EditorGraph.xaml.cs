using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using GraphX.Common.Enums;
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
        private EditorOperationMode _opMode = EditorOperationMode.Select;
        private VertexControl _ecFrom;
        private readonly EditorObjectManager _editorManager;

        public EditorGraph()
        {
            InitializeComponent();
            _editorManager = new EditorObjectManager(graphArea, zoomCtrl);
            var dgLogic = new LogicCoreExample();
            graphArea.LogicCore = dgLogic;
            graphArea.VertexSelected += graphArea_VertexSelected;
            graphArea.EdgeSelected += graphArea_EdgeSelected;
            graphArea.SetVerticesMathShape(VertexShape.Circle);
            graphArea.VertexLabelFactory = new DefaultVertexlabelFactory();

            dgLogic.DefaultLayoutAlgorithm = LayoutAlgorithmTypeEnum.Custom;
            dgLogic.DefaultOverlapRemovalAlgorithm = OverlapRemovalAlgorithmTypeEnum.None;
            dgLogic.DefaultEdgeRoutingAlgorithm = EdgeRoutingAlgorithmTypeEnum.None;
            dgLogic.EdgeCurvingEnabled = true;

            zoomCtrl.IsAnimationEnabled = false;
            ZoomControl.SetViewFinderVisibility(zoomCtrl, Visibility.Visible);
            zoomCtrl.Zoom = 2;
            zoomCtrl.MinZoom = .5;
            zoomCtrl.MaxZoom = 50;
            zoomCtrl.ZoomSensitivity = 25;
            zoomCtrl.MouseDown += zoomCtrl_MouseDown;

            butDelete.Checked += ToolbarButton_Checked;
            butSelect.Checked += ToolbarButton_Checked;
            butEdit.Checked += ToolbarButton_Checked;

            butSelect.IsChecked = true;
        }

        void graphArea_EdgeSelected(object sender, EdgeSelectedEventArgs args)
        {
            if (args.MouseArgs.LeftButton == MouseButtonState.Pressed && _opMode == EditorOperationMode.Delete)
                graphArea.RemoveEdge(args.EdgeControl.Edge as DataEdge, true);
        }

        void graphArea_VertexSelected(object sender, VertexSelectedEventArgs args)
        {
             if(args.MouseArgs.LeftButton == MouseButtonState.Pressed)            
             {
                 switch (_opMode)
                 {
                     case EditorOperationMode.Edit:
                         CreateEdgeControl(args.VertexControl);
                         break;
                     case EditorOperationMode.Delete:
                         SafeRemoveVertex(args.VertexControl);
                         break;
                     default:
                         if (_opMode == EditorOperationMode.Select && args.Modifiers == ModifierKeys.Control)
                             SelectVertex(args.VertexControl);
                         break;
                 }
             }
        }

        private static void SelectVertex(DependencyObject vc)
        {
            if (DragBehaviour.GetIsTagged(vc))
            {
                HighlightBehaviour.SetHighlighted(vc, false);
                DragBehaviour.SetIsTagged(vc, false);
            }
            else
            {
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
            if(butDelete.IsChecked == true && ReferenceEquals(sender, butDelete))
            {
                butEdit.IsChecked = false;
                butSelect.IsChecked = false;
                zoomCtrl.Cursor = Cursors.Help;
                _opMode = EditorOperationMode.Delete;
                ClearEditMode();
                ClearSelectMode();
                return;
            }
            if (butEdit.IsChecked == true && ReferenceEquals(sender, butEdit))
            {
                butDelete.IsChecked = false;
                butSelect.IsChecked = false;
                zoomCtrl.Cursor = Cursors.Pen;
                _opMode = EditorOperationMode.Edit;
                ClearSelectMode();
                return;
            }
            if (butSelect.IsChecked == true && ReferenceEquals(sender, butSelect))
            {
                butEdit.IsChecked = false;
                butDelete.IsChecked = false;
                zoomCtrl.Cursor = Cursors.Hand;
                _opMode = EditorOperationMode.Select;
                ClearEditMode();
                graphArea.SetVerticesDrag(true, true);
                graphArea.SetEdgesDrag(true);
                return;
            }
        }

        private void ClearSelectMode(bool soft = false)
        {
            graphArea.VertexList.Values
                .Where(DragBehaviour.GetIsTagged)
                .ToList()
                .ForEach(a =>
                {
                    HighlightBehaviour.SetHighlighted(a, false);
                    DragBehaviour.SetIsTagged(a, false);
                });

            if (!soft)
                graphArea.SetVerticesDrag(false);
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
            var vc = new VertexControl(data);
            vc.SetPosition(position);
            graphArea.AddVertexAndData(data, vc, true);
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
            var ec = new EdgeControl(_ecFrom, vc, data);
            graphArea.InsertEdgeAndData(data, ec, 0, true);

            HighlightBehaviour.SetHighlighted(_ecFrom, false);
            _ecFrom = null;
            _editorManager.DestroyVirtualEdge();
        }

        private void SafeRemoveVertex(VertexControl vc)
        {
            //remove vertex and all adjacent edges from layout and data graph
            graphArea.RemoveVertexAndEdges(vc.Vertex as DataVertex);
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
