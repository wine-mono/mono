' EXPECT-COMPILE-CONTAINS: could not be found in `M'
' EXPECT-COMPILE-NOT-CONTAINS: CS0165
Module M
    Sub Main()
        x = 1
        Dim x As Integer
    End Sub
End Module
