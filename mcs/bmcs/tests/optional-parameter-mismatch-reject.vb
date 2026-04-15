' EXPECT-COMPILE-CONTAINS: Cannot convert from 'string' to 'int'
' EXPECT-COMPILE-NOT-CONTAINS: CS1501
Module Test
    Sub F(x As Integer, Optional y As Integer = 10)
    End Sub

    Sub Main()
        F("bad")
    End Sub
End Module
