' EXPECT: ok
Imports System

Public Structure SpanLike
    ReadOnly Property AsString() As String
        Get
            Return "ok"
        End Get
    End Property
End Structure

Module Test
    Sub Main()
        Dim s As New SpanLike()
        Console.WriteLine(s.AsString)
    End Sub
End Module
