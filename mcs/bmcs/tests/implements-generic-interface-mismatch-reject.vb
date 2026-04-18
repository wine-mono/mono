' EXPECT-COMPILE-CONTAINS: containing class does not implement interface
Imports System.Collections.Generic

Class Sample
    Implements IComparer(Of Integer)

    Public Function CompareInts(ByVal x As Integer, ByVal y As Integer) As Integer Implements IComparer(Of String).Compare
        Return x - y
    End Function
End Class
