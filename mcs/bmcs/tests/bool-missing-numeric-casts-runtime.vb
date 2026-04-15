' EXPECT: -1
' EXPECT: 0
' EXPECT: 65535
' EXPECT: 0
' EXPECT: 4294967295
' EXPECT: 0
' EXPECT: 18446744073709551615
' EXPECT: 0
Module M
    Sub Main()
        System.Console.WriteLine(CSByte(True))
        System.Console.WriteLine(CSByte(False))
        System.Console.WriteLine(CUShort(True))
        System.Console.WriteLine(CUShort(False))
        System.Console.WriteLine(CUInt(True))
        System.Console.WriteLine(CUInt(False))
        System.Console.WriteLine(CULng(True))
        System.Console.WriteLine(CULng(False))
    End Sub
End Module
