' EXPECT: 77
Interface ILeftProp
    ReadOnly Property P() As Integer
End Interface

Interface IRightProp
    ReadOnly Property P() As Integer
End Interface

Class Sample
    Implements ILeftProp, IRightProp

    Private ReadOnly Property Q() As Integer Implements ILeftProp.P, IRightProp.P
        Get
            Return 7
        End Get
    End Property
End Class

Module Test
    Sub Main()
        System.Console.Write(DirectCast(New Sample(), ILeftProp).P)
        System.Console.WriteLine(DirectCast(New Sample(), IRightProp).P)
    End Sub
End Module
