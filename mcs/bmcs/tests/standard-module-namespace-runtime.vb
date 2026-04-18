' EXPECT: 2
' EXPECT: A
Imports System

Module Test
    Sub Main()
        Dim code As Integer = 65
        Console.WriteLine(Microsoft.VisualBasic.vbNewLine.Length)
        Console.WriteLine(Microsoft.VisualBasic.ChrW(code))
    End Sub
End Module
