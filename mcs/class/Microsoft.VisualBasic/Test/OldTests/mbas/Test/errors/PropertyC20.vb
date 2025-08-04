'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 15
REM ExpectedError: BC31065
REM ErrorMessage:  'Set' parameter cannot be declared 'Optional'.

option strict
Imports System

Class AB
	Public writeonly Property Prop(a as Integer) as Integer
		set (Optional Byval i as Integer = 20)
		End set
      End Property
End Class

Module Test
      Public Sub Main()		
	End Sub
End Module

 
