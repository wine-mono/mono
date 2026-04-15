' EXPECT: ok
Imports System

Public Class SpanLike
    ReadOnly Property AsString() As String
        Get
            Return "ok"
        End Get
    End Property

    ReadOnly Property AsString(ByVal compiler As Object) As String
        Get
            Return "ok"
        End Get
    End Property
End Class

Module Test
    Sub Main()
        Dim s As New SpanLike()
        Console.WriteLine(s.AsString(Nothing))
    End Sub
End Module
