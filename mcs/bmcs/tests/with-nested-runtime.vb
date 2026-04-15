' EXPECT: 9
Imports System

Class Inner
    Public Value As Integer
End Class

Class Outer
    Public Child As Inner = New Inner()
End Class

Module Test
    Sub Main()
        Dim o As New Outer()

        With o
            With .Child
                .Value = 9
            End With
        End With

        Console.WriteLine(o.Child.Value)
    End Sub
End Module
