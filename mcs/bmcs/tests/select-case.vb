' EXPECT: two
' EXPECT: small
' EXPECT: big
' EXPECT: other
Imports System

Module Test
    Sub Describe(x As Integer)
        Select Case x
            Case 1
                Console.WriteLine("one")
            Case 2
                Console.WriteLine("two")
            Case 3, 4, 5
                Console.WriteLine("small")
            Case 100
                Console.WriteLine("big")
            Case Else
                Console.WriteLine("other")
        End Select
    End Sub

    Sub Main()
        Describe(2)
        Describe(4)
        Describe(100)
        Describe(42)
    End Sub
End Module
