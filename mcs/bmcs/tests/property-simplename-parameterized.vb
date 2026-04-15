' EXPECT: b
Imports System
Class Foo
    Private _data() As String = {"a", "b", "c"}
    Default Public ReadOnly Property Item(i As Integer) As String
        Get
            Return _data(i)
        End Get
    End Property
    Function GetIt(i As Integer) As String
        Return Item(i)
    End Function
End Class
Module Test
    Sub Main()
        Dim f As New Foo()
        Console.WriteLine(f.GetIt(1))
    End Sub
End Module
