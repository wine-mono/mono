'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ExpConversionDecimaltoShortA
	Sub Main()
		Dim a as Decimal = 123.501 
		Dim b as Short
		b = CShort(a)
		if b <> 124
			Throw new System.Exception("Decimal to Short Conversion is not working properly. Expected 124 but got " &b)
		End if	
	End Sub
End Module
