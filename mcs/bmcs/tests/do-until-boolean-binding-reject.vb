' EXPECT-COMPILE-CONTAINS: Can not convert the expression to a boolean
' EXPECT-COMPILE-NOT-CONTAINS: Operator ! cannot be applied
Imports System

' 'Until' negates the Boolean-expression result, not the source expression.
' This should report only the normal Boolean-expression failure for Integer,
' not an extra bogus unary-operator error from lowering to raw 'Not i'.
Module Test
    Sub Main()
        Dim i As Integer = 0
        Do Until i
            Console.WriteLine(i)
            i += 1
        Loop
    End Sub
End Module
