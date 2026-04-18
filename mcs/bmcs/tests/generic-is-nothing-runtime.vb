' EXPECT: True
' EXPECT: False
' EXPECT: True
' EXPECT: False
Imports System

Public Class C
    Shared Function RefT(Of T As Exception)(ByVal x As T) As Boolean
        Return x Is Nothing
    End Function

    Shared Function OpenT(Of T)(ByVal x As T) As Boolean
        Return x Is Nothing
    End Function

    Shared Sub Main()
        Console.WriteLine(RefT(Of Exception)(Nothing))
        Console.WriteLine(RefT(Of Exception)(New Exception()))
        Console.WriteLine(OpenT(Of Object)(Nothing))
        Console.WriteLine(OpenT(Of Integer)(1))
    End Sub
End Class
