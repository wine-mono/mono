' EXPECT-COMPILE-CONTAINS: /tmp/mapped-parse.aspx(102) error
' EXPECT-COMPILE-CONTAINS: Parsing error
Imports System

Module Test
    Sub Main()
#ExternalSource("/tmp/mapped-parse.aspx", 100)
        Dim ok As Integer
        Dim bad =
#End ExternalSource
    End Sub
End Module
