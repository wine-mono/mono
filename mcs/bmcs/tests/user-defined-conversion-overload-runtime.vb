' EXPECT: int
Imports System

Structure Box
    Public Value As Integer

    Public Sub New(ByVal value As Integer)
        Me.Value = value
    End Sub

    Public Shared Widening Operator CType(ByVal x As Box) As Integer
        Return x.Value
    End Operator
End Structure

Module Test
    Function Pick(ByVal value As Integer) As String
        Return "int"
    End Function

    Function Pick(ByVal value As Long) As String
        Return "long"
    End Function

    Sub Main()
        Console.WriteLine(Pick(New Box(7)))
    End Sub
End Module
