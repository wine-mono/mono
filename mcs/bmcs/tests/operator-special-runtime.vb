' EXPECT: 10:3
' EXPECT: 3
' EXPECT: True
' EXPECT: 7
' EXPECT: 4
' EXPECT: yes
' EXPECT: -1
Imports System

Structure PairText
    Public Text As String

    Public Shared Operator &(ByVal x As PairText, ByVal y As PairText) As String
        Return x.Text & ":" & y.Text
    End Operator

    Public Shared Operator Like(ByVal x As PairText, ByVal y As PairText) As Boolean
        Return x.Text = y.Text
    End Operator
End Structure

Structure Box
    Public Value As Integer

    Public Sub New(ByVal value As Integer)
        Me.Value = value
    End Sub

    Public Shared Operator \(ByVal x As Box, ByVal y As Box) As Integer
        Return x.Value \ y.Value
    End Operator

    Public Shared Widening Operator CType(ByVal x As Box) As Integer
        Return x.Value
    End Operator

    Public Shared Narrowing Operator CType(ByVal x As Integer) As Box
        Return New Box(x)
    End Operator
End Structure

Structure Truthy
    Public Value As Integer

    Public Sub New(ByVal value As Integer)
        Me.Value = value
    End Sub

    Public Shared Operator IsTrue(ByVal x As Truthy) As Boolean
        Return x.Value > 0
    End Operator

    Public Shared Operator IsFalse(ByVal x As Truthy) As Boolean
        Return x.Value <= 0
    End Operator

    Public Shared Operator Not(ByVal x As Truthy) As Truthy
        Return New Truthy(-x.Value)
    End Operator
End Structure

Module Test
    Sub Main()
        Dim left As New PairText()
        left.Text = "10"
        Dim right As New PairText()
        right.Text = "3"
        Console.WriteLine(left & right)

        Dim x As New Box(10)
        Dim y As New Box(3)
        Console.WriteLine(x \ y)

        Dim same1 As New PairText()
        same1.Text = "same"
        Dim same2 As New PairText()
        same2.Text = "same"
        Console.WriteLine(same1 Like same2)

        Dim asInteger As Integer = CType(New Box(7), Integer)
        Console.WriteLine(asInteger)

        Dim asBox As Box = CType(4, Box)
        Console.WriteLine(asBox.Value)

        Dim truth As New Truthy(1)
        If truth Then
            Console.WriteLine("yes")
        End If

        Console.WriteLine((Not truth).Value)
    End Sub
End Module
