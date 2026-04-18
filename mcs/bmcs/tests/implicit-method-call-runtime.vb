' EXPECT: 7
Imports System

Module Test
    Function F() As Integer
        Return 7
    End Function

    Sub Main()
        Console.WriteLine(F)
    End Sub
End Module
