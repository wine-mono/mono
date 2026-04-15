' EXPECT: ok
Imports System
Imports System.Collections

' This file covers declaration of a property with both 'Default'
' and 'Shadows'.  It stops at declaration rather than invocation.
Public Class MyList
    Inherits ArrayList

    Default Public Shadows Property Value(ByVal index As Integer) As String
        Get
            Return "got"
        End Get
        Set(ByVal v As String)
        End Set
    End Property
End Class

Module Test
    Sub Main()
        Dim m As New MyList
        Dim unused As MyList = m
        Console.WriteLine("ok")
    End Sub
End Module
