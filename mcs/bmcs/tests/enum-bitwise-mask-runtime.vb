' EXPECT: 3
' EXPECT: 1
' EXPECT: True
' EXPECT: 7
Imports System

<Flags()> _
Enum MsgMask As Integer
    None = 0
    One = 1
    Two = 2
    Four = 4
End Enum

Module M
    Sub Main()
        Dim value As MsgMask = MsgMask.One Or MsgMask.Two

        Console.WriteLine(value And 7)
        Console.WriteLine(value And 1)
        Console.WriteLine((value And 2) = 2)
        Console.WriteLine(value Or 4)
    End Sub
End Module
