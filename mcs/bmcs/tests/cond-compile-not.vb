' EXPECT: release mode
Imports System

Module Test
    Sub Main()
#If Not DEBUG Then
        Console.WriteLine("release mode")
#Else
        Console.WriteLine("debug mode")
#End If
    End Sub
End Module
