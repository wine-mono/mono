' EXPECT: child says hi
Imports System

Public MustInherit Class Base
    Public MustOverride Sub Greet()
    Public MustOverride Function Name() As String
End Class

Public Class Child
    Inherits Base
    Public Overrides Sub Greet()
        Console.WriteLine(Name() & " says hi")
    End Sub
    Public Overrides Function Name() As String
        Return "child"
    End Function
End Class

Module Test
    Sub Main()
        Dim c As Base = New Child
        c.Greet()
    End Sub
End Module
