' EXPECT: 42
' EXPECT: hello
Imports System

Public Class Box(Of T)
    Private m_value As T
    Public Sub New(v As T)
        m_value = v
    End Sub
    Public Function Get_() As T
        Return m_value
    End Function
End Class

Module Test
    Sub Main()
        Dim ib As New Box(Of Integer)(42)
        Console.WriteLine(ib.Get_())

        Dim sb As New Box(Of String)("hello")
        Console.WriteLine(sb.Get_())
    End Sub
End Module
