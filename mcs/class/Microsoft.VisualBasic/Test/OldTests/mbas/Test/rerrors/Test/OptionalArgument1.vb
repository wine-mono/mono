'=============================================================================================
'Name:Manish Kumar Sinha 
'Email Address: manishkumarsinha@sify.com
'Test Case Name: Argument passing by Optional Keyword:
'O.P-1.0.0: An Optional parameter must specify a constant expression to be used a replacement
'		value if no argument is specified.When we omit one or more optional arguments in
'		 the argument list, you use successive commas to mark their positions. 
'		If we don't mark commas then it produce run time error
'=============================================================================================

Imports System
Imports Nunit.Framework

<TestFixture>_
Public Class OP1_0_0
	_<Test, ExpectedException (GetType (System.InvalidCastException))>
	Sub F(ByVal telephoneNo as Long, Optional ByVal code as Integer = 080,Optional ByVal code1  As Integer = 091, Optional ByRef name As String="Sinha")
		if (code <> 080 and code1 <> 091 and name="Sinha")
			Throw New System.Exception("#A1, Unexcepted behaviour in string of OP1_0_0")
		end if
   	End Sub 

      Public Sub TestForException()	
   		  Dim telephoneNo As Long = 9886066432
		  Dim name As String ="Manish"
    		  F(telephoneNo,name)
      End Sub
End Class 