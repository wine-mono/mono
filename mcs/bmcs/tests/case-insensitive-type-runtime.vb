' EXPECT: 7
Imports System

Class Sample
    Public Function Value() As Integer
        Return 7
    End Function
End Class

Module Test
    Sub Main()
        Dim instance As sample = New sample()
        Console.WriteLine(instance.Value())
    End Sub
End Module
