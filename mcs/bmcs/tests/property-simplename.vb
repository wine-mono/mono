' EXPECT: 42
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
    Public Function GetIt() As Integer
        Return Val
    End Function
End Class
Module Test
    Sub Main()
        Dim o As New Obj()
        Console.WriteLine(o.GetIt())
    End Sub
End Module
