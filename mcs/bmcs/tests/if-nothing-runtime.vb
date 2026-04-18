' EXPECT: x
' EXPECT: True
Imports System

Module Test
    Sub Main()
        Dim s As String = If(Nothing, "x")
        Dim o As Object = If(Nothing, Nothing)

        Console.WriteLine(s)
        Console.WriteLine(o Is Nothing)
    End Sub
End Module
