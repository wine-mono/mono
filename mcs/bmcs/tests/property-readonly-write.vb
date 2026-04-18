' EXPECT-COMPILE-CONTAINS: lacks a set accessor
Imports System
Class Obj
    ReadOnly Property Bar() As Integer
        Get
            Return 42
        End Get
    End Property
End Class
Module Test
    Sub Main()
        Dim o As New Obj()
        o.Bar = 10
    End Sub
End Module
