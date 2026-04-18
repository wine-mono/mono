' EXPECT: 99
Imports System
Imports System.Collections.Generic

Class MyDict
    Inherits Dictionary(Of String, Integer)

    Public Function GetBase(key As String) As Integer
        Return MyBase.Item(key)
    End Function
End Class

Module Test
    Sub Main()
        Dim d As New MyDict()
        d.Add("x", 99)
        Console.WriteLine(d.GetBase("x"))
    End Sub
End Module
