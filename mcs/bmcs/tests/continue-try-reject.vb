' EXPECT-COMPILE-CONTAINS: 'Continue Try' is not supported by this bootstrap compiler
' EXPECT-COMPILE-CONTAINS: 'Continue Try' is not supported by this bootstrap compiler
Module Test
    Sub Main()
        Try
            Continue Try
        Catch
        End Try
    End Sub
End Module
