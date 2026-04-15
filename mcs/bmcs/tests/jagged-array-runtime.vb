' EXPECT: 3
Imports System
Class C
    Shared Sub Main()
        Dim x As Integer()() = New Integer()() { New Integer() {1}, New Integer() {2, 3} }
        Console.WriteLine(x(1)(1))
    End Sub
End Class
