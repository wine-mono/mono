' EXPECT-COMPILE-CONTAINS: The new() constraint must be last.
Interface IFoo
End Interface

Class Bad(Of T As {New, IFoo})
End Class
