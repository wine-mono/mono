' EXPECT: 1
' EXPECT: 2

Public Interface IFoo
    ReadOnly Property P() As Integer
End Interface

Public Class C
    Implements IFoo

    Public ReadOnly Property P() As Integer
        Get
            Return 1
        End Get
    End Property

    Private ReadOnly Property P2() As Integer Implements IFoo.P
        Get
            Return 2
        End Get
    End Property
End Class

Module M
    Sub Main()
        Dim c As New C()
        Dim f As IFoo = c
        System.Console.WriteLine(c.P)
        System.Console.WriteLine(f.P)
    End Sub
End Module
