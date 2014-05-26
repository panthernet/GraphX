Imports System.Windows
Imports GraphX.GraphSharp.Algorithms.Layout.Simple.FDP
Imports GraphX.Controls
Imports GraphX.Logic
Imports GraphX.GraphSharp.Algorithms.OverlapRemoval
Imports WindowsDesktop_VB.NET_WinForms_Example.Models
Imports QuickGraph

Public Class Form1

    Dim zoomctrl As ZoomControl
    Dim gArea As GraphAreaExample

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        wpfHost.Child = GenerateWpfVisuals()

        gArea.GenerateGraph(True)

        zoomctrl.ZoomToFill()
    End Sub

    Private Function GenerateWpfVisuals() As UIElement
        zoomctrl = New ZoomControl()
        ZoomControl.SetViewFinderVisibility(zoomctrl, System.Windows.Visibility.Visible)
        ' ENABLES WINFORMS HOSTING MODE --- >

        Dim logic = New GXLogicCore(Of DataVertex, DataEdge, BidirectionalGraph(Of DataVertex, DataEdge))()
        gArea = New GraphAreaExample()
        gArea.EnableWinFormsHostingMode = True
        gArea.LogicCore = logic

        logic.Graph = GenerateGraph()
        logic.DefaultLayoutAlgorithm = GraphX.LayoutAlgorithmTypeEnum.KK
        logic.DefaultLayoutAlgorithmParams = logic.AlgorithmFactory.CreateLayoutParameters(GraphX.LayoutAlgorithmTypeEnum.KK)
        DirectCast(logic.DefaultLayoutAlgorithmParams, KKLayoutParameters).MaxIterations = 100
        logic.DefaultOverlapRemovalAlgorithm = GraphX.OverlapRemovalAlgorithmTypeEnum.FSA
        logic.DefaultOverlapRemovalAlgorithmParams = logic.AlgorithmFactory.CreateOverlapRemovalParameters(GraphX.OverlapRemovalAlgorithmTypeEnum.FSA)
        DirectCast(logic.DefaultOverlapRemovalAlgorithmParams, OverlapRemovalParameters).HorizontalGap = 50
        DirectCast(logic.DefaultOverlapRemovalAlgorithmParams, OverlapRemovalParameters).VerticalGap = 50
        logic.DefaultEdgeRoutingAlgorithm = GraphX.EdgeRoutingAlgorithmTypeEnum.None
        logic.AsyncAlgorithmCompute = False
        zoomctrl.Content = gArea
        Dim myResourceDictionary = New ResourceDictionary()
        myResourceDictionary.Source = New Uri("Templates\template.xaml", UriKind.Relative)
        zoomctrl.Resources.MergedDictionaries.Add(myResourceDictionary)

        Return zoomctrl
    End Function

    Private Function GenerateGraph() As BidirectionalGraph(Of DataVertex, DataEdge)
        'FOR DETAILED EXPLANATION please see SimpleGraph example project
        Dim dataGraph = New GraphExample()
        For i As Integer = 1 To 9
            Dim dataVertex = New DataVertex("MyVertex " & i)
            dataVertex.ID = i
            dataGraph.AddVertex(dataVertex)
        Next
        Dim vlist = dataGraph.Vertices.ToList()
        'Then create two edges optionaly defining Text property to show who are connected
        Dim dataEdge = New DataEdge(vlist(0), vlist(1))
        dataEdge.Text = String.Format("{0} -> {1}", vlist(0), vlist(1))
        dataGraph.AddEdge(dataEdge)
        dataEdge = New DataEdge(vlist(2), vlist(3))
        dataEdge.Text = String.Format("{0} -> {1}", vlist(2), vlist(3))
        dataGraph.AddEdge(dataEdge)

        dataEdge = New DataEdge(vlist(2), vlist(2))
        dataEdge.Text = String.Format("{0} -> {1}", vlist(2), vlist(2))
        dataGraph.AddEdge(dataEdge)

        Return dataGraph
    End Function
End Class
