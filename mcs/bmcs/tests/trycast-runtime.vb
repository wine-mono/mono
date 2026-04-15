' EXPECT: hit Dog
' EXPECT: miss
' EXPECT: miss Nothing
' EXPECT: hit IBark
Imports System

' VB TryCast must return Nothing on a failed reference cast
' instead of throwing InvalidCastException.

Interface IBark
    Sub Bark()
End Interface

Class Animal
End Class

Class Dog
    Inherits Animal
    Implements IBark
    Public Sub Bark() Implements IBark.Bark
    End Sub
End Class

Class Cat
    Inherits Animal
End Class

Module Test
    Sub Main()
        Dim a As Animal = New Dog()
        Dim d As Dog = TryCast(a, Dog)
        If d IsNot Nothing Then
            Console.WriteLine("hit Dog")
        Else
            Console.WriteLine("miss")
        End If

        Dim a2 As Animal = New Cat()
        Dim d2 As Dog = TryCast(a2, Dog)
        If d2 IsNot Nothing Then
            Console.WriteLine("hit Dog 2")
        Else
            Console.WriteLine("miss")
        End If

        ' TryCast on a Nothing reference must not throw.
        Dim a3 As Animal = Nothing
        Dim d3 As Dog = TryCast(a3, Dog)
        If d3 IsNot Nothing Then
            Console.WriteLine("hit Dog 3")
        Else
            Console.WriteLine("miss Nothing")
        End If

        ' Cast to an interface the runtime type implements.
        Dim a4 As Animal = New Dog()
        Dim b As IBark = TryCast(a4, IBark)
        If b IsNot Nothing Then
            Console.WriteLine("hit IBark")
        Else
            Console.WriteLine("miss IBark")
        End If
    End Sub
End Module
