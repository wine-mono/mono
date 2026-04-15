' EXPECT: 42
' EXPECT: 4294967295
' EXPECT: 0
' EXPECT: 4277077181
Imports System

' VB unsigned integer literal suffixes: US, UI, and UL.
'
' The helpers widen to Int64 so the test can print the values
' without depending on unsigned-to-string helpers in the VB runtime.

Module Test
    Sub ShowUInt(n As UInteger)
        Console.WriteLine(CLng(n))
    End Sub

    Sub ShowULong(n As ULong)
        Console.WriteLine(CLng(n))
    End Sub

    Sub Main()
        ShowUInt(42UI)
        ShowULong(4294967295UL)
        ShowULong(0UL)

        ' 0xFEEF04BD = 4277077181.
        ShowUInt(&HFEEF04BDUI)
    End Sub
End Module
