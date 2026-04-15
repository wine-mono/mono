' EXPECT: Outer`1+Inner`1
' EXPECT: System.Int32
' EXPECT: System.String
Imports System

Class Outer(Of T)
    Public Class Inner(Of U)
    End Class
End Class

Module Test
    Sub Main()
        Dim t As Type = GetType(Outer(Of Integer).Inner(Of String))
        Dim args As Type() = t.GetGenericArguments()
        Console.WriteLine(t.GetGenericTypeDefinition().FullName)
        Console.WriteLine(args(0).FullName)
        Console.WriteLine(args(1).FullName)
    End Sub
End Module
