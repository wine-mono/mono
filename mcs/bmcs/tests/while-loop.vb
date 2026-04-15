' EXPECT: 0
' EXPECT: 1
' EXPECT: 2
' EXPECT: done
Imports System

Module Test
    Sub Main()
        Dim i As Integer = 0
        While i < 3
            Console.WriteLine(i)
            i += 1
        End While
        Console.WriteLine("done")
    End Sub
End Module
