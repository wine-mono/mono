' EXPECT: 5
Imports System

Public Class Sample
    Const Value As Integer = 5
End Class

Module Test
    Sub Main()
        Console.WriteLine(Sample.Value)
    End Sub
End Module
