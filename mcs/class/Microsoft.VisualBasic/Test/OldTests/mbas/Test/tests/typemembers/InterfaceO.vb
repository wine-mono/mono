'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Imports System

Interface A
	Function A1()
End Interface

Class B
	Implements A
	Public Function A1() implements A.A1
	End Function
End Class

Module A2
	Sub Main()
		Dim x as A
		Dim x1 as B= new B()
		x1 = x
	End Sub
End Module
