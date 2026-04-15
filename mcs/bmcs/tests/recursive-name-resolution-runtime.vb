' EXPECT: 10
' EXPECT: 10
' EXPECT: 10
' EXPECT: 6
Imports System

Public Delegate Function IntFunc(ByVal x As Integer) As Integer

Public Class InstanceSum
    Public Function SumDown(ByVal n As Integer) As Integer
        If n = 0 Then
            Return 0
        End If

        Return n + SumDown(n - 1)
    End Function
End Class

Public Class SharedSum
    Public Shared Function SumDown(ByVal n As Integer) As Integer
        If n = 0 Then
            Return 0
        End If

        Return n + SumDown(n - 1)
    End Function
End Class

Module Test
    Public Function SumDown(ByVal n As Integer) As Integer
        If n = 0 Then
            Return 0
        End If

        Return n + SumDown(n - 1)
    End Function

    Public Function SumViaDelegate(ByVal n As Integer) As Integer
        Dim f As IntFunc
        f = AddressOf SumViaDelegate

        If n = 0 Then
            Return 0
        End If

        Return n + f(n - 1)
    End Function

    Sub Main()
        Console.WriteLine(New InstanceSum().SumDown(4))
        Console.WriteLine(SharedSum.SumDown(4))
        Console.WriteLine(SumDown(4))
        Console.WriteLine(SumViaDelegate(3))
    End Sub
End Module
