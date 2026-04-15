' EXPECT: x=hello y=world
Imports System

Module Test
    Sub Swap(Of T)(ByRef a As T, ByRef b As T)
        Dim tmp As T
        tmp = a
        a = b
        b = tmp
    End Sub

    Sub Main()
        Dim x As String = "world"
        Dim y As String = "hello"
        Swap(Of String)(x, y)
        Console.WriteLine("x=" & x & " y=" & y)
    End Sub
End Module
