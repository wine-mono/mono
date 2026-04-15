' EXPECT-COMPILE-CONTAINS: must be a reference type in order to use it as type parameter `T'
Module Test
    Function Make(Of T As Class)() As Integer
        Return 0
    End Function

    Sub Main()
        Make(Of Integer)()
    End Sub
End Module
