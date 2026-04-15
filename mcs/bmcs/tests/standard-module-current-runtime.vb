' EXPECT: 7
' EXPECT: 7
Imports System

Namespace N
    Module Helpers
        Public Function F() As Integer
            Return 7
        End Function
    End Module

    Module Test
        Sub Main()
            Console.WriteLine(F())
            Console.WriteLine(N.F())
        End Sub
    End Module
End Namespace
