' EXPECT: x=2 y=1
Imports System

Module Test
    Sub Swap(ByRef a As Integer, ByRef b As Integer)
        Dim tmp As Integer
        tmp = a
        a = b
        b = tmp
    End Sub

    Sub Main()
        Dim x As Integer = 1
        Dim y As Integer = 2
        Swap(x, y)
        Console.WriteLine("x=" & x & " y=" & y)
    End Sub
End Module
