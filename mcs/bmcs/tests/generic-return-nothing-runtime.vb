' EXPECT: True
' EXPECT: 0
' EXPECT: True
Imports System
Imports System.Reflection

Module M
    Function F(Of T)(ByVal done As Boolean, ByVal x As Object) As T
        If done Then
            Return CType(x, T)
        End If

        Return Nothing
    End Function

    Function ContainsInitobj(ByVal il() As Byte) As Boolean
        Dim i As Integer

        For i = 0 To il.Length - 2
            If il(i) = &HFE AndAlso il(i + 1) = &H15 Then
                Return True
            End If
        Next

        Return False
    End Function

    Sub Main()
        Console.WriteLine(F(Of String)(False, Nothing) Is Nothing)
        Console.WriteLine(F(Of Integer)(False, Nothing))
        Console.WriteLine(ContainsInitobj(GetType(M).GetMethod("F").GetMethodBody().GetILAsByteArray()))
    End Sub
End Module
