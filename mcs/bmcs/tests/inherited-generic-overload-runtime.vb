' EXPECT: generic
Imports System

Class ParsedObject
End Class

Class AttributeArgumentExpression
    Inherits ParsedObject
End Class

Class BaseList(Of T As ParsedObject)
    Function Add(ByVal item As T) As T
        Console.WriteLine("generic")
        Return item
    End Function
End Class

Class AttributePositionalArgumentList
    Inherits BaseList(Of AttributeArgumentExpression)

    Overloads Sub Add(ByVal constant As Object)
        Console.WriteLine("object")
    End Sub

    Sub Test()
        Dim value As New AttributeArgumentExpression()
        Add(value)
    End Sub
End Class

Module Test
    Sub Main()
        Dim value As New AttributePositionalArgumentList()
        value.Test()
    End Sub
End Module
