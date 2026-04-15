' EXPECT: 1
Imports System

Enum Bits As Byte
    A = 1
    B = 2
End Enum

Module Test
    Sub Main()
        Dim value As Bits = Bits.A
        Console.WriteLine(CInt(value))
    End Sub
End Module
