' PREPROCESS: DEBUG
' EXPECT: ok

Public Interface IBaseObject
End Interface

Public Interface INameable
    Inherits IBaseObject
    ReadOnly Property Name() As String
End Interface

Public Class InternalException
    Sub New(ByVal obj As IBaseObject)
    End Sub
End Class

Public Class Nameables(Of T As INameable)
    Function Build(ByVal value As T) As String
        Dim ex As New InternalException(value)
        Return "ok"
    End Function
End Class

Public Class Item
    Implements INameable

    Public ReadOnly Property Name() As String Implements INameable.Name
        Get
            Return "x"
        End Get
    End Property
End Class

Module Test
    Sub Main()
        Dim n As New Nameables(Of Item)()
        System.Console.WriteLine(n.Build(New Item()))
    End Sub
End Module
