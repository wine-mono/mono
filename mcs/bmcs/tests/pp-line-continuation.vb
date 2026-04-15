' EXPECT: continued
Imports System

Module Test
    Sub Main()
#If _
        True Then
        Console.WriteLine("continued")
#Else
        Console.WriteLine("wrong")
#End If
    End Sub
End Module
