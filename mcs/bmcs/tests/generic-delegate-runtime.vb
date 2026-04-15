' EXPECT: 7
' EXPECT: True
' EXPECT: True
Imports System
Imports System.Reflection

Delegate Function Id(Of T)(x As T) As T
Delegate Function Factory(Of T As {Class, New})() As T

Class Box
    Public Sub New()
    End Sub
End Class

Module Test
    Private Function EchoInt(x As Integer) As Integer
        Return x
    End Function

    Sub Main()
        Dim idInt As Id(Of Integer) = New Id(Of Integer)(AddressOf EchoInt)
        Console.WriteLine(idInt(7))

        Dim factoryParam As Type = GetType(Factory(Of Box)).GetGenericTypeDefinition().GetGenericArguments()(0)
        Dim attrs As GenericParameterAttributes = factoryParam.GenericParameterAttributes
        Console.WriteLine((CInt(attrs) And CInt(GenericParameterAttributes.ReferenceTypeConstraint)) <> 0)
        Console.WriteLine((CInt(attrs) And CInt(GenericParameterAttributes.DefaultConstructorConstraint)) <> 0)
    End Sub
End Module
