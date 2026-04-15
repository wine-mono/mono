' EXPECT: 2
' EXPECT: 1
' EXPECT: 2
Imports System
Imports System.Collections.Generic
Imports System.Collections.ObjectModel

Module Test
    Sub Main()
        Dim values() As Integer = New Integer() {1, 2}
        Dim asList As IList(Of Integer) = values
        Dim boxed As New ReadOnlyCollection(Of Integer)(values)

        Console.WriteLine(asList.Count)
        Console.WriteLine(boxed(0))
        Console.WriteLine(boxed(1))
    End Sub
End Module
