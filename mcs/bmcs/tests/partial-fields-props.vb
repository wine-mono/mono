' EXPECT: 42 hello
' VB spec: fields, properties across partial parts.
Imports System

Partial Class Foo
    Private _val As Integer = 42
    Public ReadOnly Property Val() As Integer
        Get
            Return _val
        End Get
    End Property
End Class

Partial Class Foo
    Private _name As String = "hello"
    Public ReadOnly Property Name() As String
        Get
            Return _name
        End Get
    End Property
End Class

Module Test
    Sub Main()
        Dim f As New Foo()
        Console.Write(f.Val)
        Console.Write(" ")
        Console.WriteLine(f.Name)
    End Sub
End Module
