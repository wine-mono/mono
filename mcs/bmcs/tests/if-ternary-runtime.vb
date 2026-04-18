' EXPECT: True
' EXPECT: x
Imports System

Module Test
    Sub Main()
        Dim missing As String = If(True, Nothing, "x")
        Dim value As String = If(False, Nothing, "x")

        Console.WriteLine(missing Is Nothing)
        Console.WriteLine(value)
    End Sub
End Module
