' EXPECT: 7
Imports System

Public Structure Token
    Private m_Line As Integer

    Public Sub New(ByVal line As Integer)
        m_Line = line
    End Sub

    Public ReadOnly Property Line() As Integer
        Get
            Return m_Line
        End Get
    End Property
End Structure

Module Test
    Sub Main()
        Dim t As New Token(7)
        Console.WriteLine(t.Line)
    End Sub
End Module
