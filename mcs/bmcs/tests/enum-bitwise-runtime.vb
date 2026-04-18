' EXPECT: 3
' EXPECT: 1
' EXPECT: 3
' EXPECT: True
' EXPECT: 3
Imports System

<Flags()> _
Enum Bits
    None = 0
    A = 1
    B = 2
End Enum

Module M
    Sub Main()
        Dim x As Bits = Bits.A
        Dim both As Bits = x Or Bits.B
        Dim masked As Bits = both And Bits.A
        Dim flipped As Bits = x Xor Bits.B

        Console.WriteLine(CInt(both))
        Console.WriteLine(CInt(masked))
        Console.WriteLine(CInt(flipped))

        Dim left As Bits? = Bits.A
        Dim right As Bits? = Nothing
        Dim propagated As Bits? = left Or right
        Console.WriteLine(Not propagated.HasValue)

        right = Bits.B
        propagated = left Or right
        Console.WriteLine(CInt(propagated.Value))
    End Sub
End Module
