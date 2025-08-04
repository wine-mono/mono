'Checks if Default is working or not with Overrides...It works

Imports System

Class base
	Public Default Overridable ReadOnly Property Item(ByVal i as Integer)As Integer
		Get			
			Return i
		End Get
	End Property
End Class

Class derive
	Inherits base
	Public Default Overrides ReadOnly Property Item(ByVal i as Integer)As Integer
		Get			
			Return 2*i
		End Get
	End Property
End Class

Module DefaultA
	Sub Main()
		Dim a as Object=new derive()
		Dim i as Integer	
		i=a(10)
		if i<>20 Then
			Throw New Exception("Default Not Working")
		End If
	End Sub
End Module
