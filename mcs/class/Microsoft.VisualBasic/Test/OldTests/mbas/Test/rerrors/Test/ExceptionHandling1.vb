Imports System
Imports Nunit.Framework

<TestFixture>_
Public Class ExceptionHandlingB
	_<Test, ExpectedException (GetType (System.OverflowException))>
       Public Sub TestForException()		
       		dim i as integer
                i = 2 / i
                i = 3
       End Sub
End Class 
