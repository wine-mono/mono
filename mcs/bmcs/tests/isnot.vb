' EXPECT: yes
' EXPECT: yes
Imports System

Module Test
    Sub Main()
        Dim s As String = "hi"
        If s IsNot Nothing Then
            Console.WriteLine("yes")
        End If

        Dim t As Object = New Object()
        Dim u As Object = Nothing
        If t IsNot u Then
            Console.WriteLine("yes")
        End If
    End Sub
End Module
