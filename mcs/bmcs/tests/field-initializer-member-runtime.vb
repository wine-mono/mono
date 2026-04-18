' EXPECT: 5
Class Sample
    Private seed As Integer = 3
    Private value As Integer = GetSeed()

    Private Function GetSeed() As Integer
        Return seed + 2
    End Function

    Public Function Read() As Integer
        Return value
    End Function
End Class

Module Test
    Sub Main()
        System.Console.WriteLine((New Sample()).Read())
    End Sub
End Module
