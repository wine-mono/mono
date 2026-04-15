' EXPECT: True
' VB spec: Inherits can be on one part, methods on another.
Imports System

Class Base
    Public Overridable Function IsBase() As Boolean
        Return True
    End Function
End Class

Partial Class Derived
    Inherits Base
End Class

Partial Class Derived
    Public Sub ShowBase()
        Console.WriteLine(IsBase())
    End Sub
End Class

Module Test
    Sub Main()
        Dim d As New Derived()
        d.ShowBase()
    End Sub
End Module
