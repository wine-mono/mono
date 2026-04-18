' EXPECT: call 1
' EXPECT: hit
' EXPECT: calls=1
' EXPECT: call 1
' EXPECT: fallback
' EXPECT: calls=1
Imports System

' VB9 If(a, b) is a null-coalescing expression: return 'a' if it
' is non-Nothing, else return 'b'.  Crucially, 'a' must be
' evaluated EXACTLY ONCE even though the result logically needs
' to be both tested and returned.

Module Test
    Dim m_Calls As Integer = 0

    Function MakeString(value As String) As String
        m_Calls += 1
        Console.WriteLine("call " & m_Calls)
        Return value
    End Function

    Sub Check(label As String, got As String)
        Console.WriteLine(label)
        Console.WriteLine("calls=" & m_Calls)
    End Sub

    Sub Main()
        ' Case 1: 'a' is non-null.  We should see exactly one "call",
        ' the result should be "hit", and calls=1.
        m_Calls = 0
        Dim r1 As String = If(MakeString("hit"), "fallback")
        Check(r1, r1)

        ' Case 2: 'a' is Nothing.  We should still see exactly one
        ' "call" for the left side (returning Nothing), then fall
        ' through to the right side.
        m_Calls = 0
        Dim r2 As String = If(MakeString(Nothing), "fallback")
        Check(r2, r2)
    End Sub
End Module
