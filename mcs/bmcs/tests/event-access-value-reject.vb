' EXPECT-COMPILE-CONTAINS: Expression denotes a `event access' where a `variable, value' was expected
Imports System

' VB event access may only appear in RaiseEvent, AddHandler, and
' RemoveHandler.  In an ordinary value context like an initializer it
' must remain classified as event access and fail, rather than silently
' degrading into backing-field or delegate access.

Class C
    Public Event E As EventHandler

    Sub M()
        Dim x = E
    End Sub
End Class
