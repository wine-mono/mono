' EXPECT-COMPILE-CONTAINS: inaccessible due to its protection level
Imports System

Public Class Sample
    Dim Value As Integer
End Class

Module Test
    Sub Main()
        Dim instance As New Sample()
        Console.WriteLine(instance.Value)
    End Sub
End Module
