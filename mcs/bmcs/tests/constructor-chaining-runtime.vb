' EXPECT: base 1
' EXPECT: derived 0
' EXPECT: derived 2
Imports System

Class BaseValue
    Public Sub New(v As Integer)
        Console.WriteLine("base " & v)
    End Sub
End Class

Class DerivedValue
    Inherits BaseValue

    Public Sub New()
        MyBase.New(1)
        Console.WriteLine("derived 0")
    End Sub

    Public Sub New(v As Integer)
        Me.New()
        Console.WriteLine("derived " & v)
    End Sub
End Class

Module Test
    Sub Main()
        Dim value As New DerivedValue(2)
    End Sub
End Module
