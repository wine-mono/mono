' EXPECT: Int32
Imports System

Public Class BaseType
    Protected Function Pick() As String
        Return "non-generic"
    End Function

    Protected Function Pick(Of T)() As String
        Return GetType(T).Name
    End Function
End Class

Public Class DerivedType
    Inherits BaseType

    Public Function Run() As String
        Return MyBase.Pick(Of Integer)()
    End Function
End Class

Module Test
    Sub Main()
        Console.WriteLine(New DerivedType().Run())
    End Sub
End Module
