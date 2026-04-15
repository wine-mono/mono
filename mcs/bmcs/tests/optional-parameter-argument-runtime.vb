' EXPECT: 5
Imports System

Module Test
    Sub F(Optional ByVal x As Integer = 10)
        Console.WriteLine(x)
    End Sub

    Sub Main()
        F(5)
    End Sub
End Module
