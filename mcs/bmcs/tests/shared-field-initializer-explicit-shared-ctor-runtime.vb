' EXPECT: False
' EXPECT: False

Class C
    Private Shared O As New Object()
    Private Shared P As Object = New Object()

    Shared Sub New()
    End Sub

    Shared Sub Main()
        System.Console.WriteLine(O Is Nothing)
        System.Console.WriteLine(P Is Nothing)
    End Sub
End Class
