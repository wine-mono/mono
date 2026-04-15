' EXPECT: True
' EXPECT: False
' EXPECT: False
' EXPECT: True
' EXPECT: -2
' EXPECT: False

Module M
    Sub Main()
        Dim b1 As Boolean? = True
        Dim b2 As Boolean? = Nothing
        Dim i1 As Integer? = 1
        Dim i2 As Integer? = Nothing

        Dim rb1 As Boolean? = Not b1
        Dim rb2 As Boolean? = Not b2
        Dim ri1 As Integer? = Not i1
        Dim ri2 As Integer? = Not i2

        System.Console.WriteLine(rb1.HasValue)
        System.Console.WriteLine(rb1.Value)
        System.Console.WriteLine(rb2.HasValue)
        System.Console.WriteLine(ri1.HasValue)
        System.Console.WriteLine(ri1.Value)
        System.Console.WriteLine(ri2.HasValue)
    End Sub
End Module
