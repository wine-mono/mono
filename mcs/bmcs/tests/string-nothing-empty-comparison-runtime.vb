' EXPECT: True
' EXPECT: False
' EXPECT: True
' EXPECT: False

Imports System

Module Test
    Sub Main()
        Dim s As String = Nothing

        Console.WriteLine(s = String.Empty)
        Console.WriteLine(s <> String.Empty)
        Console.WriteLine(Nothing = String.Empty)
        Console.WriteLine(Nothing <> String.Empty)
    End Sub
End Module
