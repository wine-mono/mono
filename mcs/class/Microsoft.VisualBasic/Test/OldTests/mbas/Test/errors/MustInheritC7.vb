REM LineNo: 12
REM ExpectedError: BC31404
REM ErrorMessage: 'Public Function F1() As Integer' cannot shadow a method declared 'MustOverride'.

MustInherit Class C1
    Public MustOverride Function F1() As Integer
End Class

'Omitting keyword 'overrides' in a class that inherits the mustinherit class
Class C3
    Inherits C1
        Public Function F1() As Integer
        End Function
End Class

Module MustInheritC2
    Sub Main()
    End Sub
End Module


