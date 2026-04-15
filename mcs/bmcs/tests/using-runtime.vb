' EXPECT: body 1
' EXPECT: dispose 1
' EXPECT: body 2
' EXPECT: dispose 2
' EXPECT: dispose 3
' EXPECT: caught
Imports System

' VB Using ... End Using must call Dispose() on the resource when
' the block exits, whether normally or via an exception.

Class Disposable
    Implements IDisposable

    Public Tag As Integer

    Public Sub New(t As Integer)
        Tag = t
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Console.WriteLine("dispose " & Tag)
    End Sub
End Class

Module Test
    Sub Main()
        ' Normal exit: dispose must fire after the body.
        Using d As Disposable = New Disposable(1)
            Console.WriteLine("body 1")
        End Using

        ' Normal exit with a nested expression inside the body.
        Using d As Disposable = New Disposable(2)
            Console.WriteLine("body 2")
        End Using

        ' Exceptional exit: Dispose must still fire.
        Try
            Using d As Disposable = New Disposable(3)
                Throw New Exception("boom")
            End Using
        Catch ex As Exception
            Console.WriteLine("caught")
        End Try
    End Sub
End Module
