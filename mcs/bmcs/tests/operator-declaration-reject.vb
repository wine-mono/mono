' EXPECT-COMPILE-CONTAINS: requires a matching operator `!=' to also be defined
Module Test
    Structure Box
        Public Value As Integer

        Public Shared Operator =(ByVal x As Box, ByVal y As Box) As Boolean
            Return x.Value = y.Value
        End Operator
    End Structure
End Module
