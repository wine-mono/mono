' PREPROCESS: -D NET_VER=2.0
' EXPECT: 2
Imports System
Imports System.ComponentModel
Imports System.Security.Permissions

Module Test
#If NET_VER >= 4.0 Then
    <EditorBrowsable(EditorBrowsableState.Advanced), SecurityPermission(SecurityAction.Demand, SerializationFormatter:=True)> _
    Public Function Value() As Integer
        Return 4
    End Function
#Else
    Public Function Value() As Integer
        Return 2
    End Function
#End If

    Sub Main()
        Console.WriteLine(Value())
    End Sub
End Module
