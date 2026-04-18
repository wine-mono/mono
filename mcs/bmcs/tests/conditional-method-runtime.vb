' EXPECT: 1

Imports System.Diagnostics

Module Test
    <Conditional("DEBUG")> _
    Private Sub TraceHit()
        System.Console.Write("2")
    End Sub

    Sub Main()
        System.Console.Write("1")
        TraceHit()
    End Sub
End Module
