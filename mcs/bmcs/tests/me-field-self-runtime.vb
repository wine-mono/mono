' EXPECT: True
Class Sample
    Private selfRef As Sample = Me

    Public Function IsSelf() As Boolean
        Return selfRef Is Me
    End Function
End Class

Module Test
    Sub Main()
        System.Console.WriteLine((New Sample()).IsSelf())
    End Sub
End Module
