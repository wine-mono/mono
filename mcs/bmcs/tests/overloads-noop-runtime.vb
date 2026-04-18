' EXPECT: 1
' EXPECT: 2
Imports System

Public Class Sample
    Overloads Sub Show()
        Console.WriteLine("1")
    End Sub

    Overloads Sub Show(ByVal value As Integer)
        Console.WriteLine(value)
    End Sub
End Class

Module Test
    Sub Main()
        Dim instance As New Sample()
        instance.Show()
        instance.Show(2)
    End Sub
End Module
