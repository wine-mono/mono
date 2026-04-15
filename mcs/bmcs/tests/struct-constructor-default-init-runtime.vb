' EXPECT: 5
' EXPECT: True
' EXPECT: True
Imports System

Public Structure S
	Public A As Integer
	Public B As String

	Public Sub New(ByVal x As Integer)
		If Me.B Is Nothing Then
			A = x
		End If
	End Sub
End Structure

Module Test
	Sub Main()
		Dim s As New S(5)
		Console.WriteLine(s.A)
		Console.WriteLine(s.B Is Nothing)
		Console.WriteLine(New S(5).B Is Nothing)
	End Sub
End Module
