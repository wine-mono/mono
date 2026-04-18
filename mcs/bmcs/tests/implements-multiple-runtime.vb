' EXPECT: ok
' EXPECT: ok
Interface ILeft
    Sub F()
End Interface

Interface IRight
    Sub F()
End Interface

Class Sample
    Implements ILeft, IRight

    Private Sub Emit() Implements ILeft.F, IRight.F
        System.Console.WriteLine("ok")
    End Sub
End Class

Module Test
    Sub Main()
        DirectCast(New Sample(), ILeft).F()
        DirectCast(New Sample(), IRight).F()
    End Sub
End Module
