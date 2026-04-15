' EXPECT: hello from A
' EXPECT: hello from B
Imports System

Partial Public Class Foo
    Public Sub GreetA()
        Console.WriteLine("hello from A")
    End Sub
End Class

Partial Public Class Foo
    Public Sub GreetB()
        Console.WriteLine("hello from B")
    End Sub
End Class

Module Test
    Sub Main()
        Dim f As New Foo
        f.GreetA()
        f.GreetB()
    End Sub
End Module
