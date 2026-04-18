' EXPECT: 0
' EXPECT: 0

Structure Payload
    Public Value As Integer
End Structure

Class Box
    Public Shared Function ReadValue(ByVal arg As Payload) As Integer
        Return arg.Value
    End Function

    Public Sub New(ByVal arg As Payload)
        System.Console.WriteLine(arg.Value)
    End Sub
End Class

Module Program
    Sub Main()
        System.Console.WriteLine(Box.ReadValue(Nothing))
        Dim tmp As New Box(Nothing)
    End Sub
End Module
