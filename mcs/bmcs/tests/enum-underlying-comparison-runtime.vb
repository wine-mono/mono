' EXPECT: True
' EXPECT: True
' EXPECT: True
Imports System

Enum CompareResult
    Less
    Equal
    Greater
End Enum

Module Test
    Function MatchInteger(ByVal val As Integer) As Boolean
        Return val = CompareResult.Equal
    End Function

    Function MatchLong(ByVal val As Long) As Boolean
        Return val > CompareResult.Equal
    End Function

    Sub Main()
        Console.WriteLine(MatchInteger(1))
        Console.WriteLine(MatchLong(2))
        Console.WriteLine(2 > CompareResult.Equal)
    End Sub
End Module
