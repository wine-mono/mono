'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Imports System.Threading
Imports System.Globalization

Module ExpConversionStringtoSingleA
	Sub Main()
			Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US")

			Dim a as Single
			Dim b as String= "123.5"
			a = CSng(b)
			if a <> 123.5
				Throw new System.Exception("Conversion of String to Single not working. Expected 123.5 but got " &a) 
			End if		
	End Sub
End Module
