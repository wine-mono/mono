' EXPECT: 1
Imports System

Enum Bits As Byte
    A = 1
End Enum

Module Test
    Sub PrintByte(ByVal value As Byte)
        Console.WriteLine(value)
    End Sub

    Sub Main()
        PrintByte(Bits.A)
    End Sub
End Module
