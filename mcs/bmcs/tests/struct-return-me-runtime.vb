' EXPECT: 1
' EXPECT: 1
Imports System

Public Structure Counter
    Private value As Integer

    Public Function Bump() As Counter
        value = value + 1
        Return Me
    End Function

    Public Function GetValue() As Integer
        Return value
    End Function
End Structure

Module M
    Sub Main()
        Dim c As New Counter()
        Dim bumped As Counter = c.Bump()
        Console.WriteLine(bumped.GetValue())
        Console.WriteLine(c.GetValue())
    End Sub
End Module
