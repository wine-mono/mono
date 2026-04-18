' EXPECT: 9
Imports System

Class Sample
    Public Function PrintValue() As Integer
        Return 9
    End Function
End Class

Module Test
    Sub Main()
        Dim s As New Sample()
        Console.WriteLine(s.printvalue())
    End Sub
End Module
