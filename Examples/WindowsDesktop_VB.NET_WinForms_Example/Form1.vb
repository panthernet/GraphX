Imports System.Windows
Imports WindowsDesktop_VB.NET_WinForms_Example.Models
Imports GraphX.PCL.Common.Enums
Imports GraphX.PCL.Logic.Algorithms.LayoutAlgorithms
Imports GraphX.PCL.Logic.Algorithms.OverlapRemoval
Imports GraphX.PCL.Logic.Models
Imports GraphX.Controls
Imports QuickGraph

Public Class Form1

    Dim _zoomctrl As ZoomControl
    Dim _gArea As GraphAreaExample

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        wpfHost.Child = GenerateWpfVisuals()

        _gArea.GenerateGraph(True)

        _zoomctrl.ZoomToFill()
    End Sub

    Private Function GenerateWpfVisuals() As UIElement
        _zoomctrl = New ZoomControl()
        ZoomControl.SetViewFinderVisibility(_zoomctrl, Visibility.Visible)
        ' ENABLES WINFORMS HOSTING MODE --- >

        Dim logic = New GXLogicCore(Of DataVertex, DataEdge, BidirectionalGraph(Of DataVertex, DataEdge))()
        _gArea = New GraphAreaExample()
        _gArea.EnableWinFormsHostingMode = True
        _gArea.LogicCore = logic

        logic.Graph = GenerateGraph()
        logic.DefaultLayoutAlgorithm = LayoutAlgorithmTypeEnum.KK
        logic.DefaultLayoutAlgorithmParams = logic.AlgorithmFactory.CreateLayoutParameters(LayoutAlgorithmTypeEnum.KK)
        DirectCast(logic.DefaultLayoutAlgorithmParams, KKLayoutParameters).MaxIterations = 100
        logic.DefaultOverlapRemovalAlgorithm = OverlapRemovalAlgorithmTypeEnum.FSA
        logic.DefaultOverlapRemovalAlgorithmParams = logic.AlgorithmFactory.CreateOverlapRemovalParameters(OverlapRemovalAlgorithmTypeEnum.FSA)
        DirectCast(logic.DefaultOverlapRemovalAlgorithmParams, OverlapRemovalParameters).HorizontalGap = 50
        DirectCast(logic.DefaultOverlapRemovalAlgorithmParams, OverlapRemovalParameters).VerticalGap = 50
        logic.DefaultEdgeRoutingAlgorithm = EdgeRoutingAlgorithmTypeEnum.None
        logic.AsyncAlgorithmCompute = False
        _zoomctrl.Content = _gArea
        Dim myResourceDictionary = New ResourceDictionary()
        myResourceDictionary.Source = New Uri("Templates\template.xaml", UriKind.Relative)
        _zoomctrl.Resources.MergedDictionaries.Add(myResourceDictionary)

        Return _zoomctrl
    End Function

    Private Function GenerateGraph() As BidirectionalGraph(Of DataVertex, DataEdge)
        'FOR DETAILED EXPLANATION please see SimpleGraph example project
        Dim dataGraph = New GraphExample()
        For i As Integer = 1 To 9
            Dim dataVertex = New DataVertex("MyVertex " & i)
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
