Class C
    Default ReadOnly Property Item(i As Integer) As Integer
        Get
            Return i
        End Get
    End Property

    Default ReadOnly Property Item(s As String) As Integer
        Get
            Return s.Length
        End Get
    End Property

    Shared Sub Main()
        Dim c As New C()
        System.Console.WriteLine(c(3))
        System.Console.WriteLine(c("abcd"))
    End Sub
End Class

' EXPECT-RUN-OUTPUT:
' 3
' 4
