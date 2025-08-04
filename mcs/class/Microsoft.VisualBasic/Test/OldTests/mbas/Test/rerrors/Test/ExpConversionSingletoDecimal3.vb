'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

'System.OverflowException

Imports System
Imports Nunit.Framework

<TestFixture>_
Public Class ExpConversionDoubletoDecimalC
	_<Test, ExpectedException (GetType (System.OverflowException))>
        Public Sub TestForException()	
			Dim a as Decimal
			Dim b as Double = Double.NaN
			a = CDec(b)
        End Sub
End Class 
