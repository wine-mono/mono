' EXPECT: int-target
Imports System

Module Test
    Sub Main()
        GoTo 100
        Console.WriteLine("missed")
100:
        Console.WriteLine("int-target")
    End Sub
End Module
