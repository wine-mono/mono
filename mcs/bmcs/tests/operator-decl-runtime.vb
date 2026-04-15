' EXPECT: True
' EXPECT: False
' EXPECT: True
' EXPECT: hello world
Imports System

' User-defined operator declarations should emit real operator
' metadata and bind through ordinary VB operator resolution.

Public Class Token
    Public Value As String
    Public Sub New(v As String)
        Value = v
    End Sub

    Shared Operator =(a As Token, b As Token) As Boolean
        Return a.Value = b.Value
    End Operator

    Shared Operator <>(a As Token, b As Token) As Boolean
        Return a.Value <> b.Value
    End Operator
End Class

Public Class Wrapper
    Public N As Integer
    Public Sub New(x As Integer)
        N = x
    End Sub

    Shared Operator >(a As Wrapper, b As Wrapper) As Boolean
        Return a.N > b.N
    End Operator

    Shared Operator <(a As Wrapper, b As Wrapper) As Boolean
        Return a.N < b.N
    End Operator
End Class

Module Test
    Sub Main()
        Dim a As New Token("hi")
        Dim b As New Token("hi")
        Dim c As New Token("bye")
        Console.WriteLine(a = b)    ' True  (value eq)
        Console.WriteLine(a = c)    ' False
        Console.WriteLine(a <> c)   ' True

        Dim x As New Wrapper(3)
        Dim y As New Wrapper(5)
        If x < y Then
            Console.WriteLine("hello world")
        End If
    End Sub
End Module
