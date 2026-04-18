' EXPECT: 7
' Derived instance field initializers must run after the chosen MyBase.New()
' call when the initializer reads state exposed by a base member.
Class Base
    Private ReadOnly values As Integer()

    Public Sub New()
        values = New Integer() {7}
    End Sub

    Protected ReadOnly Property First As Integer
        Get
            Return values(0)
        End Get
    End Property
End Class

Class Derived
    Inherits Base

    Private ReadOnly snapshot As Integer = First

    Public Sub New()
        MyBase.New()
    End Sub

    Public Sub Print()
        System.Console.WriteLine(snapshot)
    End Sub
End Class

Module Test
    Sub Main()
        Dim value As New Derived()
        value.Print()
    End Sub
End Module
