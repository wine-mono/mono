' EXPECT-COMPILE-CONTAINS: accessibility modifier must be more restrictive than the property or indexer
Public Class Sample
    Private m_Value As Integer

    Public Property Value() As Integer
        Public Get
            Return m_Value
        End Get
        Set(ByVal value As Integer)
            m_Value = value
        End Set
    End Property
End Class
