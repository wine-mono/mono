REM LineNo: 21
REM ExpectedError: BC30591
REM ErrorMessage:  'WithEvents' variable does not raise any events.

Imports System

Class C
	Public Event E

	Public Sub S()
		RaiseEvent E
	End Sub
End Class

Class C1
	dim WithEvents x as Object = new C()
	Sub call_S()
		x.S()
	End Sub

	Sub EH() Handles x.E
		Console.WriteLine("event called")
	End Sub
End Class

Module M
	Sub Main()
		dim y as new C1
		y.call_S()
	End Sub
End Module
