' EXPECT: pre
' EXPECT: target
' EXPECT: post
' EXPECT: 6
Imports System

' VB GoTo statements and labels.

Module Test
    Sub Main()
        Console.WriteLine("pre")
        GoTo skip
        Console.WriteLine("skipped")
skip:
        Console.WriteLine("target")
        Console.WriteLine("post")

        ' GoTo used in a fall-through loop: accumulate 1+2+3.
        Dim total As Integer = 0
        Dim i As Integer = 1
loop_top:
        total = total + i
        i = i + 1
        If i <= 3 Then GoTo loop_top
        Console.WriteLine(total)
    End Sub
End Module
