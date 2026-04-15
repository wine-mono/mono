' EXPECT: System.Boolean
' EXPECT: False

Module M
    Sub Main()
        Dim o As Object = True
        Dim r = Not o

        System.Console.WriteLine(r.GetType().FullName)
        System.Console.WriteLine(r)
    End Sub
End Module
