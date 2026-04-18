' EXPECT: 1
' EXPECT: 2
' EXPECT: 3
' EXPECT: ---
' EXPECT: 10
' EXPECT: 8
' EXPECT: 6
' EXPECT: ---
' EXPECT: 5
' EXPECT: 4
' EXPECT: 3
' EXPECT: 2
' EXPECT: 1
' EXPECT: ---
' EXPECT: skipped
' EXPECT: ---
' EXPECT: 5
Imports System

' Verify:
'  1. Positive literal Step still works.
'  2. Even negative literal Step iterates from high to low.
'  3. 'Step -1' (most common vbnc form) works.
'  4. A Step -1 loop where start < end executes zero iterations
'     (not an infinite loop, not a wraparound).
'  5. Single-element loops at both boundaries still fire.

Module Test
    Sub Main()
        ' Positive literal step.
        For i As Integer = 1 To 3
            Console.WriteLine(i)
        Next
        Console.WriteLine("---")

        ' Negative literal step (Step -2).
        For i As Integer = 10 To 6 Step -2
            Console.WriteLine(i)
        Next
        Console.WriteLine("---")

        ' Step -1 (the vbnc-canonical descending form).
        For i As Integer = 5 To 1 Step -1
            Console.WriteLine(i)
        Next
        Console.WriteLine("---")

        ' Negative step with start < end: no iterations.
        For i As Integer = 1 To 5 Step -1
            Console.WriteLine("LEAK " & i)
        Next
        Console.WriteLine("skipped")
        Console.WriteLine("---")

        ' Single-element loop (start = end).
        For i As Integer = 5 To 5
            Console.WriteLine(i)
        Next
    End Sub
End Module
