' EXPECT: yes
' PREPROCESS: -D TEST_PP=True
Imports System
#If TEST_PP Then
Module Test
    Sub Main()
        Console.WriteLine("yes")
    End Sub
End Module
#End If
