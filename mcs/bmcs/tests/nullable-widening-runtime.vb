' EXPECT: 5
Imports System

Module Test
    Sub Main()
        Dim x As Integer? = 5
        Console.WriteLine(x.Value)
    End Sub
End Module
