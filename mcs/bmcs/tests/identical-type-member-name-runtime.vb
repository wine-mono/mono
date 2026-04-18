' EXPECT: 18
' EXPECT: True

Public Class Compiler
    Public Shared m_Compiler As Integer = 7
    Public Shared CurrentCompiler As Compiler = New Compiler(11)

    Public Value As Integer

    Public Sub New(ByVal value As Integer)
        Me.Value = value
    End Sub
End Class

Public Class Helper
    Public ReadOnly Property Compiler() As Compiler
        Get
            Return New Compiler(-1)
        End Get
    End Property

    Public Shared Function ReadSharedCompiler() As Integer
        Return Compiler.m_Compiler + Compiler.CurrentCompiler.Value
    End Function
End Class

Public Class tm
End Class

Public Class HandlesClause
    Public Shared Function IsMe(ByVal value As tm) As Boolean
        Return value IsNot Nothing
    End Function
End Class

Public Class HandlesOwner
    Public ReadOnly Property HandlesClause() As HandlesClause
        Get
            Return Nothing
        End Get
    End Property

    Public Shared Function Check(ByVal value As tm) As Boolean
        Return HandlesClause.IsMe(value)
    End Function
End Class

Module Program
    Sub Main()
        System.Console.WriteLine(Helper.ReadSharedCompiler())
        System.Console.WriteLine(HandlesOwner.Check(New tm()))
    End Sub
End Module
