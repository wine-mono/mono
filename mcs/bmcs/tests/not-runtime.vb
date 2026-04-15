' EXPECT: System.Byte
' EXPECT: 254
' EXPECT: System.SByte
' EXPECT: -2
' EXPECT: System.Int16
' EXPECT: -2
' EXPECT: System.UInt16
' EXPECT: 65534
' EXPECT: E
' EXPECT: 254
' EXPECT: System.Boolean
' EXPECT: False

Enum E As Byte
    A = 1
End Enum

Module M
    Sub Main()
        Dim bb = Not CType(1, Byte)
        Dim sb = Not CType(1, SByte)
        Dim sh = Not CType(1, Short)
        Dim us = Not CType(1, UShort)
        Dim ee = Not E.A
        Dim b = Not True

        System.Console.WriteLine(bb.GetType().FullName)
        System.Console.WriteLine(CInt(bb))
        System.Console.WriteLine(sb.GetType().FullName)
        System.Console.WriteLine(CInt(sb))
        System.Console.WriteLine(sh.GetType().FullName)
        System.Console.WriteLine(CInt(sh))
        System.Console.WriteLine(us.GetType().FullName)
        System.Console.WriteLine(CInt(us))
        System.Console.WriteLine(ee.GetType().FullName)
        System.Console.WriteLine(CInt(ee))
        System.Console.WriteLine(b.GetType().FullName)
        System.Console.WriteLine(b)
    End Sub
End Module
