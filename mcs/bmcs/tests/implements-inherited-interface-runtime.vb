' EXPECT: LRX
Interface ILeft
    Sub F()
End Interface

Interface IRight
    Sub F()
End Interface

Interface ILeftRight
    Inherits ILeft, IRight
    Shadows Sub F()
End Interface

Class Sample
    Implements ILeftRight

    Private Sub LeftF() Implements ILeft.F
        System.Console.Write("L")
    End Sub

    Private Sub RightF() Implements IRight.F
        System.Console.Write("R")
    End Sub

    Private Sub BothF() Implements ILeftRight.F
        System.Console.WriteLine("X")
    End Sub
End Class

Module Test
    Sub Main()
        DirectCast(New Sample(), ILeft).F()
        DirectCast(New Sample(), IRight).F()
        DirectCast(New Sample(), ILeftRight).F()
    End Sub
End Module
