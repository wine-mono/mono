REM LineNo: 202
REM ExpectedError: BC30002
REM ErrorMessage: Type 'Int' is not defined.

REM LineNo: 102
REM ExpectedError: BC30002
REM ErrorMessage: Type 'Int' is not defined.

REM LineNo: 36
REM ExpectedError: BC30002
REM ErrorMessage: Type 'Int' is not defined.

' Force an error to visually examine whether errors within #ExternalSource directives are properly reported

Imports System
Module ExternalSourceDirective
	Sub Main()
		#ExternalSource("/home/main.aspx",100)
		Dim I As Integer
		'Compiler should report error in "/home/main.aspx" at line 102 	
		Dim B As Int	
		#End ExternalSource
	End Sub


	Sub A()
		#ExternalSource("/home/a.aspx",200)
		Dim I As Integer
		'Compiler should report error in "/home/a.aspx" at line 202
		Dim B As Int	
		#End ExternalSource		
	End Sub

	Sub C()
		'Compiler should report error in "ExternalSourceDirectivesC4.vb" at line 24
		Dim B As Int
	End Sub
End Module
