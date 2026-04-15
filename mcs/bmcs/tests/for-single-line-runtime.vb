Imports System

Module Test
    Private Function IndexOfValue(ByVal values As Integer(), ByVal item As Integer) As Integer
        Dim i As Integer : For i = 0 To values.Length - 1 : If values(i) = item Then Return i : Next : Return -1
    End Function

    Sub Main()
        Console.WriteLine(IndexOfValue(New Integer() {1, 4, 9}, 4))
        Console.WriteLine(IndexOfValue(New Integer() {1, 4, 9}, 7))
    End Sub
End Module
