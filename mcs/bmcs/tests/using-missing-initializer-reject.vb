' EXPECT-COMPILE-CONTAINS: 'Using' resource variables must include an initializer
Imports System

Class Disposable
    Implements IDisposable

    Public Sub Dispose() Implements IDisposable.Dispose
    End Sub
End Class

Module Test
    Sub Main()
        Using d As Disposable
        End Using
    End Sub
End Module
