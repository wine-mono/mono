' EXPECT: 11
Interface IValue
    ReadOnly Property Value() As Integer
End Interface

Class Sample
    Implements IValue

    Private ReadOnly Property Hidden() As Integer Implements IValue.Value
        Get
            Return 11
        End Get
    End Property
End Class

Module Test
    Sub Main()
        System.Console.WriteLine(DirectCast(New Sample(), IValue).Value)
    End Sub
End Module
