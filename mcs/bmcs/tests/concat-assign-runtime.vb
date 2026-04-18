' EXPECT: 1
' EXPECT: ab

Module M
    Dim values(0) As String
    Dim calls As Integer

    Function Slot() As Integer
        calls += 1
        Return 0
    End Function

    Sub Main()
        values(0) = "a"
        values(Slot()) &= "b"
        System.Console.WriteLine(calls)
        System.Console.WriteLine(values(0))
    End Sub
End Module
