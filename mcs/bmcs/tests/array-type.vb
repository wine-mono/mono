' EXPECT: 0
' Tests that 'As SomeType()' array type annotations parse on
' fields, parameters, and return types.  Doesn't exercise array
' creation expressions ('New String() {...}')
Imports System

Public Class Holder
    Public Names As String()
End Class

Module Test
    Sub Main()
        Dim h As New Holder
        Dim n As Integer
        If h.Names Is Nothing Then
            n = 0
        Else
            n = h.Names.Length
        End If
        Console.WriteLine(n)
    End Sub
End Module
