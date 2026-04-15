Class NumericLiteralRuntime
    Shared Function Describe(ByVal value As ULong) As String
        Return "ULong"
    End Function

    Shared Function Describe(ByVal value As Integer) As String
        Return "Integer"
    End Function

    Shared Function Describe(ByVal value As Short) As String
        Return "Short"
    End Function

    Shared Function Describe(ByVal value As Single) As String
        Return "Single"
    End Function

    Shared Function Describe(ByVal value As Double) As String
        Return "Double"
    End Function

    Shared Function Describe(ByVal value As Decimal) As String
        Return "Decimal"
    End Function

    Shared Sub Main()
        Dim tiny As SByte = 127
        Dim wide As UShort = 65535US

        System.Console.WriteLine(18446744073709551615UL = 18446744073709551615UL)
        System.Console.WriteLine(Describe(18446744073709551615UL))
        System.Console.WriteLine(&H8000S)
        System.Console.WriteLine(Describe(&H8000S))
        System.Console.WriteLine(Describe(1!))
        System.Console.WriteLine(Describe(1#))
        System.Console.WriteLine(Describe(1@))
        System.Console.WriteLine(&H80000000)
        System.Console.WriteLine(Describe(&H80000000))
        System.Console.WriteLine(tiny)
        System.Console.WriteLine(wide)
    End Sub
End Class
