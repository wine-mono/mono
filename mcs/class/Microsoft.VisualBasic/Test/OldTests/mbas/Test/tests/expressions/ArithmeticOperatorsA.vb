
Imports System

Module ArithmeticOperatorsA

    Sub main()

        Dim a1, a2 As Double
        a1 = 3 / 2
        If a1 <> 1.5 Then
            throw New System.Exception("#A1-RegularDivisionOperator:Failed")
        End If

        Dim b1 As Integer

        b1 = 12 \ 2.5
        If b1 <> 6 Then
            throw New System.Exception("#A2-IntegerDivisionOperator:Failed")
        End If

        b1 = 11 \ 4
        If b1 <> 2 Then
            throw New System.Exception("#A3-IntegerDivisionOperator:Failed")
        End If

        b1 = 67 \ -4
        If b1 <> -16 Then
            throw New System.Exception("#A4-IntegerDivisionOperator:Failed")
        End If

        a1 = 12 Mod 2
        If a1 <> 0 Then
            throw New System.Exception("#A5-ModOperator:Failed")
        End If

        'a1 = 12.6 Mod 5
        'If a1 <> 2.6 Then
        'throw New System.Exception("#A6-ModOperator:Failed")
        'End If

        a1 = 2 ^ 3
        If a1 <> 8 Then
            throw New System.Exception("#A7-ExponentialOperator:Failed")
        End If

        a1 = (-2) ^ 3
        If a1 <> -8 Then
            throw New System.Exception("#A8-ExponentialOperator:Failed")
        End If


    End Sub

End Module