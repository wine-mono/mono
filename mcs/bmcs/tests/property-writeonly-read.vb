' EXPECT-COMPILE-CONTAINS: lacks a get accessor
Imports System
Class Obj
    Private _val As Integer
    WriteOnly Property Bar() As Integer
        Set(value As Integer)
            _val = value
        End Set
    End Property
End Class
Module Test
    Sub Main()
        Dim o As New Obj()
        Dim x As Integer = o.Bar
    End Sub
End Module
