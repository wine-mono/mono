Imports System

Class Box
    Public ReadOnly Property Value As Integer
        Get
            Return 7
        End Get
    End Property
End Class

Class Program
    Private Shared Function MakeBox(Of T)() As Box
        Return New Box()
    End Function

    Shared Sub Main()
        Console.WriteLine(MakeBox(Of Integer).Value)
    End Sub
End Class
