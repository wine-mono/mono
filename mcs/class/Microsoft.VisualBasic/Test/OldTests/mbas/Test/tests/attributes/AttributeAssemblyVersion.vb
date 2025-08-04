'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)

Imports System
Imports System.Reflection
Imports System.Runtime.CompilerServices

<assembly: AssemblyVersion("3.2.1.0")>

Module Test
Sub Main()
 	
	dim asm as System.Reflection.AssemblyName
	dim i as integer
	asm = System.Reflection.Assembly.GetCallingAssembly ().GetName ()
	if asm.Version.ToString() <> "3.2.1.0" then
		Throw New Exception ("Expected Version No. 3.2.1.0")
	End if
End Sub
End Module
