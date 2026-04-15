' EXPECT-COMPILE-CONTAINS: diagnostic-filename-bad.vb(6) error
' EXPECT-COMPILE-CONTAINS: Parsing error
Module BadFile
    Sub Broken()
        Dim x =
    End Sub
End Module
