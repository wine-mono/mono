' EXPECT: 3
Imports System.Collections.Generic

Class Sample
    Implements IComparer(Of Integer)

    Private Function CompareInts(ByVal x As Integer, ByVal y As Integer) As Integer Implements IComparer(Of Integer).Compare
        Return x - y
    End Function
End Class

Module Test
    Sub Main()
        Dim cmp As IComparer(Of Integer) = New Sample()
        System.Console.WriteLine(cmp.Compare(7, 4))
    End Sub
End Module
