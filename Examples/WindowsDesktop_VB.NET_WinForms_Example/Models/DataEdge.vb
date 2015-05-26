
Imports GraphX.PCL.Common.Models

Namespace Models

    Public Class DataEdge
        Inherits EdgeBase(Of DataVertex)
        ''' <summary>
        ''' Default constructor. We need to set at least Source and Target properties of the edge.
        ''' </summary>
        ''' <param name="source">Source vertex data</param>
        ''' <param name="target">Target vertex data</param>
        ''' <param name="weight">Optional edge weight</param>
        Public Sub New(source As DataVertex, target As DataVertex, Optional weight As Double = 1)
            MyBase.New(source, target, weight)
        End Sub
        ''' <summary>
        ''' Default parameterless constructor (for serialization compatibility)
        ''' </summary>
        Public Sub New()
            MyBase.New(Nothing, Nothing, 1)
        End Sub

        ''' <summary>
        ''' Custom string property for example
        ''' </summary>
        Public Property Text As String

#Region "GET members"
        Public Overloads Function ToString() As String
            Return Text
        End Function


#End Region
    End Class
End Namespace