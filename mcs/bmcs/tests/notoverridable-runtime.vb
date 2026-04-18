' EXPECT: base
' EXPECT: child
Imports System

Public Class Base
    Public Overridable Function Name() As String
        Return "base"
    End Function
End Class

Public Class Child
    Inherits Base

    Public NotOverridable Overrides Function Name() As String
        Return "child"
    End Function
End Class

Module Test
    Sub Main()
        Dim b As New Base()
        Dim c As Base = New Child()
        Console.WriteLine(b.Name())
        Console.WriteLine(c.Name())
    End Sub
End Module
