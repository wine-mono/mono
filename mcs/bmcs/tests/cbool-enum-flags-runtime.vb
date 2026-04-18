' EXPECT:
' True
' False
' True
' False

Imports System.Reflection

<Flags()> _
Enum Bits As UShort
    None = 0
    A = 1
    B = 2
End Enum

Module Test
    Sub Main()
        Dim bitsValue As Bits = Bits.A Or Bits.B
        System.Console.WriteLine(CBool(bitsValue And Bits.A))
        System.Console.WriteLine(CBool(bitsValue And Bits.None))
        System.Console.WriteLine(CBool(BindingFlags.Public And BindingFlags.Public))
        System.Console.WriteLine(CBool(BindingFlags.Public And BindingFlags.NonPublic))
    End Sub
End Module
