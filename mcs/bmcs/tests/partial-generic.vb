' EXPECT: 42
' VB spec: partial generic declarations must agree on arity and type parameter names.
Imports System

Partial Public Class Box(Of T)
    Public Value As T
End Class

Partial Public Class Box(Of T)
    Public Function GetValue() As T
        Return Value
    End Function
End Class

Module Test
    Sub Main()
        Dim b As New Box(Of Integer)
        b.Value = 42
        Console.WriteLine(b.GetValue())
    End Sub
End Module
