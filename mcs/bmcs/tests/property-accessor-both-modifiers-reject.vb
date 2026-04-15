' EXPECT-COMPILE-CONTAINS: cannot specify accessibility modifiers for both accessors of the property or indexer
Public Class Sample
    Private m_Value As Integer

    Public Property Value() As Integer
        Private Get
            Return m_Value
        End Get
        Protected Set(ByVal value As Integer)
            m_Value = value
        End Set
    End Property
End Class
