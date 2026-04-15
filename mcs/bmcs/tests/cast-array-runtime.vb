' EXPECT: 3
' EXPECT: miss
Imports System

Module Test
    Sub Main()
        Dim boxed As Object = New Byte() {1, 2, 3}
        Dim arr As Byte() = CType(boxed, Byte())
        Console.WriteLine(arr.Length)

        Dim other As Object = "not an array"
        Dim miss As Byte() = TryCast(other, Byte())
        If miss Is Nothing Then
            Console.WriteLine("miss")
        End If
    End Sub
End Module
