' EXPECT: start
' EXPECT: limit
' EXPECT: step
' EXPECT: 1
' EXPECT: 2
' EXPECT: 3
Imports System

' VB evaluates the lower bound, upper bound, and Step once at loop entry
' in textual order before assigning the initial value to the control variable.
Module Test
    Function Mark(label As String, value As Integer) As Integer
        Console.WriteLine(label)
        Return value
    End Function

    Sub Main()
        For i As Integer = Mark("start", 1) To Mark("limit", 3) Step Mark("step", 1)
            Console.WriteLine(i)
        Next
    End Sub
End Module
