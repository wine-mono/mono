' EXPECT: 1
' EXPECT: 2
' EXPECT: 3
' EXPECT: a
' EXPECT: b
Imports System
Imports System.Collections.Generic

Module Test
    Sub Main()
        Dim nums As New List(Of Integer)
        nums.Add(1)
        nums.Add(2)
        nums.Add(3)

        ' Inline typed form: For Each x As Integer In nums
        For Each x As Integer In nums
            Console.WriteLine(x)
        Next

        ' Pre-declared form: Dim s, then For Each s In ...
        Dim strs As New List(Of String)
        strs.Add("a")
        strs.Add("b")
        Dim s As String
        For Each s In strs
            Console.WriteLine(s)
        Next
    End Sub
End Module
