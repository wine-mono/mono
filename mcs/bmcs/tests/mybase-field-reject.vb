' EXPECT-COMPILE-CONTAINS: Keyword base is not available in the current context
Class Base
    Protected value As Integer = 1
End Class

Class Sample
    Inherits Base
    Private copy As Integer = MyBase.value
End Class
