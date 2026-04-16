' EXPECT: 3
Imports System

Class Emitter
    Public Event Ping As EventHandler

    Public Sub Fire()
        RaiseEvent Ping(Me, EventArgs.Empty)
    End Sub
End Class

Module Test
    Dim total As Integer

    Sub Count(sender As Object, e As EventArgs)
        total += 1
    End Sub

    Sub Main()
        Dim source As New Emitter()

        AddHandler source.Ping, AddressOf Count
        AddHandler source.Ping, AddressOf Count
        source.Fire()

        RemoveHandler source.Ping, AddressOf Count
        source.Fire()

        Console.WriteLine(total)
    End Sub
End Module
