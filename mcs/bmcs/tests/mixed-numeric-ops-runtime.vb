Class MixedNumericOpsRuntime
    Shared Function Describe(ByVal value As Long) As String
        Return "Long"
    End Function

    Shared Function Describe(ByVal value As Decimal) As String
        Return "Decimal"
    End Function

    Shared Function Describe(ByVal value As Double) As String
        Return "Double"
    End Function

    Shared Function Describe(ByVal value As UInteger) As String
        Return "UInteger"
    End Function

    Shared Function Describe(ByVal value As Boolean) As String
        Return "Boolean"
    End Function

    Shared Sub Main()
        System.Console.WriteLine(Describe(1I + 1UI))
        System.Console.WriteLine(1I + 1UI)
        System.Console.WriteLine(Describe(1L + 1UL))
        System.Console.WriteLine(1L + 1UL)
        System.Console.WriteLine(Describe(5I / 2I))
        System.Console.WriteLine(5I / 2I)
        System.Console.WriteLine(Describe(4294967295UI >> 1))
        System.Console.WriteLine(4294967295UI >> 1)
        System.Console.WriteLine(Describe(1UI < 2I))
        System.Console.WriteLine(1UI < 2I)
        System.Console.WriteLine(Describe(-5UI))
        System.Console.WriteLine(-5UI)
        System.Console.WriteLine(Describe(-5UL))
        System.Console.WriteLine(-5UL)
    End Sub
End Class
