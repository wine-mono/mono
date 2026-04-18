' EXPECT: body 10 20
' EXPECT: dispose 20
' EXPECT: dispose 10
Imports System

Class Disposable
    Implements IDisposable

    Public Tag As Integer

    Public Sub New(tagValue As Integer)
        Tag = tagValue
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Console.WriteLine("dispose " & Tag)
    End Sub
End Class

Module Test
    Sub Main()
        Using first As Disposable = New Disposable(10), second As Disposable = New Disposable(20)
            Console.WriteLine("body " & first.Tag & " " & second.Tag)
        End Using
    End Sub
End Module
