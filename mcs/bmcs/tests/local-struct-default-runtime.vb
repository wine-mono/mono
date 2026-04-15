' EXPECT: 0
Structure S
    Public X As Integer
End Structure

Module M
    Sub Main()
        Dim p As S
        System.Console.WriteLine(p.X)
    End Sub
End Module
