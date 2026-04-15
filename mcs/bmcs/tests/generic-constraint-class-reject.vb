' EXPECT-COMPILE-CONTAINS: must be a reference type in order to use it as type parameter `T'
Class Box(Of T As Class)
End Class

Module Test
    Sub Main()
        Dim bad As Box(Of Integer) = Nothing
    End Sub
End Module
