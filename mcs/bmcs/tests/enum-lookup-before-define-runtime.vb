' BMCS-ARGS: support/enum-lookup-before-define-helper.vbi
' EXPECT: 3
' EXPECT: 4
' EXPECT: 5
' EXPECT: 6
' EXPECT: 7
' EXPECT: UShort
' EXPECT: Using
Module Test
    Sub Main()
        System.Console.WriteLine(CInt(KS.UShort))
        System.Console.WriteLine(CInt(KS.Using))
        System.Console.WriteLine(CInt(KS.Xor))
        System.Console.WriteLine(CInt(KS.LT))
        System.Console.WriteLine(CInt(KS.NumberOfItems))
        System.Console.WriteLine(strSpecial(KS.UShort))
        System.Console.WriteLine(strSpecial(KS.Using))
    End Sub
End Module
