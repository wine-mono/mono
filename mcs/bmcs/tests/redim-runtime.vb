' EXPECT: len 11
' EXPECT: sum 0
' EXPECT: len 4
' EXPECT: sum 24
' EXPECT: str len 3
' EXPECT: str all null
' EXPECT: multi len 11
' EXPECT: rect rank 2
' EXPECT: rect rows 4
' EXPECT: rect cols 3
Imports System

' VB 'ReDim target(n)' must replace target with a brand new
' array of length n+1 (VB bounds are inclusive).
'
' 'ReDim Preserve target(n)' must additionally copy the
' existing contents up to Min(oldLen, newLen).
'
' bmcs doesn't support VB-style 'arr(i)' element access, so
' this test observes effects via .Length and For Each.

Module Test
    Sub Main()
        ' Non-Preserve: fresh zero-initialized array of length 11.
        Dim nums As Integer() = Nothing
        ReDim nums(10)
        Console.WriteLine("len " & nums.Length)
        Dim s As Integer = 0
        For Each n As Integer In nums
            s += n
        Next
        Console.WriteLine("sum " & s)   ' all zeros

        ' Preserve: copy first min(old, new) elements.
        '    source = {7, 8, 9}, ReDim Preserve to 4 slots
        '    new = {7, 8, 9, 0}, sum = 24
        Dim vals As Integer() = New Integer() {7, 8, 9}
        ReDim Preserve vals(3)
        Console.WriteLine("len " & vals.Length)
        s = 0
        For Each v As Integer In vals
            s += v
        Next
        Console.WriteLine("sum " & s)

        ' Reference-type element: strings default to Nothing.
        Dim strs As String() = Nothing
        ReDim strs(2)
        Console.WriteLine("str len " & strs.Length)
        Dim all_null As Boolean = True
        For Each t As String In strs
            If t IsNot Nothing Then
                all_null = False
            End If
        Next
        If all_null Then
            Console.WriteLine("str all null")
        Else
            Console.WriteLine("str had non-null")
        End If

        ' Multi-clause ReDim: resize two arrays in one statement.
        '    a -> length 5, b -> length 6, total 11
        Dim a As Integer() = Nothing
        Dim b As Integer() = Nothing
        ReDim a(4), b(5)
        Console.WriteLine("multi len " & (a.Length + b.Length))

        ' Multi-dimensional ReDim (non-Preserve).  vbnc itself
        ' uses this form in TypeResolution.Init: the upper bound
        ' is inclusive per dimension, so ReDim rect(3, 2) gives
        ' a 4x3 array (rows = 3+1, cols = 2+1).
        Dim rect As Integer(,) = Nothing
        ReDim rect(3, 2)
        Console.WriteLine("rect rank " & rect.Rank)
        Console.WriteLine("rect rows " & rect.GetLength(0))
        Console.WriteLine("rect cols " & rect.GetLength(1))
    End Sub
End Module
