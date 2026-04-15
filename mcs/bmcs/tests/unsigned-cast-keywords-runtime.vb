' EXPECT: 1
' EXPECT: 2
' EXPECT: 3

Module Test
    Sub Main()
        Dim a As UShort = CUShort(1)
        Dim b As UInteger = CUInt(2)
        Dim c As ULong = CULng(3)

        System.Console.WriteLine(a)
        System.Console.WriteLine(b)
        System.Console.WriteLine(c)
    End Sub
End Module
