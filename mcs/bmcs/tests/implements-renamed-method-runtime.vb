' EXPECT: 9
Interface IValue
    Function GetValue() As Integer
End Interface

Class Sample
    Implements IValue

    Private Function ReadValue() As Integer Implements IValue.GetValue
        Return 9
    End Function
End Class

Module Test
    Sub Main()
        System.Console.WriteLine(DirectCast(New Sample(), IValue).GetValue())
    End Sub
End Module
