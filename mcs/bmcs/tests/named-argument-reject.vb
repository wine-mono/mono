' EXPECT-COMPILE-CONTAINS: Named arguments are not supported by this bootstrap compiler; rewrite the call to use positional arguments
Module Test
    Private Sub Foo(a As Integer, b As Integer)
    End Sub

    Sub Main()
        Foo(b := 1, a := 2)
    End Sub
End Module
