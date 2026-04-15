' EXPECT: ok
' VB spec: only one declaration needs Partial, regardless of order.
Imports System

Public Class Foo
    Public Sub A()
    End Sub
End Class

Partial Public Class Foo
    Public Sub B()
        Console.WriteLine("ok")
    End Sub
End Class

Module Test
    Sub Main()
        Dim f As New Foo()
        f.B()
    End Sub
End Module
