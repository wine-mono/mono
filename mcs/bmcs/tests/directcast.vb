' EXPECT: hi
' EXPECT: 42
' EXPECT: InvalidCastException
' EXPECT: NullReferenceException
Imports System

Module Test
    Sub Main()
        Dim o As Object = "hi"
        Dim s As String = DirectCast(o, String)
        Console.WriteLine(s)

        Dim i As Object = 42
        Dim n As Integer = DirectCast(i, Integer)
        Console.WriteLine(n)

        Try
            Dim boxedShort As Object = CShort(1)
            Dim bad As Integer = DirectCast(boxedShort, Integer)
            Console.WriteLine(bad)
        Catch ex As InvalidCastException
            Console.WriteLine(ex.GetType().Name)
        End Try

        Try
            Dim missing As Object = Nothing
            Dim emptyValue As Integer = DirectCast(missing, Integer)
            Console.WriteLine(emptyValue)
        Catch ex As NullReferenceException
            Console.WriteLine(ex.GetType().Name)
        End Try
    End Sub
End Module
