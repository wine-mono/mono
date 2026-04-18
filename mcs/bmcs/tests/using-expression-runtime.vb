' EXPECT: make 1
' EXPECT: body
' EXPECT: dispose 1
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
    Private NextTag As Integer = 0

    Private Function MakeResource() As IDisposable
        NextTag += 1
        Console.WriteLine("make " & NextTag)
        Return New Disposable(NextTag)
    End Function

    Sub Main()
        Using MakeResource()
            Console.WriteLine("body")
        End Using
    End Sub
End Module
