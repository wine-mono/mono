Module M
	private i as integer

	public Property p() as Integer
		GET
			return i
		END GET

		SET (ByVal val as Integer)
			i = val
		End SET

	End Property

	Sub Main()
		p = 10
		if p<>10 
			throw new System.Exception("#A1 Property Not Working")
		end if
	End Sub

End Module
