' EXPECT: decimal
' EXPECT: date
' EXPECT: string
Module Test
    Sub Main()
        Dim high As Decimal = 5D
        Dim low As Decimal = 3D
        Dim laterDate As Date = #1/2/2000#
        Dim earlierDate As Date = #1/1/2000#
        Dim highString As String = "b"
        Dim lowString As String = "a"

        If high > low Then
            System.Console.WriteLine("decimal")
        End If

        If laterDate > earlierDate Then
            System.Console.WriteLine("date")
        End If

        If highString > lowString Then
            System.Console.WriteLine("string")
        End If
    End Sub
End Module
