' EXPECT-COMPILE-CONTAINS: Array bounds cannot appear in type specifiers.
Imports System.Collections.Generic

Module Test
    Sub Main()
        Dim value As Dictionary(Of String, Integer(3)) = Nothing
    End Sub
End Module
