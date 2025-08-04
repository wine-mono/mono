
'
' Author:
'   Roopa Wilson (rowilson@novell.com)
' Copyright (C) 2004 Novell, Inc (http://www.novell.com)
'
' Permission is hereby granted, free of charge, to any person obtaining
' a copy of this software and associated documentation files (the
' "Software"), to deal in the Software without restriction, including
' without limitation the rights to use, copy, modify, merge, publish,
' distribute, sublicense, and/or sell copies of the Software, and to
' permit persons to whom the Software is furnished to do so, subject to
' the following conditions:
' 
' The above copyright notice and this permission notice shall be
' included in all copies or substantial portions of the Software.
' 
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
' EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
' MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
' NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
' LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
' OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
' WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
'
Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As String
		'Begin Code
			Dim result() As String = Split ("HelloHelloHelloHello","ello")
			If result.Length <> 4 Then
				Throw New Exception ("#Split01: Expected 5 but got " + (result.Length).ToString ())
			End If
			If Len (result (0)) <> 1 Then
				Throw New Exception ("#Split02: Expected 1 but got " + Len(result (0)).ToString ())
			End If
			Return result.Length
		'End Code
	End Function
End Class
