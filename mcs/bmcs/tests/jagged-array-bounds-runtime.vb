' EXPECT: 2
Imports System
Class C
    Shared Sub Main()
        Dim x As Integer()() = New Integer(1)() { New Integer() {1}, New Integer() {2} }
        Console.WriteLine(x.Length)
    End Sub
End Class
