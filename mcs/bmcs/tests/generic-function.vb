' EXPECT: 42
Imports System

Module Test
    Function Box(Of T)(x As T) As T
        Return x
    End Function

    Sub Main()
        Dim n As Integer = Box(Of Integer)(42)
        Console.WriteLine(n)
    End Sub
End Module
