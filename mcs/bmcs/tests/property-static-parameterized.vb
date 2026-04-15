' EXPECT: one
Imports System
Class Lookup
    Private Shared _data() As String = {"zero", "one", "two"}
    Default Public Shared ReadOnly Property Item(i As Integer) As String
        Get
            Return _data(i)
        End Get
    End Property
End Class
Module Test
    Sub Main()
        Console.WriteLine(Lookup.Item(1))
    End Sub
End Module
