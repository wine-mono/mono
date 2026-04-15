' EXPECT: 7
Imports System

Class Outer(Of T)
    Protected Class Inner
        Public Shared Function Value() As Integer
            Return 7
        End Function
    End Class
End Class

Class Derived
    Inherits Outer(Of Integer)

    Shared Sub Main()
        Console.WriteLine(Inner.Value())
    End Sub
End Class
