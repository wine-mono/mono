' EXPECT: base False False True
' EXPECT: derived
Imports System

Class BaseValue
    Public Sub New(ByVal compiler As Integer, Optional ByVal skip As Boolean = False, Optional ByVal canFail As Boolean = False, Optional ByVal canBeImplicitSimpleName As Boolean = True)
        Console.WriteLine("base " & skip & " " & canFail & " " & canBeImplicitSimpleName)
    End Sub
End Class

Class DerivedValue
    Inherits BaseValue

    Public Sub New()
        MyBase.New(1)
        Console.WriteLine("derived")
    End Sub
End Class

Module Test
    Sub Main()
        Dim value As New DerivedValue()
    End Sub
End Module
