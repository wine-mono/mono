Imports System
Module IntegerTypeCharTest
    Sub Main()
            Dim m As Integer
            m = f(20)
            If m <> 20 Then
                Throw New Exception("IntegerTypeCharTest: failed")
            End If
            Exit Sub
    End Sub

    Function f%(ByVal param%)
        f = param
    End Function
End Module
