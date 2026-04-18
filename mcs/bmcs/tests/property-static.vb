' EXPECT: 42
Imports System
Class Foo
    Private Shared _val As Integer = 42
    Public Shared ReadOnly Property Val() As Integer
        Get
            Return _val
        End Get
    End Property
End Class
Module Test
    Sub Main()
        Console.WriteLine(Foo.Val)
    End Sub
End Module
