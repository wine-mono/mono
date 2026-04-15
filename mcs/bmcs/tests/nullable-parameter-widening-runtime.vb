Imports System

MustInherit Class E
End Class

Module M
    Function TakeBool(ByVal value As Boolean?) As Boolean
        Return value.HasValue AndAlso value.Value
    End Function

    Function TakeLong(ByVal value As Long?) As Long
        Return value.Value
    End Function

    Function TakeRefBool(ByVal marker As Integer, ByRef value As E, ByVal flag As Boolean?) As Boolean
        Return marker = 1 AndAlso value Is Nothing AndAlso flag.HasValue AndAlso flag.Value
    End Function

    Sub Main()
        Console.WriteLine(TakeBool(True))
        Console.WriteLine(TakeLong(1) = 1)
        Console.WriteLine(TakeRefBool(1, Nothing, True))
    End Sub
End Module
