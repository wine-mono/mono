REM LineNo: 7
REM ExpectedError: BC30176
REM ErrorMessage: Only one of 'Public', 'Private', 'Protected', 'Friend', or 'Protected Friend' can be specified.

Imports System
Class C1
	Private Protected d As Integer
End Class
Module Accessibility
	Sub Main()
	End Sub
End Module
