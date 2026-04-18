' EXPECT: 9
Imports System

Public Interface IValue
    Function GetValue() As Integer
End Interface

Public Class Sample
    Implements IValue

    Public Function GetValue() As Integer Implements IValue.GetValue
        Return 9
    End Function
End Class

Module Test
    Sub Main()
        Dim value As IValue = New Sample()
        Console.WriteLine(value.GetValue())
    End Sub
End Module
