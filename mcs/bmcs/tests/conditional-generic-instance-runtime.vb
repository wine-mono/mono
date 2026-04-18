' EXPECT: 1

Imports System.Diagnostics

Public Class Box(Of T)
    <Conditional("DEBUG")> _
    Public Sub TraceHit()
        System.Console.Write("2")
    End Sub
End Class

Module Test
    Sub Main()
        Dim box As New Box(Of Integer)()
        System.Console.Write("1")
        box.TraceHit()
    End Sub
End Module
