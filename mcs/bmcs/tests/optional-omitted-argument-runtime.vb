' EXPECT: 24

Module M
    Function Sum(Optional ByVal a As Integer = 10, Optional ByVal b As Integer = 20, Optional ByVal c As Integer = 30) As Integer
        Return a + b + c
    End Function

    Sub Main()
        System.Console.WriteLine(Sum(1, , 3))
    End Sub
End Module
