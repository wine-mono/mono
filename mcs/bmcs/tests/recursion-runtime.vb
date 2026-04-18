' EXPECT: 120
' EXPECT: 55
' EXPECT: method: 6
' EXPECT: instance: 16
Imports System

' Function recursion must work.  The tricky case is that the function
' name is also the name of the implicit return variable, so
' naive SimpleName resolution inside the body binds the function
' name to that local and any recursive 'Foo(args)' fails to
' compile.  The VB spec says: in an invocation context, the
' implicit return variable is skipped and resolution continues
' past it to the method.

Module Test
    ' Classic factorial - the Return expression itself contains
    ' a recursive call.
    Function Fact(n As Integer) As Integer
        If n <= 1 Then
            Return 1
        End If
        Return n * Fact(n - 1)
    End Function

    ' Fibonacci - two recursive calls in a single expression.
    Function Fib(n As Integer) As Integer
        If n < 2 Then
            Return n
        End If
        Return Fib(n - 1) + Fib(n - 2)
    End Function

    ' Assignment to the implicit return variable must still work
    ' alongside recursive calls: set the variable, then return it
    ' via a 'Return' statement that also calls the function.
    Function Method(n As Integer) As Integer
        Method = 1
        If n > 1 Then
            Method = n * Method(n - 1)
        End If
        Return Method
    End Function

    Sub Main()
        Console.WriteLine(Fact(5))             ' 120
        Console.WriteLine(Fib(10))             ' 55
        Console.WriteLine("method: " & Method(3))   ' 6

        ' Instance-method recursion (non-Shared context, Me.Foo path).
        Dim h As New Helper()
        Console.WriteLine("instance: " & h.Power(2, 4))  ' 2^4 = 16
    End Sub
End Module

Public Class Helper
    ' Instance recursion path: the disambiguator uses 'Me.Foo'
    ' for non-Shared methods.  Computes factorial via recursion.
    Public Function Power(base1 As Integer, exp As Integer) As Integer
        If exp = 0 Then
            Return 1
        End If
        Return base1 * Power(base1, exp - 1)
    End Function
End Class
