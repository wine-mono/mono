' EXPECT: -1.5
' EXPECT: -2.5
Module Test
    Sub Main()
        Dim d As Double = 1.5
        Dim s As Single = 2.5F

        System.Console.WriteLine(-d)
        System.Console.WriteLine(-s)
    End Sub
End Module
