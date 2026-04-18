' EXPECT: 2

Imports System

Interface ITest
    Function M() As Integer
End Interface

Class A
    Implements ITest

    Friend Overridable Function M() As Integer Implements ITest.M
        Return 1
    End Function
End Class

Class B
    Inherits A
    Implements ITest

    Friend Overrides Function M() As Integer
        Return 2
    End Function
End Class

Module Program
    Sub Main()
        Dim x As ITest = New B()
        Console.WriteLine(x.M())
    End Sub
End Module
