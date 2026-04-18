' EXPECT: world
Imports System
Class Base
    Private _name As String = "hello"
    Public Overridable Property Name() As String
        Get
            Return _name
        End Get
        Set(value As String)
            _name = value
        End Set
    End Property
End Class
Class Derived
    Inherits Base
    Public Sub SetBase(v As String)
        MyBase.Name = v
    End Sub
End Class
Module Test
    Sub Main()
        Dim d As New Derived()
        d.SetBase("world")
        Console.WriteLine(d.Name)
    End Sub
End Module
