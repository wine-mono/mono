  '
  ' Copyright (c) 2002-2003 Mainsoft Corporation.
  '
  ' Permission is hereby granted, free of charge, to any person obtaining a
  ' copy of this software and associated documentation files (the "Software"),
  ' to deal in the Software without restriction, including without limitation
  ' the rights to use, copy, modify, merge, publish, distribute, sublicense,
  ' and/or sell copies of the Software, and to permit persons to whom the
  ' Software is furnished to do so, subject to the following conditions:
  ' 
  ' The above copyright notice and this permission notice shall be included in
  ' all copies or substantial portions of the Software.
  ' 
  ' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
  ' IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
  ' FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
  ' AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
  ' LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
  ' FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
  ' DEALINGS IN THE SOFTWARE.
  '
Imports System
Imports Microsoft.VisualBasic
Public Class TestClass
    Public Function Test() As String
        Dim i As Integer
        Dim oCa As New Ca()
        Dim str1 As String
        Dim caughtException As Boolean
        '// Invalid UseCallType value; must be Method, Get, or Set.
        caughtException = False
        Try
            str1 = CallByName(oCa, "foo", 10, "val")
        Catch e As ArgumentException
            If Err.Number = 5 Then
                caughtException = True
            End If
        End Try
        If caughtException = False Then Return "failed at sub test 1"
        Return "success"
    End Function
End Class
Public Class Ca
    Private m_value As String
    Public Property prop() As String
        Get
            Return m_value
        End Get
        Set(ByVal Value As String)
            m_value = Value
        End Set
    End Property
    Public Function foo(ByVal var1 As String, ByVal var2 As String) As String
        Return "foo" & m_value & var1 & var2
    End Function
End Class