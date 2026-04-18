' EXPECT: 77
Imports System
Imports System.Collections.Generic
Class MyDict
    Inherits Dictionary(Of String, Integer)
    Sub SetBase(key As String, val As Integer)
        MyBase.Item(key) = val
    End Sub
End Class
Module Test
    Sub Main()
        Dim d As New MyDict()
        d.Add("x", 1)
        d.SetBase("x", 77)
        Console.WriteLine(d.Item("x"))
    End Sub
End Module
