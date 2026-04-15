' EXPECT: 99
Imports System
Class Obj
    Private _val As Integer = 42
    Public Property Val() As Integer
        Get
            Return _val
        End Get
        Set(value As Integer)
            _val = value
        End Set
    End Property
    Public Sub SetIt(v As Integer)
        Val = v
    End Sub
End Class
Module Test
    Sub Main()
        Dim o As New Obj()
        o.SetIt(99)
        Console.WriteLine(o.Val)
    End Sub
End Module
