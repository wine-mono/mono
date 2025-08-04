'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ImpConversionDecimaltoSingleA
	Sub Main()
		Dim a as Decimal = 123.5
		Dim b as Single = a
		if b<>123.5 then 
			Throw New System.Exception("Implicit Conversion of Long to Single has Failed. Expected 123.5 but got " &b)
		End if		
	End Sub
End Module
