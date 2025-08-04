' ErrorMessage: System.ArrayTypeMismatchException: Attempted to store
' an element of the incorrect type into the array.
                                                                                
Imports System
Imports Nunit.Framework
                                                                                
<TestFixture> _
Public Class AssignmentStatements1
                                                                                
                <Test, ExpectedException (GetType (System.ArrayTypeMismatchException))> _
                Public Sub TestArrayTypeMismatch ()
                        Dim sa(10) As String
                        Dim oa As Object() = sa
                        oa(0) = Nothing
                        oa(1) = "Hello "
                        oa(2) = "World"
                        oa(3) = New Date(2004, 12, 7)
                                                                                
            End Sub
End Class









