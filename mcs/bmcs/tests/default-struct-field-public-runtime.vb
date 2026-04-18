' EXPECT: 4
Imports System

Public Structure Sample
    Dim Value As Integer
End Structure

Module Test
    Sub Main()
        Dim instance As Sample
        instance.Value = 4
        Console.WriteLine(instance.Value)
    End Sub
End Module
