' EXPECT: True
' EXPECT: False
' EXPECT: True
Imports System

Module Test
    Private Function Matches(Of T)(ByVal value As Object) As Boolean
        Return TypeOf value Is T
    End Function

    Sub Main()
        Console.WriteLine(Matches(Of String)("hello"))
        Console.WriteLine(Matches(Of String)(New Object()))
        Console.WriteLine(Matches(Of Integer)(CObj(42)))
    End Sub
End Module
