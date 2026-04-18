' EXPECT-COMPILE-CONTAINS: have inconsistent constraints for type parameter `T'
Partial Class Box(Of T As Class)
End Class

Partial Class Box(Of T As Structure)
End Class
