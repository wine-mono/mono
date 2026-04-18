' EXPECT: 11
Imports System

Module Test
    Sub Bump(ByRef value As Integer)
        value = value + 1
    End Sub

    Sub Main()
        Dim x As Byte = 10
        Bump(x)
        Console.WriteLine(x)
    End Sub
End Module
