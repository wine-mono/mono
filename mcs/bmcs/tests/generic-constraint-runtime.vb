' EXPECT: True
' EXPECT: True
' EXPECT: True
' EXPECT: True
' EXPECT: True
' EXPECT: True
Imports System
Imports System.Reflection

Class Sample
    Implements IComparable

    Public Sub New()
    End Sub

    Public Function CompareTo(ByVal obj As Object) As Integer Implements IComparable.CompareTo
        Return 0
    End Function
End Class

Class Box(Of T As {Class, IComparable, New})
End Class

Module Test
    Public Function Make(Of T As {Class, IComparable, New})() As Type
        Return GetType(T)
    End Function

    Sub Main()
        Dim classParam As Type = GetType(Box(Of Sample)).GetGenericTypeDefinition().GetGenericArguments()(0)
        Dim classAttrs As GenericParameterAttributes = classParam.GenericParameterAttributes
        Console.WriteLine((CInt(classAttrs) And CInt(GenericParameterAttributes.ReferenceTypeConstraint)) <> 0)
        Console.WriteLine((CInt(classAttrs) And CInt(GenericParameterAttributes.DefaultConstructorConstraint)) <> 0)
        Console.WriteLine(classParam.GetGenericParameterConstraints()(0) Is GetType(IComparable))

        Dim methodParam As Type = GetType(Test).GetMethod("Make").GetGenericArguments()(0)
        Dim methodAttrs As GenericParameterAttributes = methodParam.GenericParameterAttributes
        Console.WriteLine((CInt(methodAttrs) And CInt(GenericParameterAttributes.ReferenceTypeConstraint)) <> 0)
        Console.WriteLine((CInt(methodAttrs) And CInt(GenericParameterAttributes.DefaultConstructorConstraint)) <> 0)
        Console.WriteLine(methodParam.GetGenericParameterConstraints()(0) Is GetType(IComparable))
    End Sub
End Module
