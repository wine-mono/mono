' EXPECT: make 1
' EXPECT: 8
Imports System

Class Box
    Public Value As Integer
End Class

Module Test
    Private Calls As Integer = 0
    Private SharedBox As New Box()

    Private Function MakeBox() As Box
        Calls += 1
        Console.WriteLine("make " & Calls)
        Return SharedBox
    End Function

    Sub Main()
        SharedBox.Value = 7

        With MakeBox()
            .Value = .Value + 1
        End With

        Console.WriteLine(SharedBox.Value)
    End Sub
End Module
