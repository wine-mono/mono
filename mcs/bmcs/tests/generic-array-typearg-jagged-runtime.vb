' EXPECT: 3
Imports System
Imports System.Collections.Generic

Module Test
    Sub Main()
        Dim value As New List(Of Integer()())()
        value.Add(New Integer()() {New Integer() {1}, New Integer() {2, 3}})
        Console.WriteLine(value(0)(1)(1))
    End Sub
End Module
