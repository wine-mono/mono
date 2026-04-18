' EXPECT: 1024
' EXPECT: 5
Module M
    Sub Main()
        Dim buffer(1023) As Byte
        Dim sourceLength As Integer = 5
        Dim temp(sourceLength - 1) As String

        System.Console.WriteLine(buffer.Length)
        System.Console.WriteLine(temp.Length)
    End Sub
End Module
