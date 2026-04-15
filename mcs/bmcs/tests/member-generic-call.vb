' EXPECT: 42
' EXPECT: hello
Imports System

Public Class Bag
    Public Value1 As Object = 42
    Public Value2 As Object = "hello"

    Public Function GetValue(Of T)(which As Integer) As T
        If which = 1 Then
            Return DirectCast(Value1, T)
        Else
            Return DirectCast(Value2, T)
        End If
    End Function
End Class

Module Test
    Sub Main()
        Dim b As New Bag
        ' Generic method call via member access: bag.GetValue(Of T)(i)
        Console.WriteLine(b.GetValue(Of Integer)(1))
        Console.WriteLine(b.GetValue(Of String)(2))
    End Sub
End Module
