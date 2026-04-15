' EXPECT-COMPILE-CONTAINS: 'AddHandler' requires an event access as its first operand
Imports System

Module Test
    Sub Main()
        Dim handler As EventHandler = Nothing
        AddHandler handler, handler
    End Sub
End Module
