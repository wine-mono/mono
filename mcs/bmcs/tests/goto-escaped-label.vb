' EXPECT: escaped
Imports System

Module Test
    Sub Main()
        GoTo [tail]
        Console.WriteLine("missed")
[tail]:
        Console.WriteLine("escaped")
    End Sub
End Module
