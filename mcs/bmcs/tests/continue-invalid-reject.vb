' EXPECT-COMPILE-CONTAINS: 'Continue Select' is not supported by this bootstrap compiler
' EXPECT-COMPILE-CONTAINS: 'Continue Select' is not supported by this bootstrap compiler
Module Test
    Sub Main()
        Select Case 1
            Case 1
                Continue Select
        End Select
    End Sub
End Module
