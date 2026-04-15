' EXPECT: 0
' EXPECT: 5
Imports System

Module Test
    Sub Main()
        Dim x As Nullable(Of Integer)

        x = Nothing
        Console.WriteLine(If(x, 0))

        x = New Nullable(Of Integer)(5)
        Console.WriteLine(If(x, 0))
    End Sub
End Module
