' EXPECT: before 0
' EXPECT: before 1
' EXPECT: before 2
' EXPECT: after 0
' EXPECT: after 1
' EXPECT: after 2
' EXPECT: until 0
' EXPECT: until 1
Imports System

Module Test
    Sub Main()
        ' Do While cond ... Loop (test-before)
        Dim i As Integer = 0
        Do While i < 3
            Console.WriteLine("before " & i)
            i += 1
        Loop

        ' Do ... Loop While cond (test-after)
        i = 0
        Do
            Console.WriteLine("after " & i)
            i += 1
        Loop While i < 3

        ' Do Until cond ... Loop (test-before, negated)
        i = 0
        Do Until i >= 2
            Console.WriteLine("until " & i)
            i += 1
        Loop
    End Sub
End Module
