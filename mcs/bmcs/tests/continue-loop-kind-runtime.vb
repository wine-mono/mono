' EXPECT: w 1:1
' EXPECT: cf 1
' EXPECT: w 2:1
' EXPECT: cf 2
' EXPECT: w 3:1
' EXPECT: cf 3
' EXPECT: ---
' EXPECT: f 1:1
' EXPECT: ew 1
Imports System

' Continue/Exit must honor the written loop kind, not merely the
' innermost enclosing loop. This covers Continue For inside While and
' Exit While inside For.
Module Test
    Sub Main()
        For i As Integer = 1 To 3
            Dim j As Integer = 0
            While j < 3
                j += 1
                If j = 2 Then
                    Console.WriteLine("cf " & i)
                    Continue For
                End If
                Console.WriteLine("w " & i & ":" & j)
            End While
            Console.WriteLine("tail " & i)
        Next

        Console.WriteLine("---")

        Dim k As Integer = 0
        While k < 3
            k += 1
            For m As Integer = 1 To 3
                If m = 2 Then
                    Console.WriteLine("ew " & k)
                    Exit While
                End If
                Console.WriteLine("f " & k & ":" & m)
            Next
            Console.WriteLine("after " & k)
        End While
    End Sub
End Module
