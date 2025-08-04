'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ExpConversionDoubletoDecimalA
	Sub Main()
			Dim a as Decimal
			Dim b as Double = -4.94065645841247e-324
			a = CDec(b)
			if a<>-0 Then
				Throw New System.Exception("Double to Decimal Conversion is not working properly. Expected 0 but got " &a)		
		End if		
	End Sub
End Module
