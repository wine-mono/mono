' EXPECT: ok
Imports System
Imports System.Collections

Module Test
    Sub Main()
        ' Partial namespace: Generic resolves to System.Collections.Generic
        ' via the Imports System.Collections directive.
        Dim list As New Generic.List(Of Integer)
        list.Add(1)
        Console.WriteLine("ok")
    End Sub
End Module
