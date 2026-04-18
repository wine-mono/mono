' EXPECT: ok
Imports System
Imports System.Collections.Generic

Public Class Box
    Inherits List(Of String)

    Default Shadows ReadOnly Property Item(ByVal index As Integer) As String
        Get
            Return "ok"
        End Get
    End Property
End Class

Module Test
    Sub Main()
        Dim b As New Box()
        Console.WriteLine(b(0))
    End Sub
End Module
