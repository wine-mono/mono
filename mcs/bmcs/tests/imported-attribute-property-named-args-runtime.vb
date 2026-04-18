' EXPECT: False
' EXPECT: False
Imports System
Imports System.Security.Permissions

<AttributeUsage(AttributeTargets.Class, AllowMultiple:=False, Inherited:=False)> _
Public Class A
    Inherits Attribute
End Class

<A()> _
Public Class C
End Class

Public Class D
    <SecurityPermission(SecurityAction.Demand, SerializationFormatter:=True)> _
    Public Sub M()
    End Sub
End Class

Module M
    Sub Main()
        Dim usage As AttributeUsageAttribute
        usage = CType(Attribute.GetCustomAttribute(GetType(A), GetType(AttributeUsageAttribute)), AttributeUsageAttribute)
        Console.WriteLine(usage.AllowMultiple)
        Console.WriteLine(usage.Inherited)
    End Sub
End Module
