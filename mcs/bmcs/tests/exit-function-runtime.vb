' EXPECT: 42
' EXPECT: default
' EXPECT: 6
Imports System

' 'Exit Function' must return control to the caller using the
' current value of the implicit return variable.

Module Test
    Function WithValue(x As Integer) As Integer
        WithValue = 42
        If x > 0 Then
            Exit Function   ' must return 42
        End If
        WithValue = 99
        Return WithValue
    End Function

    Function WithDefault() As String
        ' Never assigned: the implicit return variable starts at
        ' the default of the return type (Nothing for String).
        Exit Function
    End Function

    Function Computed(n As Integer) As Integer
        Dim total As Integer = 0
        For i As Integer = 1 To n
            total += i
            If total >= 6 Then
                Computed = total
                Exit Function
            End If
        Next
        Computed = -1
    End Function

    Sub Main()
        Console.WriteLine(WithValue(1))
        Dim s As String = WithDefault()
        If s Is Nothing Then
            Console.WriteLine("default")
        Else
            Console.WriteLine("got: " & s)
        End If
        Console.WriteLine(Computed(100))
    End Sub
End Module
