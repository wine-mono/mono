' EXPECT: True
Imports System
Imports System.Runtime.InteropServices

Module Test
    <DllImport("libc", EntryPoint:="getpid")> _
    Private Shared Function getpid() As Integer
    End Function

    <DllImport("libc", EntryPoint:="srand")> _
    Private Shared Sub srand(ByVal seed As Integer)
    End Sub

    Sub Main()
        srand(1)
        Console.WriteLine(getpid() > 0)
    End Sub
End Module
