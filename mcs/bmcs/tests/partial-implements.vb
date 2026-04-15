' EXPECT: disposed
' VB spec: Implements on one part, implementation on another.
Imports System

Partial Class Foo
    Implements IDisposable
End Class

Partial Class Foo
    Public Sub Dispose() Implements IDisposable.Dispose
        Console.WriteLine("disposed")
    End Sub
End Class

Module Test
    Sub Main()
        Dim f As New Foo()
        f.Dispose()
    End Sub
End Module
