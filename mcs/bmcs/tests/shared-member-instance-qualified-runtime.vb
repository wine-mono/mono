'
' EXPECT: 12
' EXPECT: 22
' EXPECT: 30
' EXPECT: 0
Imports System

Public Class C
	Public Shared F As Integer
	Private Shared backing As Integer

	Public Shared Property P() As Integer
		Get
			Return backing
		End Get
		Set(ByVal value As Integer)
			backing = value
		End Set
	End Property

	Public Shared Function M() As Integer
		Return 30
	End Function
End Class

Module Test
	Private evals As Integer

	Private Function MakeC() As C
		evals += 1
		Return New C()
	End Function

	Sub Main()
		C.F = 11
		C.P = 21

		MakeC().F = 12
		MakeC().P = 22

		Console.WriteLine(MakeC().F)
		Console.WriteLine(MakeC().P)
		Console.WriteLine(MakeC().M())
		Console.WriteLine(evals)
	End Sub
End Module
