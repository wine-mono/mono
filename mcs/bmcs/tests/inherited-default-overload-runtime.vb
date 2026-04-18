' EXPECT: ok
Imports System
Imports System.Collections.Generic

Public Class Box
    Inherits List(Of String)

    Default Overloads ReadOnly Property Item(ByVal name As String) As String
        Get
            Return "name"
        End Get
    End Property
End Class

Module Test
    Sub Main()
        Dim b As New Box()
        b.Add("ok")
        Console.WriteLine(b(0))
    End Sub
End Module
