' EXPECT: 3
' EXPECT: 60
Imports System

Module Test
    Sub Main()
        Dim nums As Integer() = New Integer() {10, 20, 30}
        Console.WriteLine(nums.Length)

        Dim sum As Integer = 0
        For Each n As Integer In nums
            sum += n
        Next
        Console.WriteLine(sum)
    End Sub
End Module
