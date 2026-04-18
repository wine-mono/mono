' EXPECT: 2

Interface ICounter
    Function M() As Integer
End Interface

Class A
    Implements ICounter

    Friend Overridable Function M() As Integer Implements ICounter.M
        Return 1
    End Function
End Class

Class B
    Inherits A

    Friend Overrides Function M() As Integer
        Return 2
    End Function
End Class

Module Test
    Sub Main()
        Dim x As ICounter = New B()
        System.Console.WriteLine(x.M())
    End Sub
End Module
