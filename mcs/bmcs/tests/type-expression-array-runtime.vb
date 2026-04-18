' EXPECT: System.Byte[]
' EXPECT: type-check
Imports System

Module Test
    Sub Main()
        Console.WriteLine(GetType(Byte()).FullName)

        Dim value As Object = New Byte() {1, 2}
        If TypeOf value Is Byte() Then
            Console.WriteLine("type-check")
        End If
    End Sub
End Module
