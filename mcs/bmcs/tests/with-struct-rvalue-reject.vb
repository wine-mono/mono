' EXPECT-COMPILE-CONTAINS: Cannot modify expression because it is not a variable.
Structure Counter
    Public Value As Integer
End Structure

Module Test
    Private Function MakeCounter() As Counter
        Dim c As Counter = New Counter()
        Return c
    End Function

    Sub Main()
        With MakeCounter()
            .Value = 10
        End With
    End Sub
End Module
