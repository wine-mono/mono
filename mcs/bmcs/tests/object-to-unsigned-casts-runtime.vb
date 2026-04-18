' EXPECT: 5
' EXPECT: 6
' EXPECT: 7
Imports System

Module Test
    Sub Main()
        Dim us As Object = CShort(5)
        Dim ui As Object = 6
        Dim ul As Object = 7

        Console.WriteLine(CUShort(us))
        Console.WriteLine(CUInt(ui))
        Console.WriteLine(CULng(ul))
    End Sub
End Module
