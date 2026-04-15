' EXPECT: 2
Imports System
Imports system.collections.generic

Module Test
    Sub Main()
        Dim values As New list(Of Integer)()
        values.Add(1)
        values.Add(2)
        Console.WriteLine(values.Count)
    End Sub
End Module
