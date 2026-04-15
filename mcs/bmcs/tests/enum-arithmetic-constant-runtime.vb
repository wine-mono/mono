' EXPECT: 10
Public Enum E As Byte
    None
    A
End Enum

Module Test
    Private xs() As Integer = {10, 20}

    Private Function F(ByVal e As E) As Integer
        Return xs(e - 1)
    End Function

    Sub Main()
        System.Console.WriteLine(F(E.A))
    End Sub
End Module
