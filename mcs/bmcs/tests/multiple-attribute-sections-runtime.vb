' EXPECT: 1
' EXPECT: 1

Imports System

<AttributeUsage(AttributeTargets.Method)> _
Public Class MarkAAttribute
    Inherits Attribute
End Class

<AttributeUsage(AttributeTargets.Method)> _
Public Class MarkBAttribute
    Inherits Attribute
End Class

Public Class Test
    <MarkA()> _
    <MarkB()> _
    Public Shared Sub Main()
        Console.WriteLine(GetType(Test).GetMethod("Main").GetCustomAttributes(GetType(MarkAAttribute), False).Length)
        Console.WriteLine(GetType(Test).GetMethod("Main").GetCustomAttributes(GetType(MarkBAttribute), False).Length)
    End Sub
End Class
