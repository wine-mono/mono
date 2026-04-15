' EXPECT: world
Imports System
Class Obj
    Private _name As String = "hello"
    Public Property Name() As String
        Get
            Return _name
        End Get
        Set(value As String)
            _name = value
        End Set
    End Property
End Class
Module Test
    Sub Main()
        Dim o As New Obj()
        o.Name = "world"
        Console.WriteLine(o.Name)
    End Sub
End Module
