' EXPECT: derived+base
Imports System
Class Base
    Public Overridable ReadOnly Property Val() As String
        Get
            Return "base"
        End Get
    End Property
End Class
Class Derived
    Inherits Base
    Public Overrides ReadOnly Property Val() As String
        Get
            Return "derived+" & MyBase.Val
        End Get
    End Property
End Class
Module Test
    Sub Main()
        Dim d As New Derived()
        Console.WriteLine(d.Val)
    End Sub
End Module
