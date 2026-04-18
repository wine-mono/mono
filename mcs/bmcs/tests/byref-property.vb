' EXPECT: 11
Imports System

Module Test
    Class Box
        Private _value As Integer

        Public Property Value() As Integer
            Get
                Return _value
            End Get
            Set(ByVal value As Integer)
                _value = value
            End Set
        End Property
    End Class

    Sub Bump(ByRef value As Integer)
        value = value + 1
    End Sub

    Sub Main()
        Dim box As New Box()
        box.Value = 10
        Bump(box.Value)
        Console.WriteLine(box.Value)
    End Sub
End Module
