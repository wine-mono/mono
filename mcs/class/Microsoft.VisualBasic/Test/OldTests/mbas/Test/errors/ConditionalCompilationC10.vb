REM LineNo: 10
REM ExpectedError: BC30012
REM ErrorMessage: '#If' block must end with a matching '#End If'.

Imports System
Module ConditionalCompilation
	Sub Main()
		Console.WriteLine("Hello World 1")
	End Sub
#If False
	Sub A()
		Console.WriteLine("Hello World 2")
	End Sub
#ElseIf True
	Sub B()
		Console.WriteLine("Hello World 3")
	End Sub
#Else
	Sub C()
		Console.WriteLine("Hello World 4")
	End Sub
End Module
