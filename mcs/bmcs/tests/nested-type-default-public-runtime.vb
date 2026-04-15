' EXPECT: 3
' EXPECT: 0
' EXPECT: 7
Imports System

Public Class Outer
    Class Inner
        Public Shared Function Value() As Integer
            Return 3
        End Function
    End Class

    Enum Choice
        Zero
    End Enum

    Delegate Function Transformer(ByVal value As Integer) As Integer
End Class

Module Test
    Function Increment(ByVal value As Integer) As Integer
        Return value + 1
    End Function

    Sub Main()
        Dim fn As New Outer.Transformer(AddressOf Increment)
        Console.WriteLine(Outer.Inner.Value())
        Console.WriteLine(CInt(Outer.Choice.Zero))
        Console.WriteLine(fn(6))
    End Sub
End Module
