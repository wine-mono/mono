' EXPECT: 0
' EXPECT: 1
' EXPECT: 2
' EXPECT: ---
' EXPECT: 10
' EXPECT: 12
' EXPECT: 14
Imports System

Module Test
    Sub Main()
        ' Basic typed For ... To loop
        For i As Integer = 0 To 2
            Console.WriteLine(i)
        Next
        Console.WriteLine("---")

        ' Pre-declared variable + Step
        Dim j As Integer
        For j = 10 To 14 Step 2
            Console.WriteLine(j)
        Next
    End Sub
End Module
