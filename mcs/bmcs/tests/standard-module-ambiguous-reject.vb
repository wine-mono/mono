' EXPECT-COMPILE-CONTAINS: is an ambiguous reference
Imports System

Namespace N
    Module A
        Public Function F() As Integer
            Return 1
        End Function
    End Module

    Module B
        Public Function F() As Integer
            Return 2
        End Function
    End Module

    Module Test
        Sub Main()
            Console.WriteLine(F())
        End Sub
    End Module
End Namespace
