REM LineNo: 29
REM ExpectedError: BC30183
REM ErrorMessage: Keyword is not valid as an identifier.

Imports System
                                                                                
Class C
        Public Event E
                                                                                
        Public Sub S()
                RaiseEvent E
        End Sub
End Class
                                                                                
Class C1

	Inherits C

	Public Event E1

        Sub call_S()
                S()
        End Sub
                                                                                
        Sub EH() Handles MyBase.E
                Console.WriteLine("event-H called")
        End Sub

	Sub EH1() Handles Me.E1
	End Sub
End Class

Module M
        Sub Main()
                dim y as new C1 ()
                y.call_S()
        End Sub
End Module
