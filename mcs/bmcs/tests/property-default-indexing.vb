' EXPECT: 1
Imports System
Imports System.Collections.Generic
Module Test
    Sub Main()
        Dim d As New Dictionary(Of String, Integer)
        d.Add("a", 1)
        Console.WriteLine(d("a"))
    End Sub
End Module
