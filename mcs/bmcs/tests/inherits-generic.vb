' EXPECT: 1
Imports System
Imports System.Collections

Public Class TypeList
    Inherits Generic.List(Of Type)
End Class

Module Test
    Sub Main()
        Dim tl As New TypeList
        tl.Add(GetType(Integer))
        Console.WriteLine(tl.Count)
    End Sub
End Module
