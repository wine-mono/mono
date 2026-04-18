' EXPECT: [x]
' EXPECT: zero
Imports System

Class C
    Function F() As String
        Return "zero"
    End Function

    Function F(ParamArray xs() As Integer) As String
        Return "params"
    End Function
End Class

Module M
    Sub Main()
        Console.WriteLine("[" & " x ".Trim() & "]")

        Dim c As New C()
        Console.WriteLine(c.F())
    End Sub
End Module
