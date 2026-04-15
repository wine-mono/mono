' EXPECT: 2
' EXPECT: A
Imports System
Imports Microsoft.VisualBasic

Module Test
    Sub Main()
        Dim code As Integer = 65
        Console.WriteLine(vbNewLine.Length)
        Console.WriteLine(ChrW(code))
    End Sub
End Module
