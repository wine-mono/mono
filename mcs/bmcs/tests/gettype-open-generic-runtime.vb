' EXPECT: True
' EXPECT: True
' EXPECT: 2
' EXPECT: True
Imports System

Module Test
    Sub Main()
        Dim openType As Type = GetType(System.Collections.Generic.Dictionary(Of ,))
        Dim closedType As Type = GetType(System.Collections.Generic.Dictionary(Of Integer, String))

        Console.WriteLine(openType.IsGenericType)
        Console.WriteLine(openType.IsGenericTypeDefinition)
        Console.WriteLine(openType.GetGenericArguments().Length)
        Console.WriteLine(openType Is closedType.GetGenericTypeDefinition())
    End Sub
End Module
