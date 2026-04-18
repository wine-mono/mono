' EXPECT-COMPILE-CONTAINS: field initializer cannot reference the non-static field
Class Sample
    Private value As Integer = 1
    Private Shared copy As Integer = value
End Class
