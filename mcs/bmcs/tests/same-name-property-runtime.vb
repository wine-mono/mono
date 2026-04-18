' EXPECT: 12
Class Message
    Private _value As Integer

    Public Property Message As Integer
        Get
            Return _value
        End Get
        Set(ByVal value As Integer)
            _value = value
        End Set
    End Property

    Shared Sub Main()
        Dim m As New Message()
        m.Message = 12
        System.Console.WriteLine(m.Message)
    End Sub
End Class
