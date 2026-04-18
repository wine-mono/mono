' EXPECT-COMPILE-CONTAINS: 'Static' local variables are not supported by this bootstrap compiler; rewrite as a Shared or instance field on the enclosing class
Imports System

' VB 'Static' local variables require semantics this bootstrap
' compiler does not implement: mutations must be visible across
' recursive calls to the same method, initializers must run
' under Monitor protection, and the backing storage must be
' per-instance for non-Shared methods and per-type for Shared
' methods. The compiler rejects 'Static' so this case fails
' explicitly instead of silently miscompiling.

Module Test
    Sub UsingStatic()
        Static n As Integer
        n = n + 1
        Console.WriteLine(n)
    End Sub

    Sub Main()
        UsingStatic()
    End Sub
End Module
