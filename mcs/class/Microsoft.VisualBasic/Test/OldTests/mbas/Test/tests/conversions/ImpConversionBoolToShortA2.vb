Module ImpConversionofBooltoShort
	Sub Main()
		Dim b as Boolean = True
		Dim a as Short = b 
		if a <> -1 then 
			Throw New System.Exception("Implicit Conversion of Bool(True) to Short has Failed. Expected -1 got " & a)
		End if
	End Sub
End Module
