' EXPECT: 1
Imports System

#If DEBUG AndAlso False Then
Module Test
    Sub Main()
        Console.WriteLine(0)
    End Sub
End Module
#Else
Module Test
    Sub Main()
        Console.WriteLine(1)
    End Sub
End Module
#End If
