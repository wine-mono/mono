' EXPECT: 0
' EXPECT: 0
' EXPECT: 0
Imports System

Module Test
    Function FromReturn() As Integer()
        Return New Integer() {}
    End Function

    Function FromAssignment() As Integer()
        Dim result() As Integer
        result = New Integer() {}
        Return result
    End Function

    Function FromDeclaration() As Integer()
        Dim result() As Integer = New Integer() {}
        Return result
    End Function

    Sub Main()
        Console.WriteLine(FromReturn().Length)
        Console.WriteLine(FromAssignment().Length)
        Console.WriteLine(FromDeclaration().Length)
    End Sub
End Module
