' EXPECT: ok
Imports System

Public Class Sample
    Sub New()
    End Sub
End Class

Module Test
    Sub Main()
        Dim instance As New Sample()
        If instance IsNot Nothing Then
            Console.WriteLine("ok")
        End If
    End Sub
End Module
