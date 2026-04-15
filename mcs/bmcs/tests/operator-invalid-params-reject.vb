' EXPECT-COMPILE-CONTAINS: Operator parameters cannot be declared ByRef, Optional, or ParamArray
Structure Box
    Public Value As Integer

    Public Shared Operator +(ByRef x As Box) As Box
        Return x
    End Operator
End Structure
