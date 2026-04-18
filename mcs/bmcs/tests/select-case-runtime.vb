' EXPECT: range hit 1
' EXPECT: range miss
' EXPECT: range hit 6
' EXPECT: compare hit
' EXPECT: compare miss
' EXPECT: equal hit
' EXPECT: comma hit 1
' EXPECT: comma hit 3
' EXPECT: comma miss
' EXPECT: SIDE EFFECT
' EXPECT: one
Imports System

Module Test
    ' Verifies that:
    '  (a) Case x To y works with RUNTIME values for x and y
    '  (b) Case Is <op> expr works
    '  (c) Case expr1, expr2 (comma list) works
    '  (d) Case Else fires when nothing else matches
    '  (e) The selector expression is evaluated exactly once even
    '      when multiple case clauses reference it
    Sub Describe(sel As Integer, lo As Integer, hi As Integer, tag As String)
        Select Case sel
            Case lo To hi
                Console.WriteLine(tag & " hit " & (sel - lo + 1))
            Case Else
                Console.WriteLine(tag & " miss")
        End Select
    End Sub

    Sub DescribeCompare(sel As Integer, boundary As Integer)
        Select Case sel
            Case Is > boundary
                Console.WriteLine("compare hit")
            Case Else
                Console.WriteLine("compare miss")
        End Select
    End Sub

    Sub DescribeEqual(sel As Integer, match As Integer)
        Select Case sel
            Case match
                Console.WriteLine("equal hit")
            Case Else
                Console.WriteLine("equal miss")
        End Select
    End Sub

    Sub DescribeComma(sel As Integer)
        Select Case sel
            Case 1, 2, 3
                Console.WriteLine("comma hit " & sel)
            Case Else
                Console.WriteLine("comma miss")
        End Select
    End Sub

    Dim m_Calls As Integer = 0

    Function SideEffectSelector() As Integer
        m_Calls += 1
        Console.WriteLine("SIDE EFFECT")
        Return 1
    End Function

    Sub CheckOnceEvaluation()
        m_Calls = 0
        Select Case SideEffectSelector()
            Case 0
                Console.WriteLine("zero")
            Case 1
                Console.WriteLine("one")
            Case 2
                Console.WriteLine("two")
            Case Else
                Console.WriteLine("other")
        End Select
        If m_Calls <> 1 Then
            Console.WriteLine("FAIL selector evaluated " & m_Calls & " times")
        End If
    End Sub

    Sub Main()
        ' Runtime-value range: lo=10, hi=15, sel=10 (first element, should hit)
        Describe(10, 10, 15, "range")
        ' Runtime-value range: sel=5 not in [10,15]
        Describe(5, 10, 15, "range")
        ' Runtime-value range: sel=15 (last element)
        Describe(15, 10, 15, "range")

        ' Runtime Is comparison
        DescribeCompare(10, 5)
        DescribeCompare(5, 10)

        ' Equality with runtime match value
        DescribeEqual(7, 7)

        ' Comma-list
        DescribeComma(1)
        DescribeComma(3)
        DescribeComma(99)  ' else

        ' Selector evaluated exactly once
        CheckOnceEvaluation()
    End Sub
End Module
