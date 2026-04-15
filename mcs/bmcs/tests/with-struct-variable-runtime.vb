' EXPECT: 10
Imports System

Structure Counter
    Public Value As Integer
End Structure

Module Test
    Sub Main()
        Dim c As Counter = New Counter()

        With c
            .Value = 10
        End With

        Console.WriteLine(c.Value)
    End Sub
End Module
