' EXPECT-COMPILE-CONTAINS: 'TryCast' requires a reference-typed operand and target; `int' is not known to be a reference type
Module Test
    Sub Main()
        Dim o As Object = TryCast(5, Object)
    End Sub
End Module
