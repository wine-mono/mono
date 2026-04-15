' EXPECT: debug on
' PREPROCESS: -D DEBUG=1
Imports System

Module Test
    Sub Main()
#If DEBUG Then
        Console.WriteLine("debug on")
#Else
        Console.WriteLine("debug off")
#End If
    End Sub
End Module
