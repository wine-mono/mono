' EXPECT-COMPILE-CONTAINS: cannot be sealed because it is not an override
Public Class Sample
    Public NotOverridable Function Name() As String
        Return "x"
    End Function
End Class
