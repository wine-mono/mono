' EXPECT: reached
Imports System

Module Test
    Sub Main()
        GoTo done
        Console.WriteLine("missed")
Done:
        Console.WriteLine("reached")
    End Sub
End Module
