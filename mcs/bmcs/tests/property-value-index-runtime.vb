' EXPECT: 1
Imports System
Imports System.Collections.Generic

Class Box
    Private store As New Dictionary(Of Char, Integer)()

    Public ReadOnly Property Map() As Dictionary(Of Char, Integer)
        Get
            Return store
        End Get
    End Property
End Class

Module Test
    Sub Main()
        Dim b As New Box()
        b.Map("a"c) = 1
        Console.WriteLine(b.Map("a"c))
    End Sub
End Module
