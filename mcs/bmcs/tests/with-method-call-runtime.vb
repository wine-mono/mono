' EXPECT: 1
' EXPECT: 11
Imports System

Class Box
    Public Value As Integer

    Public Sub SetValue(v As Integer)
        Value = v
    End Sub
End Class

Module Test
    Private Calls As Integer = 0
    Private SharedBox As New Box()

    Private Function MakeBox() As Box
        Calls += 1
        Return SharedBox
    End Function

    Sub Main()
        With MakeBox()
            .SetValue(11)
        End With

        Console.WriteLine(Calls)
        Console.WriteLine(SharedBox.Value)
    End Sub
End Module
