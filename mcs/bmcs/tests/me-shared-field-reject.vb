' EXPECT-COMPILE-CONTAINS: not valid in static code
Class Sample
    Private Shared selfRef As Sample = Me
End Class
