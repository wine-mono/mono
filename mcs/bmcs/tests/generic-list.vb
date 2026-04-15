' EXPECT: count=2
Imports System
Imports System.Collections.Generic

Module Test
    Sub Main()
        Dim list As New List(Of Integer)
        list.Add(42)
        list.Add(17)
        Console.WriteLine("count=" & list.Count)
    End Sub
End Module
