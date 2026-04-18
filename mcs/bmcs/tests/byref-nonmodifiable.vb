' EXPECT: arg=7
' EXPECT: x=5
Imports System

Module Test
    Sub Bump(ByRef value As Integer)
        value = value + 1
        Console.WriteLine("arg=" & value)
    End Sub

    Sub Main()
        Dim x As Integer = 5
        Bump(x + 1)
        Console.WriteLine("x=" & x)
    End Sub
End Module
