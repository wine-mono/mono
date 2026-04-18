' EXPECT: hello
' Tests that a single Partial declaration still behaves like an
' ordinary class declaration.
Imports System

Partial Public Class Foo
    Public Sub Greet()
        Console.WriteLine("hello")
    End Sub
End Class

Module Test
    Sub Main()
        Dim f As New Foo
        f.Greet()
    End Sub
End Module
