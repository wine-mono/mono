' EXPECT: True
' EXPECT: True
Imports System
Imports System.Reflection

Public Class Sample
    Private m_Value As Integer

    Public Property Value() As Integer
        Get
            Return m_Value
        End Get
        Protected Friend Set(ByVal value As Integer)
            m_Value = value
        End Set
    End Property
End Class

Module Test
    Sub Main()
        Dim p As PropertyInfo = GetType(Sample).GetProperty("Value")

        Console.WriteLine(p.GetGetMethod(True).IsPublic)
        Console.WriteLine(p.GetSetMethod(True).IsFamilyOrAssembly)
    End Sub
End Module
