' EXPECT: base
' EXPECT: True

Imports System

Public Class Base
    Public Overridable Function Name() As String
        Return "base"
    End Function
End Class

Public NotInheritable Class Derived : Inherits Base
    Public Overrides Function Name() As String
        Return MyBase.Name()
    End Function
End Class

Module Program
    Sub Main()
        Dim value As Derived = New Derived()
        Console.WriteLine(value.Name())
        Console.WriteLine(TypeOf value Is Base)
    End Sub
End Module
