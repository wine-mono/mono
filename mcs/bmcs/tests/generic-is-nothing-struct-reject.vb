' EXPECT-COMPILE-CONTAINS: Operator Is cannot be applied to operands of type `T'
Public Class C
    Shared Function StructT(Of T As Structure)(ByVal x As T) As Boolean
        Return x Is Nothing
    End Function
End Class
