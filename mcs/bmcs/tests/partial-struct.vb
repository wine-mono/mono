' EXPECT: 3 7
' VB spec: structures can be partial.
Imports System

Partial Structure Point
    Public X As Integer
End Structure

Partial Structure Point
    Public Y As Integer
End Structure

Module Test
    Sub Main()
        Dim p As Point
        p.X = 3
        p.Y = 7
        Console.Write(p.X)
        Console.Write(" ")
        Console.WriteLine(p.Y)
    End Sub
End Module
