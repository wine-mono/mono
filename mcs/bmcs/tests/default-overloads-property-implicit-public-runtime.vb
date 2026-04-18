' EXPECT: ok
Imports System

Public Class Box
    Default Overloads ReadOnly Property Item(ByVal name As String) As String
        Get
            Return "ok"
        End Get
    End Property

    Default Overloads ReadOnly Property Item(ByVal parent As String, ByVal child As String) As String
        Get
            Return "ok"
        End Get
    End Property
End Class

Module Test
    Sub Main()
        Dim b As New Box()
        Console.WriteLine(b("x"))
    End Sub
End Module
