' EXPECT: True
' EXPECT: 1
Imports System

Module Test
    Sub Main()
        Dim value As Double
        Console.WriteLine(Double.TryParse("1", value))
        Console.WriteLine(CInt(value))
    End Sub
End Module
