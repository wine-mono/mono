' EXPECT-COMPILE-CONTAINS: cannot assign to `d' because it is readonly
Imports System

Class Disposable
    Implements IDisposable

    Public Sub Dispose() Implements IDisposable.Dispose
    End Sub
End Class

Module Test
    Sub Main()
        Using d As Disposable = New Disposable()
            d = New Disposable()
        End Using
    End Sub
End Module
