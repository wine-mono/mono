' EXPECT: base
Imports System

Public Class Base
    Overridable Function Name() As String
        Return "base"
    End Function
End Class

Public Class Derived
    Inherits Base

    Overrides Function Name() As String
        Return MyBase.Name()
    End Function
End Class

Module Test
    Sub Main()
        Dim value As New Derived()
        Console.WriteLine(value.Name())
    End Sub
End Module
