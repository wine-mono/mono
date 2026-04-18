' EXPECT: True
' EXPECT: True
' EXPECT: True
Imports System

Module Test
    Private Function TableIndex(ByVal op1 As TypeCode, ByVal op2 As TypeCode) As Integer
        Return op1 + op2 * 19
    End Function

    Sub Main()
        Dim op1 As TypeCode = TypeCode.String
        Dim op2 As TypeCode = TypeCode.Char

        Console.WriteLine(TableIndex(op1, op2) = CInt(op1) + CInt(op2) * 19)
        Console.WriteLine(TypeCode.Double \ 2 = CInt(TypeCode.Double) \ 2)
        Console.WriteLine(TypeCode.String Mod 4 = CInt(TypeCode.String) Mod 4)
    End Sub
End Module
