' EXPECT-COMPILE-CONTAINS: 'TryCast' operands of type parameter type `T' must have a reference-type constraint
Module Test
    Function F(Of T)(x As T) As Object
        Return TryCast(x, Object)
    End Function
End Module
