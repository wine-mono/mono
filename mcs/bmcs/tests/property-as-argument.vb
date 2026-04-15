' EXPECT: e
Imports System
Module Test
    Sub TakeChar(c As Char)
        Console.WriteLine(c)
    End Sub
    Sub Main()
        Dim s As String = "Hello"
        TakeChar(s.Chars(1))
    End Sub
End Module
