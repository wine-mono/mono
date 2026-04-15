' EXPECT: System.Int32[,][]
Imports System
Class C
    Shared Sub Main()
        Dim x As Object = New Integer()(,) { New Integer(,) {{1, 2}}, New Integer(,) {{3, 4}} }
        Console.WriteLine(x.GetType().FullName)
    End Sub
End Class
