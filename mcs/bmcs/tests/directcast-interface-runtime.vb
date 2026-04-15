' EXPECT: bark
Imports System

Interface IBark
    Sub Bark()
End Interface

Class Dog
    Implements IBark

    Public Sub Bark() Implements IBark.Bark
        Console.WriteLine("bark")
    End Sub
End Class

Module Test
    Sub Main()
        Dim o As Object = New Dog()
        Dim b As IBark = DirectCast(o, IBark)
        b.Bark()
    End Sub
End Module
