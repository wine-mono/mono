' EXPECT: 10
' EXPECT: 6
' EXPECT: 9
Imports System

' VB spec (VB 11 section 11.6.1 "Simple Name Expressions"): a
' bare function-name reference inside 'Function Foo' normally
' binds to the implicit return local, but in invocation,
' invocation-statement AND AddressOf contexts the local is
' skipped and resolution continues to the method group.
' The core recursion case is covered by recursion-runtime.vb;
' this test exercises the AddressOf branch specifically for
' Shared (Module) and instance methods.  The delegate
' construction form here is the one this test covers.

Delegate Function IntUnary(x As Integer) As Integer

Module Test
    ' Shared (Module) method: the disambiguator rewrites the
    ' 'AddressOf Doubler' argument expression from the bare
    ' SimpleName(Doubler) - which would bind to the implicit
    ' return local - to MemberAccess(Test, Doubler), which
    ' resolves to the method group on the Module's IL class.
    Function Doubler(x As Integer) As Integer
        If x < 0 Then
            Dim self As IntUnary = New IntUnary(AddressOf Doubler)
            Return self(-x)
        End If
        Return x * 2
    End Function

    Sub Main()
        Console.WriteLine(Doubler(-5))  ' self(5) -> 5 * 2 = 10
        Console.WriteLine(Doubler(3))   ' 6

        Dim h As New Helper()
        Console.WriteLine(h.Triple(3))  ' 9
    End Sub
End Module

Public Class Helper
    ' Instance method: the disambiguator rewrites
    ' 'AddressOf Triple' to MemberAccess(This, Triple), so
    ' the delegate is bound to 'Me.Triple', not to the
    ' Integer return local.
    Public Function Triple(x As Integer) As Integer
        If x < 0 Then
            Dim self As IntUnary = New IntUnary(AddressOf Triple)
            Return self(-x)
        End If
        Return x * 3
    End Function
End Class
