' EXPECT: ok
Imports System

Module Test
    Sub Main()
#If False Then
        @#$^**
#Else
        Console.WriteLine("ok")
#End If
    End Sub
End Module
