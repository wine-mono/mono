' EXPECT: positive
' EXPECT: zero or negative
' EXPECT: done
Imports System

Module Test
    Sub Describe(x As Integer)
        If x > 0 Then Console.WriteLine("positive") Else Console.WriteLine("zero or negative")
    End Sub

    Sub Main()
        Describe(5)
        Describe(0)
        If True Then Console.WriteLine("done")
    End Sub
End Module
