' EXPECT: tail
Imports System

Module Test
    Sub Main()
        GoTo tail
        Console.WriteLine("missed")
        Console.WriteLine("before") : tail: Console.WriteLine("tail")
    End Sub
End Module
