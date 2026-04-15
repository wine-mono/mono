' EXPECT: 0
' EXPECT: 1
' EXPECT: 2
' EXPECT: stopped
' EXPECT: 5
' EXPECT: 10
Imports System

Module Test
    Sub Work(x As Integer)
        If x < 0 Then
            Exit Sub
        End If
        Console.WriteLine(x)
    End Sub

    Sub Main()
        For i As Integer = 0 To 10
            If i = 3 Then
                Exit For
            End If
            Console.WriteLine(i)
        Next
        Console.WriteLine("stopped")

        Work(5)
        Work(-1)
        Work(10)
    End Sub
End Module
