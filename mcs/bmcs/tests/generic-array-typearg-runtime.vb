' EXPECT: 2
Imports System
Imports System.Collections.Generic

Module Test
    Sub Main()
        Dim value As New Dictionary(Of String, Integer())()
        value("a") = New Integer() {1, 2}
        Console.WriteLine(value("a")(1))
    End Sub
End Module
