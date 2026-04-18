' EXPECT: ok
' Tests that 'T?' nullable type syntax parses in field and
' variable declarations.
Imports System

Public Class Holder
    Public maybe As Integer?
End Class

Module Test
    Sub Main()
        Dim h As New Holder
        Dim x As Integer? = h.maybe
        If x.HasValue Then
            Console.WriteLine("has value")
        Else
            Console.WriteLine("ok")
        End If
    End Sub
End Module
