' EXPECT-COMPILE-CONTAINS: does not contain a definition for `Chars'
Imports System
Module Test
    Sub Main()
        Dim s As String = "Hello"
        Dim c As Char = s.Chars
    End Sub
End Module
