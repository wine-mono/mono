' EXPECT: hi
Imports System

Public Class Foo
    Public ReadOnly Property Name() As String
        Get
            Return "hi"
        End Get
    End Property
End Class

Module Test
    Sub Main()
        Dim f As New Foo
        Console.WriteLine(f.Name)
    End Sub
End Module
