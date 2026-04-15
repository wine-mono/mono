' EXPECT: val=42
' VB spec: members from one part can reference another part.
Imports System

Partial Class Foo
    Private _val As Integer = 42
End Class

Partial Class Foo
    Public Sub Show()
        Console.WriteLine("val=" & _val.ToString())
    End Sub
End Class

Module Test
    Sub Main()
        Dim f As New Foo()
        f.Show()
    End Sub
End Module
