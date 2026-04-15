' EXPECT: 30
' EXPECT: decorated
Imports System

' VB allows ' _' at end of line to continue the logical line
' onto the next physical line.  This file also covers the
' attribute-plus-line-continuation form on a Structure member.

Public Structure Span
    Private m_Line As Integer

    ' Line-continuation in a parameter list.
    Public Sub New(ByVal line As Integer, _
                   ByVal dummy As Integer)
        m_Line = line + dummy
    End Sub

    ' Attribute + line continuation on a struct member, exact
    ' pattern from vbnc's Tokens/Span.vb:94.
    <Obsolete("deprecated")> _
    Public Function Describe() As String
        Return "decorated"
    End Function

    Public Function Value() As Integer
        Return m_Line
    End Function
End Structure

Module Test
    Sub Main()
        Dim s As New Span(10, 20)
        Console.WriteLine(s.Value())
        Console.WriteLine(s.Describe())
    End Sub
End Module
