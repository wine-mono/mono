' EXPECT: 5
Class Sample
    Private seed As Integer = 3
    Private value As Integer = Offset

    Private ReadOnly Property Offset() As Integer
        Get
            Return seed + 2
        End Get
    End Property

    Public Function Read() As Integer
        Return value
    End Function
End Class

Module Test
    Sub Main()
        System.Console.WriteLine((New Sample()).Read())
    End Sub
End Module
