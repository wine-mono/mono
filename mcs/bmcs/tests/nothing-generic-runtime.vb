' EXPECT: 0
' EXPECT: True

Module M
    Function F(Of T)() As T
        Return Nothing
    End Function

    Sub Main()
        System.Console.WriteLine(F(Of Integer)())
        System.Console.WriteLine(F(Of String)() Is Nothing)
    End Sub
End Module
