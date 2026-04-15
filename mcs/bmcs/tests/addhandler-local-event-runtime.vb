' EXPECT: 3
Imports System

Class Emitter
    Public Event Ping As EventHandler

    Shared total As Integer

    Sub Count(sender As Object, e As EventArgs)
        total += 1
    End Sub

    Public Sub Run()
        Dim stored As EventHandler = New EventHandler(AddressOf Count)

        AddHandler Ping, stored
        AddHandler Ping, New EventHandler(AddressOf Count)
        RaiseEvent Ping(Me, EventArgs.Empty)

        RemoveHandler Ping, stored
        RaiseEvent Ping(Me, EventArgs.Empty)

        Console.WriteLine(total)
    End Sub
End Class

Module Test
    Sub Main()
        Dim source As New Emitter()
        source.Run()
    End Sub
End Module
