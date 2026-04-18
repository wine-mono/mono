' EXPECT-COMPILE-CONTAINS: single-line-if-label-reject.vb(5) error
' EXPECT-COMPILE-CONTAINS: Parsing error
Module Test
    Sub Main()
        If True Then System.Console.WriteLine("a") : L: System.Console.WriteLine("b")
    End Sub
End Module
